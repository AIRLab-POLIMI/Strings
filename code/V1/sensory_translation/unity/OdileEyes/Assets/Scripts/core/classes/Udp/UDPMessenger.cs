// Inspired by this thread: https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/
// Thanks OP la1n
// Thanks MattijsKneppers for letting me know that I also need to lock my queue while enqueuing
// Adapted during projects according to my needs

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class UDPMessenger
{
    #region FIELDS
    
        private Thread _receiveThread;

        // messages that are older than this amount in MINUTES are DELETED from 'unread messages' buffer
        private int _maxMsgAge;
        
        // max len of unread messages buffer; when capacity is reached, older messages are removed when newer arrive
        private int _bufferSize;
        
        // udpClient object
        private UdpClient _client;
        
        // UDP messages
        private List<UdpMessage> _unreadUdpMsgs = new List<UdpMessage>();
        
    #endregion

    
    #region GETTERS/SETTERS
    
        // IP and PORT
        public int MyPort { get; private set; } // Sometimes we need to define the source port, since some devices only accept messages coming from a predefined sourceport.
        
        public IPEndPoint DefaultDestinationEndPoint { get; private set; }
        public IPAddress DefaultDestinationIP { get; private set; }
        public int DefaultDestinationPort { get; private set; }

        // UDP MSG
        public UdpMessage LatestUdpMsg { get; private set; }

        public List<UdpMessage> UnreadUdpMessages
        {
            get
            {
                // when ACCESSING the unread msgs, we delete them from buffer
                var temp = _unreadUdpMsgs;
                _unreadUdpMsgs = new List<UdpMessage>();
                return temp;
            }
        }

        public bool UnreadMsgsPresent => _unreadUdpMsgs.Count > 0;

    #endregion
    

    #region SETUP
    
        public void Init(
            IPEndPoint defaultDestination, 
            int sourcePort = -1,  // If sourcePort is not set, its being chosen randomly by the system
            int maxMsgAge = 5, 
            int bufferSize = 20) 
        {
            DefaultDestinationEndPoint = defaultDestination;
            DefaultDestinationIP = defaultDestination.Address;
            DefaultDestinationPort = defaultDestination.Port;
            
            this.MyPort = sourcePort;
            if (this.MyPort <= -1)
            {
                _client = new UdpClient();
                Debug.Log("Sending to " + DefaultDestinationIP + ": " + this.DefaultDestinationPort);
            }
            else
            {
                _client = new UdpClient(this.MyPort);
                Debug.Log("Sending to " + DefaultDestinationIP + ": " + this.DefaultDestinationPort + " from Source Port: " + this.MyPort);
            }

            _client.Client.ReceiveTimeout = 2000; // Set the UDP socket timeout to 2 seconds
            
            _maxMsgAge = maxMsgAge;
            _bufferSize = bufferSize;
            
            _receiveThread = new Thread(
                new ThreadStart(ReceiveData));
            
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }
        
    #endregion
    
    
    #region RECEIVE UDP
    
        private void ReceiveData()
        {
            while (true)
            {
                try
                {
                    OnMessageReceived();
                }
                catch (SocketException e)
                {
                    Debug.LogWarning("[UdpMessenger][ReceiveData] Socket exception: " + e.Message);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[UdpMessenger][ReceiveData] Error receiving msg: " + e.Message);
                }
            }
        }

        private void OnMessageReceived()
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, this.MyPort);
            byte[] data = _client.Receive(ref anyIP);
            string text = Encoding.UTF8.GetString(data);

            // add message to unread list and save it as Latest
            LatestUdpMsg = new UdpMessage(data, text, anyIP);
            _unreadUdpMsgs.Add(LatestUdpMsg);
            
            // if maximum amount has been reached, remove oldest unread message
            if (_unreadUdpMsgs.Count > _bufferSize)
                _unreadUdpMsgs.RemoveAt(0);
        }
        
    #endregion


    #region SEND UDP

        // sendData in different ways. Can be extended accordingly
        public void SendUdp(byte[] data, IPEndPoint destination = null)
        {
            try
            {
                _client.Send(data, data.Length, destination == null ? DefaultDestinationEndPoint : destination);
            }
            catch (Exception err)
            {
                Debug.Log($"[SendUDP] - ERROR: '{err.ToString()}'");
            }
        }

        public void SendUdp(string message, IPEndPoint destination = null)
        {
            if (message == "")
            {
                return;
            }
                    
            //replace all "," with "."
            message = message.Replace(",", ".");
            message = message + "\n";
            
            SendUdp(System.Text.Encoding.ASCII.GetBytes(message), destination);
        }

        public void SendUdp(Int32 message, IPEndPoint destination = null) =>
            SendUdp(BitConverter.GetBytes(message), destination);

        public void SendUdp(Int32[] myInts, IPEndPoint destination = null)
        {
            byte[] data = new byte[myInts.Length * sizeof(Int32)];
            Buffer.BlockCopy(myInts, 0, data, 0, data.Length);
            SendUdp(data, destination);
        }
        
        public void SendUdp(Int16[] myInts, IPEndPoint destination = null)
        {
            byte[] data = new byte[myInts.Length * sizeof(Int16)];
            Buffer.BlockCopy(myInts, 0, data, 0, data.Length);
            SendUdp(data, destination);
        }
        
        
        #region backup SEND methods
        
            public void SendString(string message)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _client.Send(data, data.Length, DefaultDestinationEndPoint);

                }
                catch (Exception err)
                {
                    Debug.Log(err.ToString());
                }
            }

            public void SendInt32(Int32 myInt)
            {
                try
                {
                    byte[] data = BitConverter.GetBytes(myInt);
                    _client.Send(data, data.Length, DefaultDestinationEndPoint);
                }
                catch (Exception err)
                {
                    Debug.Log(err.ToString());
                }
            }

            public void SendInt32Array(Int32[] myInts)
            {
                try
                {
                    byte[] data = new byte[myInts.Length * sizeof(Int32)];
                    Buffer.BlockCopy(myInts, 0, data, 0, data.Length);
                    _client.Send(data, data.Length, DefaultDestinationEndPoint);
                }
                catch (Exception err)
                {
                    Debug.Log(err.ToString());
                }
            }

            public void SendInt16Array(Int16[] myInts)
            {
                try
                {
                    byte[] data = new byte[myInts.Length * sizeof(Int16)];
                    Buffer.BlockCopy(myInts, 0, data, 0, data.Length);
                    _client.Send(data, data.Length, DefaultDestinationEndPoint);
                }
                catch (Exception err)
                {
                    Debug.Log(err.ToString());
                }
            }



        #endregion
        
        
    #endregion
    

    #region CLEANUP


        public void TryRemoveOldMessages()
        {
            // called by outside wrappers. 
            // checks all unread messages and deletes the ones that are older than 'maxMsgAge' minutes

            var now = DateTime.Now;

            for (int i = _unreadUdpMsgs.Count -1; i >= 0; i--)
            {
                var msg = _unreadUdpMsgs[i];
                if (TimeHelper.TimeSubtractionInMinutes(msg.ReceptionTime, now) > _maxMsgAge)
                    _unreadUdpMsgs.Remove(msg);
            }
        }
    
        public void ClosePorts()
        {
            Debug.Log("closing receiving UDP on port: " + MyPort);

            if (_receiveThread != null)
                _receiveThread.Abort();

            _client.Close();
        }
        
    #endregion
    
    
    #region GET IP ENDPOINT (static)

        public static IPEndPoint GetIPEndPoint(string ip, int port) => 
            new IPEndPoint(System.Net.IPAddress.Parse(ip), port);

        public static IPEndPoint GetIPEndPoint(IPAddress ip, int port) =>
            new IPEndPoint(ip, port);    

    #endregion
}

public class UdpMessage
{
    public byte[] RawMsg { get; private set; }
    
    public string Msg { get; private set; }
    public IPEndPoint Sender { get; private set; }
    public DateTime ReceptionTime { get; private set; }
    
    public UdpMessage(byte[] rawMsg, string msg, IPEndPoint sender)
    {
        RawMsg = rawMsg;
        Msg = msg;
        Sender = sender;
        ReceptionTime = DateTime.Now;
    }
}
