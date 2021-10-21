using PTHPlayer.HTSP.Helpers;
using PTHPlayer.HTSP.HTSP_Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PTHPlayer.HTSP
{
    public class StateLenghtObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 4;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
    }

    public class StateDataObject
    {
        public StateDataObject(int bufferSize)
        {
            BufferSize = bufferSize;
            buffer = new byte[BufferSize];
        }
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public int BufferSize = 0;
        // Receive buffer.  
        public byte[] buffer = new byte[0];
    }

    public class HTSConnectionAsync
    {
        private const long BytesPerGiga = 1024 * 1024 * 1024;

        private volatile bool _needsRestart = false;
        private volatile bool _connected;
        private volatile int _seq = 0;

        private readonly object _lock;
        private readonly HTSConnectionListener _listener;
        private readonly string _clientName;
        private readonly string _clientVersion;

        private int _serverProtocolVersion;
        private string _servername;
        private string _serverversion;
        private string _diskSpace;

        private readonly ByteList _buffer;
        private readonly SizeQueue<HTSMessage> _receivedMessagesQueue;
        private readonly SizeQueue<HTSMessage> _messagesForSendQueue;
        private readonly Dictionary<int, HTSResponseHandler> _responseHandlers;

        Task _connectionTask;
        Task _receiveTask;
        Task _messageTask;
        Task _sendingTask;
        Task _distributorTask;

        private Socket _socket = null;

        public HTSConnectionAsync(HTSConnectionListener listener, String clientName, String clientVersion)
        {
            _connected = false;
            _lock = new object();

            _listener = listener;
            _clientName = clientName;
            _clientVersion = clientVersion;

            _buffer = new ByteList();
            _receivedMessagesQueue = new SizeQueue<HTSMessage>(int.MaxValue);
            _messagesForSendQueue = new SizeQueue<HTSMessage>(int.MaxValue);
            _responseHandlers = new Dictionary<int, HTSResponseHandler>();
        }

        public void Stop()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.Close();
            }
            _needsRestart = true;
            _connected = false;
        }

        public bool NeedsRestart()
        {
            return _needsRestart;
        }

        bool Connected()
        {
            if (_socket != null)
            {
                bool poll = _socket.Poll(1000, SelectMode.SelectRead);
                bool data = _socket.Available == 0;
                if (poll && data)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public void Open(string hostname, int port)
        {
            if (_connectionTask != null && _connectionTask.Status == TaskStatus.Running)
            {
                return;
            }

            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostname, out ipAddress))
            {
                // no IP --> ask DNS
                IPHostEntry ipHostInfo = Dns.GetHostEntry(hostname);
                ipAddress = ipHostInfo.AddressList[0];
            }

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            var tokenSource = new CancellationTokenSource();
            _connectionTask = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Connected())
                        {
                            await Task.Delay(2000);
                            continue;
                        }
                        else
                        {
                            //Handle Disconnect
                        }
                        
                        if(_socket != null)
                        {
                            tokenSource.Cancel();
                            //Reconnect
                        }

                        //Reset token
                        tokenSource = new CancellationTokenSource();

                        _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                        // connect to server
                        _socket.Connect(remoteEP);

                        _receiveTask = ReceiveHandler(tokenSource.Token);
                        _messageTask = MessageBuilder(tokenSource.Token);
                        _distributorTask = MessageDistributor(tokenSource.Token);
                        _sendingTask = SendingHandler(tokenSource.Token);

                        //Handle Connect
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(2000);
                    }
                }
            });
        }


        public bool Authenticate(String username, String password)
        {
            //_logger.Info("[TVHclient] HTSConnectionAsync.authenticate: start");

            HTSMessage helloMessage = new HTSMessage();
            helloMessage.Method = "hello";
            helloMessage.putField("clientname", _clientName);
            helloMessage.putField("clientversion", _clientVersion);
            helloMessage.putField("htspversion", HTSMessage.HTSP_VERSION);
            helloMessage.putField("username", username);

            LoopBackResponseHandler loopBackResponseHandler = new LoopBackResponseHandler();
            sendMessage(helloMessage, loopBackResponseHandler);
            HTSMessage helloResponse = loopBackResponseHandler.getResponse();
            if (helloResponse != null)
            {
                if (helloResponse.containsField("htspversion"))
                {
                    _serverProtocolVersion = helloResponse.getInt("htspversion");
                }
                else
                {
                    _serverProtocolVersion = -1;
                    //_logger.Info("[TVHclient] HTSConnectionAsync.authenticate: hello don't deliver required field 'htspversion' - htsp wrong implemented on tvheadend side.");
                }

                if (helloResponse.containsField("servername"))
                {
                    _servername = helloResponse.getString("servername");
                }
                else
                {
                    _servername = "n/a";
                    //_logger.Info("[TVHclient] HTSConnectionAsync.authenticate: hello don't deliver required field 'servername' - htsp wrong implemented on tvheadend side.");
                }

                if (helloResponse.containsField("serverversion"))
                {
                    _serverversion = helloResponse.getString("serverversion");
                }
                else
                {
                    _serverversion = "n/a";
                    //_logger.Info("[TVHclient] HTSConnectionAsync.authenticate: hello don't deliver required field 'serverversion' - htsp wrong implemented on tvheadend side.");
                }

                byte[] salt = null;
                if (helloResponse.containsField("challenge"))
                {
                    salt = helloResponse.getByteArray("challenge");
                }
                else
                {
                    salt = new byte[0];
                    //_logger.Info("[TVHclient] HTSConnectionAsync.authenticate: hello don't deliver required field 'challenge' - htsp wrong implemented on tvheadend side.");
                }

                byte[] digest = SHA1helper.GenerateSaltedSHA1(password, salt);
                HTSMessage authMessage = new HTSMessage();
                authMessage.Method = "authenticate";
                authMessage.putField("username", username);
                authMessage.putField("digest", digest);
                sendMessage(authMessage, loopBackResponseHandler);
                HTSMessage authResponse = loopBackResponseHandler.getResponse();
                if (authResponse != null)
                {
                    Boolean auth = authResponse.getInt("noaccess", 0) != 1;
                    if (auth)
                    {
                        HTSMessage getDiskSpaceMessage = new HTSMessage();
                        getDiskSpaceMessage.Method = "getDiskSpace";
                        sendMessage(getDiskSpaceMessage, loopBackResponseHandler);
                        HTSMessage diskSpaceResponse = loopBackResponseHandler.getResponse();
                        if (diskSpaceResponse != null)
                        {
                            long freeDiskSpace = -1;
                            long totalDiskSpace = -1;
                            if (diskSpaceResponse.containsField("freediskspace"))
                            {
                                freeDiskSpace = diskSpaceResponse.getLong("freediskspace") / BytesPerGiga;
                            }
                            else
                            {
                                //_logger.Info("[TVHclient] HTSConnectionAsync.authenticate: getDiskSpace don't deliver required field 'freediskspace' - htsp wrong implemented on tvheadend side.");
                            }
                            if (diskSpaceResponse.containsField("totaldiskspace"))
                            {
                                totalDiskSpace = diskSpaceResponse.getLong("totaldiskspace") / BytesPerGiga;
                            }
                            else
                            {
                                //_logger.Info("[TVHclient] HTSConnectionAsync.authenticate: getDiskSpace don't deliver required field 'totaldiskspace' - htsp wrong implemented on tvheadend side.");
                            }

                            _diskSpace = freeDiskSpace + "GB / " + totalDiskSpace + "GB";
                        }

                        HTSMessage enableAsyncMetadataMessage = new HTSMessage();
                        enableAsyncMetadataMessage.Method = "enableAsyncMetadata";
                        sendMessage(enableAsyncMetadataMessage, null);
                    }

                    //_logger.Info("[TVHclient] HTSConnectionAsync.authenticate: authenticated = " + auth);
                    return auth;
                }
            }
            //_logger.Error("[TVHclient] HTSConnectionAsync.authenticate: no hello response");
            return false;
        }

        public int getServerProtocolVersion()
        {
            return _serverProtocolVersion;
        }

        public string getServername()
        {
            return _servername;
        }

        public string getServerversion()
        {
            return _serverversion;
        }

        public string getDiskspace()
        {
            return _diskSpace;
        }

        public void sendMessage(HTSMessage message, HTSResponseHandler responseHandler)
        {
            // loop the sequence number
            if (_seq == int.MaxValue)
            {
                _seq = int.MinValue;
            }
            else
            {
                _seq++;
            }

            // housekeeping verry old response handlers
            if (_responseHandlers.ContainsKey(_seq))
            {
                _responseHandlers.Remove(_seq);
            }

            message.putField("seq", _seq);
            _messagesForSendQueue.Enqueue(message);
            _responseHandlers.Add(_seq, responseHandler);
        }

        Task SendingHandler(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        HTSMessage message = _messagesForSendQueue.Dequeue();
                        byte[] data2send = message.BuildBytes();
                        int bytesSent = _socket.Send(data2send);
                        if (bytesSent != data2send.Length)
                        {
                            //_logger.Error("[TVHclient] SendingHandler: Sending not complete! \nBytes sent: " + bytesSent + "\nMessage bytes: " +
                            //    data2send.Length + "\nMessage: " + message.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        //_logger.Error("[TVHclient] SendingHandler caught exception : {0}", ex.ToString());
                        if (_listener != null)
                        {
                            _listener.onError(ex);
                        }
                        else
                        {
                            //_logger.ErrorException("[TVHclient] SendingHandler caught exception : {0} but no error listener is configured!!!", ex, ex.ToString());
                        }
                    }
                }
            });
        }

        Task ReceiveHandler(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                byte[] readBuffer = new byte[8192];
                while (!cancellationToken.IsCancellationRequested)
                {

                    try
                    {
                        int bytesReveived = _socket.Receive(readBuffer);
                        _buffer.appendCount(readBuffer, bytesReveived);
                    }
                    catch (Exception ex)
                    {
                        if (_listener != null)
                        {
                            Task.Factory.StartNew(() => _listener.onError(ex));
                        }
                        else
                        {
                            //_logger.ErrorException("[TVHclient] ReceiveHandler caught exception : {0} but no error listener is configured!!!", ex, ex.ToString());
                        }
                    }
                }
            });
        }

        Task MessageBuilder(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        byte[] lengthInformation = _buffer.getFromStart(4);
                        long messageDataLength = HTSMessage.uIntToLong(lengthInformation[0], lengthInformation[1], lengthInformation[2], lengthInformation[3]);
                        byte[] messageData = _buffer.extractFromStart((long)messageDataLength + 4); // should be long !!!
                        HTSMessage response = HTSMessage.parse(messageData);//, //_logger);
                        _receivedMessagesQueue.Enqueue(response);
                    }
                    catch (Exception ex)
                    {
                        if (_listener != null)
                        {
                            _listener.onError(ex);
                        }
                        else
                        {
                            //_logger.ErrorException("[TVHclient] MessageBuilder caught exception : {0} but no error listener is configured!!!", ex, ex.ToString());
                        }
                    }
                }
            });
        }

        Task MessageDistributor(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        HTSMessage response = _receivedMessagesQueue.Dequeue();
                        if (response.containsField("seq"))
                        {
                            int seqNo = response.getInt("seq");
                            if (_responseHandlers.ContainsKey(seqNo))
                            {
                                HTSResponseHandler currHTSResponseHandler = _responseHandlers[seqNo];
                                if (currHTSResponseHandler != null)
                                {
                                    _responseHandlers.Remove(seqNo);
                                    currHTSResponseHandler.handleResponse(response);
                                }
                            }
                            else
                            {
                                //_logger.Fatal("[TVHclient] MessageDistributor: HTSResponseHandler for seq = '" + seqNo + "' not found!");
                            }
                        }
                        else
                        {
                            // auto update messages
                            if (_listener != null)
                            {
                                _listener.onMessage(response);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        if (_listener != null)
                        {
                            _listener.onError(ex);
                        }
                        else
                        {
                            //_logger.ErrorException("[TVHclient] MessageBuilder caught exception : {0} but no error listener is configured!!!", ex, ex.ToString());
                        }
                    }
                }
            });
        }
    }
}
