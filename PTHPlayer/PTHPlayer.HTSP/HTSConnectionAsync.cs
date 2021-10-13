﻿using PTHPlayer.HTSP.Helpers;
using PTHPlayer.HTSP.HTSP_Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        private volatile Boolean _needsRestart = false;
        private volatile Boolean _connected;
        private volatile int _seq = 0;

        private readonly object _lock;
        private readonly HTSConnectionListener _listener;
        private readonly String _clientName;
        private readonly String _clientVersion;

        private int _serverProtocolVersion;
        private string _servername;
        private string _serverversion;
        private string _diskSpace;

        private readonly ByteList _buffer;
        private readonly SizeQueue<HTSMessage> _receivedMessagesQueue;
        private readonly SizeQueue<HTSMessage> _messagesForSendQueue;
        private readonly Dictionary<int, HTSResponseHandler> _responseHandlers;

        private Thread _receiveHandlerThread;
        private Thread _messageBuilderThread;
        private Thread _sendingHandlerThread;
        private Thread _messageDistributorThread;

        private Socket _socket = null;

        public HTSConnectionAsync(HTSConnectionListener listener, String clientName, String clientVersion)
        {
            //_logger = logger;

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

        public void stop()
        {
            
            if (_receiveHandlerThread != null && _receiveHandlerThread.IsAlive)
            {
                _receiveHandlerThread = null;//.Interrupt();
            }
            if (_messageBuilderThread != null && _messageBuilderThread.IsAlive)
            {
                _messageBuilderThread = null;//.Interrupt();
            }
            if (_sendingHandlerThread != null && _sendingHandlerThread.IsAlive)
            {
                _sendingHandlerThread = null;//.Interrupt();
            }
            if (_messageDistributorThread != null && _messageDistributorThread.IsAlive)
            {
                _messageDistributorThread = null;//.Interrupt();
            }
            if (_socket != null && _socket.Connected)
            {
                _socket.Close();
            }
            _needsRestart = true;
            _connected = false;
        }

        public Boolean needsRestart()
        {
            return _needsRestart;
        }

        public void open(String hostname, int port)
        {
            if (_connected)
            {
                return;
            }

            Monitor.Enter(_lock);
            while (!_connected)
            {
                try
                {
                    // Establish the remote endpoint for the socket.

                    IPAddress ipAddress;
                    if (!IPAddress.TryParse(hostname, out ipAddress))
                    {
                        // no IP --> ask DNS
                        IPHostEntry ipHostInfo = Dns.GetHostEntry(hostname);
                        ipAddress = ipHostInfo.AddressList[0];
                    }

                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                    /*
                    //_logger.Info("[TVHclient] HTSConnectionAsync.open: " +
                        "IPEndPoint = '" + remoteEP.ToString() + "'; " +
                        "AddressFamily = '" + ipAddress.AddressFamily + "'");
                    */
                    // Create a TCP/IP  socket.
                    _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // connect to server
                    _socket.Connect(remoteEP);

                    _connected = true;
                    ////_logger.Info("[TVHclient] HTSConnectionAsync.open: socket connected.");
                }
                catch (Exception ex)
                {
                    ////_logger.Error("[TVHclient] HTSConnectionAsync.open: caught exception : {0}", ex.Message);

                    Thread.Sleep(2000);
                }
            }

            
            ThreadStart ReceiveHandlerRef = new ThreadStart(ReceiveHandler);
            _receiveHandlerThread = new Thread(ReceiveHandlerRef);
            _receiveHandlerThread.IsBackground = true;
            _receiveHandlerThread.Start();
            
            ThreadStart MessageBuilderRef = new ThreadStart(MessageBuilder);
            _messageBuilderThread = new Thread(MessageBuilderRef);
            _messageBuilderThread.IsBackground = true;
            _messageBuilderThread.Start();
            
            ThreadStart SendingHandlerRef = new ThreadStart(SendingHandler);
            _sendingHandlerThread = new Thread(SendingHandlerRef);
            _sendingHandlerThread.IsBackground = true;
            _sendingHandlerThread.Start();

            ThreadStart MessageDistributorRef = new ThreadStart(MessageDistributor);
            _messageDistributorThread = new Thread(MessageDistributorRef);
            _messageDistributorThread.IsBackground = true;
            _messageDistributorThread.Start();
            
            Monitor.Exit(_lock);
        }

        public Boolean authenticate(String username, String password)
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

        private void SendingHandler()

        {
            Boolean threadOk = true;
            while (_connected && threadOk)
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
                catch (ThreadAbortException)
                {
                    threadOk = false;
                    Thread.ResetAbort();
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
        }
        private void ReceiveHandler()
        {
            Boolean threadOk = true;
            byte[] readBuffer = new byte[8192];
            while (_connected && threadOk)
            {
                
                try
                {
                    int bytesReveived = _socket.Receive(readBuffer);
                    _buffer.appendCount(readBuffer, bytesReveived);
                }
                catch (ThreadAbortException)
                {
                    threadOk = false;
                    Thread.ResetAbort();
                }
                catch (Exception ex)
                {
                    threadOk = false;
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
        }

        private void MessageBuilder()
        {
            Boolean threadOk = true;
            while (_connected && threadOk)
            {
                try
                {
                    byte[] lengthInformation = _buffer.getFromStart(4);
                    long messageDataLength = HTSMessage.uIntToLong(lengthInformation[0], lengthInformation[1], lengthInformation[2], lengthInformation[3]);
                    byte[] messageData = _buffer.extractFromStart((long)messageDataLength + 4); // should be long !!!
                    HTSMessage response = HTSMessage.parse(messageData);//, //_logger);
                    _receivedMessagesQueue.Enqueue(response);
                }
                catch (ThreadAbortException)
                {
                    threadOk = false;
                    Thread.ResetAbort();
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
        }

        private void MessageDistributor()
        {
            Boolean threadOk = true;
            while (_connected && threadOk)
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
                catch (ThreadAbortException)
                {
                    threadOk = false;
                    Thread.ResetAbort();
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
        }
    }
}
