using PTHPlayer.HTSP.Helpers;
using PTHPlayer.HTSP.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PTHPlayer.HTSP
{
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
        const long BytesPerGiga = 1024 * 1024 * 1024;

        volatile bool _needsRestart = false;
        volatile bool _connected;
        volatile bool _authenticated;
        volatile int _seq = 0;

        readonly object _lock;
        readonly HTSConnectionListener _listener;
        readonly string _clientName;
        readonly string _clientVersion;

        private int _serverProtocolVersion;
        private string _servername;
        private string _serverversion;
        private string _diskSpace;

        readonly ByteList _buffer;
        readonly SizeQueue<HTSMessage> _receivedMessagesQueue;
        readonly SizeQueue<HTSMessage> _messagesForSendQueue;
        readonly Dictionary<int, HTSResponseHandler> _responseHandlers;

        Task _monitorHandlerThread;
        Task _receiveHandlerThread;
        Task _messageBuilderThread;
        Task _sendingHandlerThread;
        Task _messageDistributorThread;

        Socket _socket = null;

        CancellationTokenSource handlersCancellationSource;
        CancellationTokenSource monitorCancellationSource;

        Stopwatch sendTimer = new Stopwatch();

        DateTime LastValidPacketReceived = DateTime.UtcNow;
        volatile bool isSubscribtionStart = false;

        string _hostName = string.Empty;
        int _port = 0;

        string _userName = string.Empty;
        string _password = string.Empty;

        public event EventHandler<HTSPConnectionStateChangeArgs> ConnectionStateChange;
        public event EventHandler<HTSPErrorArgs> ErrorHandler;

        protected virtual void OnConnectionStateChange(HTSPConnectionStateChangeArgs e)
        {
            EventHandler<HTSPConnectionStateChangeArgs> handler = ConnectionStateChange;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnErrorHandler(HTSPErrorArgs e)
        {
            EventHandler<HTSPErrorArgs> handler = ErrorHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

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

        public void Stop(bool stopMonitor = false)
        {
            isSubscribtionStart = false;

            if (monitorCancellationSource != null && stopMonitor)
            {
                monitorCancellationSource.Cancel();
            }

            handlersCancellationSource.Cancel();
            try
            {
                if (Task.WaitAll(new[] { _receiveHandlerThread, _messageBuilderThread, _sendingHandlerThread, _messageDistributorThread }, TimeSpan.FromSeconds(15)))
                {

                    if (_socket != null && _socket.Connected)
                    {
                        _socket.Close();
                    }

                    _messagesForSendQueue.Clear();
                    _receivedMessagesQueue.Clear();

                    _needsRestart = true;
                    _connected = false;
                }
                else
                {
                    throw new Exception("Fatal close error");
                }
            }
            catch
            {
                throw;
            }
        }

        public bool NeedsRestart()
        {
            return _needsRestart;
        }

        public void Start(string hostName, int port, string userName, string password)
        {
            _hostName = hostName;
            _port = port;

            _userName = userName;
            _password = password;

            monitorCancellationSource = new CancellationTokenSource();
            _monitorHandlerThread = Task.Run(() => MonitorHandler());
        }

        public bool Open(string hostName = "", int port = 0)
        {
            if (!String.IsNullOrEmpty(hostName) && port != 0)
            {
                _hostName = hostName;
                _port = port;
            }

            handlersCancellationSource = new CancellationTokenSource();

            if (_connected)
            {
                return true;
            }

            Monitor.Enter(_lock);

            try
            {
                IPAddress ipAddress;
                if (!IPAddress.TryParse(_hostName, out ipAddress))
                {
                    // no IP --> ask DNS
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(_hostName);
                    ipAddress = ipHostInfo.AddressList[0];
                }

                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

                _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.ReceiveTimeout = 2000;
                _socket.SendTimeout = 2000;

                IAsyncResult result = _socket.BeginConnect(remoteEP, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(3000, true);

                if (success && _socket.Connected)
                {
                    _socket.EndConnect(result);
                    _connected = true;
                }
                else
                {
                    // NOTE, MUST CLOSE THE SOCKET
                    _socket.Close();
                    return false;
                }

            }
            catch (Exception ex)
            {
                OnErrorHandler(new HTSPErrorArgs(ex.Message));
                return false;
            }

            _receiveHandlerThread = Task.Run(() => ReceiveHandler());
            _messageBuilderThread = Task.Run(() => MessageBuilder());
            _sendingHandlerThread = Task.Run(() => SendingHandler());
            _messageDistributorThread = Task.Run(() => MessageDistributor());

            Monitor.Exit(_lock);

            return true;
        }

        public bool Authenticate(String username, String password, CancellationToken cancellationToken)
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
            HTSMessage helloResponse = loopBackResponseHandler.getResponse(cancellationToken);
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
                HTSMessage authResponse = loopBackResponseHandler.getResponse(cancellationToken);
                if (authResponse != null)
                {
                    Boolean auth = authResponse.getInt("noaccess", 0) != 1;
                    if (auth)
                    {
                        HTSMessage getDiskSpaceMessage = new HTSMessage();
                        getDiskSpaceMessage.Method = "getDiskSpace";
                        sendMessage(getDiskSpaceMessage, loopBackResponseHandler);
                        HTSMessage diskSpaceResponse = loopBackResponseHandler.getResponse(cancellationToken);
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

        private void MonitorHandler()
        {
            while (!monitorCancellationSource.IsCancellationRequested)
            {
                try
                {
                    var receiveDiff = DateTime.UtcNow - LastValidPacketReceived;
                    var timeOutSpan = TimeSpan.FromSeconds(8);

                    if (!Connected() || sendTimer.Elapsed > timeOutSpan || (isSubscribtionStart && receiveDiff > timeOutSpan))
                    {
                        _authenticated = false;
                        sendTimer = new Stopwatch();
                        if (handlersCancellationSource != null)
                        {

                            //Kick Disconnected
                            OnConnectionStateChange(new HTSPConnectionStateChangeArgs(ConnectionState.Disconnected));
                            Stop();
                        }

                        //Kick Connecting
                        OnConnectionStateChange(new HTSPConnectionStateChangeArgs(ConnectionState.Connecting));
                        if (!Open())
                        {
                            OnConnectionStateChange(new HTSPConnectionStateChangeArgs(ConnectionState.ConnectingFail));
                            //Kick connecting error
                            Thread.Sleep(5000);
                            continue;
                        }
                        else
                        {
                            OnConnectionStateChange(new HTSPConnectionStateChangeArgs(ConnectionState.Connected));
                        }

                    }

                    if(!_authenticated)
                    {
                        OnConnectionStateChange(new HTSPConnectionStateChangeArgs(ConnectionState.Authenticating));
                        //Kick Login
                        if (!Authenticate(_userName, _password, monitorCancellationSource.Token))
                        {
                            OnConnectionStateChange(new HTSPConnectionStateChangeArgs(ConnectionState.AuthenticatingFail));
                            //Kick login problem error
                            Thread.Sleep(5000);
                            continue;
                        }
                        else
                        {
                            OnConnectionStateChange(new HTSPConnectionStateChangeArgs(ConnectionState.Authenticated));
                            _authenticated = true;
                        }
                    }

                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    OnErrorHandler(new HTSPErrorArgs(ex.Message));
                }
            }
        }

        bool Connected()
        {
            try
            {
                if (_socket != null)
                {
                    bool part1 = _socket.Poll(1000, SelectMode.SelectRead);
                    bool part2 = (_socket.Available == 0);
                    if (part1 && part2)
                        return false;
                    else
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void SendingHandler()
        {
            while (!handlersCancellationSource.IsCancellationRequested)
            {
                try
                {
                    HTSMessage message = _messagesForSendQueue.Dequeue(handlersCancellationSource.Token);
                    if (message != null)
                    {
                        byte[] data2send = message.BuildBytes();
                        sendTimer.Restart();
                        int bytesSent = _socket.Send(data2send);
                        if (bytesSent != data2send.Length)
                        {
                            OnErrorHandler( new HTSPErrorArgs("[TVHclient] SendingHandler: Sending not complete! \nBytes sent: " + bytesSent + "\nMessage bytes: " +
                                data2send.Length + "\nMessage: " + message.ToString()));
                        }
                    }
                }
                catch (SocketException ex)
                {
                    OnErrorHandler(new HTSPErrorArgs());
                    _connected = false;
                }
                catch (Exception ex)
                {
                    if (_listener != null)
                    {
                        _listener.onError(ex);
                    }
                    else
                    {
                        OnErrorHandler(new HTSPErrorArgs());
                    }
                }
            }
        }

        private void ReceiveHandler()
        {
            byte[] readBuffer = new byte[8192];
            while (!handlersCancellationSource.IsCancellationRequested)
            {
                try
                {
                    int bytesReveived = _socket.Receive(readBuffer);
                    _buffer.appendCount(readBuffer, bytesReveived);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode != 10060 && ex.ErrorCode != 110)
                    {
                        OnErrorHandler(new HTSPErrorArgs("Receive Socket error"));
                        _connected = false;
                    }
                   // OnErrorHandler(new HTSPErrorArgs("Receive Exception error"));
                }
                catch (Exception ex)
                {
                    if (_listener != null)
                    {
                        Task.Factory.StartNew(() => _listener.onError(ex));
                    }
                    else
                    {
                        OnErrorHandler(new HTSPErrorArgs("Receive Exception error"));
                    }
                }
            }
        }

        private void MessageBuilder()
        {
            while (!handlersCancellationSource.IsCancellationRequested)
            {
                try
                {
                    byte[] lengthInformation = _buffer.getFromStart(4, handlersCancellationSource.Token);
                    if (lengthInformation.Length > 0)
                    {
                        long messageDataLength = HTSMessage.uIntToLong(lengthInformation[0], lengthInformation[1], lengthInformation[2], lengthInformation[3]);
                        byte[] messageData = _buffer.extractFromStart((long)messageDataLength + 4); // should be long !!!
                        HTSMessage response = HTSMessage.parse(messageData);
                        _receivedMessagesQueue.Enqueue(response);
                        LastValidPacketReceived = DateTime.UtcNow;
                        sendTimer.Stop();
                        //Monitor if receive sub or desub 
                        if (response.Method == "subscriptionStart")
                        {
                            isSubscribtionStart = true;
                        }
                        if (response.Method == "subscriptionStop" || response.Method == "subscriptionSkip")
                        {
                            isSubscribtionStart = false;
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
                        OnErrorHandler(new HTSPErrorArgs());
                    }
                }
            }
        }

        private void MessageDistributor()
        {
            while (!handlersCancellationSource.IsCancellationRequested)
            {
                try
                {
                    HTSMessage response = _receivedMessagesQueue.Dequeue(handlersCancellationSource.Token);
                    if (response != null)
                    {
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
                                OnErrorHandler(new HTSPErrorArgs("MessageDistributor: HTSResponseHandler for seq = '" + seqNo + "' not found!"));
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
                }
                catch (Exception ex)
                {
                    if (_listener != null)
                    {
                        _listener.onError(ex);
                    }
                    else
                    {
                        OnErrorHandler(new HTSPErrorArgs());
                    }
                }
            }
        }
    }
}
