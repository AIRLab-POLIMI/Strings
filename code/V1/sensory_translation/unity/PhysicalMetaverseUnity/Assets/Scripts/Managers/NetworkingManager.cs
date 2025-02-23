using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core;
using UnityEngine;
using GameEvents;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class NetworkingManager : Monosingleton<NetworkingManager>
{
    [SerializeField] private GameObject _setupScreen;
    
    [SerializeField] public SetupSO setup;
    
    private EndPointSO _jetsonEndpoint;

    [SerializeField] private int myTcpPort = 25777;

    [SerializeField] private int myUdpPort = 25666;

    [Tooltip("Max life of unread udp messages, in MINUTES")]
    [SerializeField] private int maxUdpAge = 2;

    [Tooltip("Max amount of unread udp messages")] [SerializeField]
    private int bufferSize = 50;

    [SerializeField] private KeyValueGameEventSO onKeyValueReceived;

    [SerializeField] private ByteSO unityPresentationKey;

    [SerializeField] private ByteSO jetsonSensorsReadyKey;

    [SerializeField] private ByteSO jetsonPingKey;
    public bool TCP_PRESENTATIONS = false;
 
    private readonly UdpMessenger _udpMessenger = new UdpMessenger();

    private TcpClient _tcpClient = null;
    private NetworkStream _tcpStream = null;

    private float pingInterval = 3f;
    private float pingIntervalUDP = 0.5f;
    private float pingTimeout = 5f;
    private float lastPingSentTime = 0f;
    private float lastPingReceivedTime = 0f;
    private float lastPingTime = 0f;

    private bool _initialized;
    private bool _connectedFirstTime = false;

    private bool pingedBack = false;

    private Thread clientReceiveThread;

    protected override void Init() =>
        SetInitialized(false);

    #region START

    public void Setup()
    {
        //todo make robot broadcast udp his ip and when picked up by this client start tcp connection (to avoid hardcoding the ip)
        _jetsonEndpoint = setup.JetsonEndpointUsage.Endpoint;
        _udpMessenger.Init(_jetsonEndpoint.EndPoint, myUdpPort, maxMsgAge: maxUdpAge, bufferSize: bufferSize);
        StartCoroutine(CheckOldMessages());
        StartCoroutine(Presentations());
        
        
    }

    #endregion

    #region PRESENTATION

    private IEnumerator Presentations()
    {

        var connected = false;
            
        _tcpClient = null;
        while (!connected)
        {
            try{
                //_setupScreen.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.red;
                //setupscreen random color
                _setupScreen.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                _tcpClient = TryTcpConnection(_jetsonEndpoint.EndPoint.Address.ToString(), myTcpPort);
            }catch(Exception e){
                Debug.Log(e);
                StartCoroutine(Presentations());
                yield break;
            }
            if (_tcpClient != null)
                connected = true;
            else
            {
                yield return new WaitForSeconds(1);
            }
        }

        byte data = unityPresentationKey.runtimeValue;
        _setupScreen.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.red;

        _tcpStream = _tcpClient.GetStream();
        _setupScreen.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.yellow;
            
        Debug.Log("SONO QUI");
        //change color of setup screen
        _setupScreen.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.green;
        yield return new WaitForSeconds(1);
        _tcpStream.WriteByte(data);
        Debug.Log("[TCP CHANNEL] sent data");
        _setupScreen.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.blue;
        if(TCP_PRESENTATIONS){
            //WAIT FOR JETSON RESPONSE
            Debug.Log("Waiting for response");
            int i = 0;
            while (!_tcpStream.DataAvailable)
            {
                i++;
                yield return new WaitForSeconds(1);
                try{
                    _setupScreen.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    Debug.Log("Data not available, waiting");
                    if  (i > 3){
                        Debug.Log("[TCP CHANNEL] sent data");
                        _tcpStream.WriteByte(data);
                        i = 0;
                    }
                }catch(Exception e){
                    Debug.Log(e);
                    StartCoroutine(Presentations());
                    yield break;
                }
            }

            byte[] response = new byte[1];
            _tcpStream.Read(response);
            Debug.Log(response[0]);
            if (response[0] == jetsonSensorsReadyKey.runtimeValue)
            {
                Debug.Log("RESPONSE OK");
                SetInitialized(true);

                lastPingReceivedTime = Time.time;
            }
            else
            {
                Debug.Log("RESPONSE NOT OK");
            }
            
        }


        SetInitialized(true);
        
        //CLOSE THE CONNECTION
        _tcpStream.Close();
        _tcpClient.Close();

        //_tcpClient = TryTcpConnection(_jetsonEndpoint.EndPoint.Address.ToString(), myTcpPort);
        //_tcpStream = _tcpClient.GetStream();
        
        //ConnectToTcpServer();
    }

    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("[ConnectToTcpServer]ERROR: "+ e);
        }
    }

    private void ListenForData()
    {
        try
        {
            Byte[] bytes = new byte[1024];
            while (true)
            {
                using (NetworkStream stream = _tcpClient.GetStream())
                {
                    int lenght;
                    while ((lenght = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[lenght];
                        if (incomingData[0] == jetsonPingKey.runtimeValue)
                        {
                            lastPingTime = Time.time;
                            Debug.Log("Pinged in ListenForData");
                        }
                    }
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e);
        }
    }


    private TcpClient TryTcpConnection(string ip, int port)
    {
        try
        {
            TcpClient client = null;
            
            client = new TcpClient();
            if (!client.ConnectAsync(ip, port).Wait(10))
            { 
                Debug.Log("No server found at" + ip + " to connect to, retrying");
            }
            else
            {
                return client;
            }

            return null;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }
    
    private async Task<TcpClient> TryTcpConnectionAsync(string ip, int port)
    {
        try
        {
            TcpClient client = new TcpClient();

            Task connectTask = client.ConnectAsync(ip, port);
        
            if (await Task.WhenAny(connectTask, Task.Delay(1000)) != connectTask)
            { 
                Debug.Log("No server found at " + ip + " to connect to, retrying");
                client.Dispose();
                return null;
            }

            await connectTask;
        
            return client;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    #endregion

    #region LOOP

    private void Update()
    {
        if(!_initialized)
            return;

        if (!_connectedFirstTime)
        {
            _connectedFirstTime = true;
            ConnectedFirstTimeRoutine();
        }
        
        ReceiveMessages();
        CheckPingNew();
        //SendPing();
    }

    private void ReceiveMessages()
    {
        if (_udpMessenger.UnreadMsgsPresent)
        {
            var messages = _udpMessenger.UnreadUdpMessages;

            foreach (var message in messages)
            {
                //print the received message
                Debug.Log("Received UDP " + message.Msg);
                lastPingReceivedTime = Time.time;
                CheckKeyValueMessage(message.Msg);
            }
        }
    }

    private bool CheckKeyValueMessage(byte[] msg)
    {
        var keyValMsg = KeyValueMsg.ParseKeyValueMsg(msg);

        if (keyValMsg != null)
        {
            onKeyValueReceived.Invoke(keyValMsg);
            return true;
        }

        return false;
    }

    #endregion

    #region PING

    private void CheckPingNew()
    {
        if (Time.time - lastPingSentTime > pingIntervalUDP)
        {
            //send ping
            byte[] msg = new byte[1];
            msg[0] = jetsonPingKey.runtimeValue;
            _udpMessenger.SendUdp(msg);
            Debug.Log("----NEWPING SENT----");
            lastPingSentTime = Time.time;
        }

        if (Time.time - lastPingReceivedTime > pingTimeout)
        {
            RetryConnection();
        }
    }

    private void CheckPing()
    {
        if (Time.time - lastPingTime > pingInterval)
        {
            var tries = 0;
            var done = false;
            while ((tries < 3) && (done == false))
            {
                Debug.Log("Try: "+tries);
                _tcpClient = TryTcpConnection(_jetsonEndpoint.EndPoint.Address.ToString(), myTcpPort);
                
                if (_tcpClient != null)
                {
                    var isAlive = PingJetson();
                
                    if (isAlive)
                    {
                        Debug.Log("PINGED!");
                        lastPingTime = Time.time;
                        _tcpStream.Close();
                        done = true;
                    }
                    else
                    {
                        Debug.Log("PING MISSED!");
                        _tcpStream?.Close();
                        //TODO QUI RICHIAMO IL SETUP PER RIPROVARE LA CONNESSIONE
                        //RetryConnection();
                        tries++;
                    }
                    _tcpClient.Close();
                
                }
                else
                {
                    Debug.Log("PING MISSED!");
                    //TODO QUI RICHIAMO IL SETUP PER RIPROVARE LA CONNESSIONE
                    //RetryConnection();
                    tries++;
                }
            }

            if (done == false)
            {
                RetryConnection();
            }
            

        }
    }
    
    private void CheckPingWithClient()
    {
        if (Time.time - lastPingTime > pingInterval)
        {
            
            var isAlive = PingJetson();
                
            if (isAlive)
            {
                Debug.Log("PINGED!");
                lastPingTime = Time.time; 
                _tcpStream.Close(); 
            }
            else
            {
                Debug.Log("PING MISSED!");
                _tcpStream?.Close();
                //TODO QUI RICHIAMO IL SETUP PER RIPROVARE LA CONNESSIONE
                RetryConnection();
            }
        }
    }

    private void SendPing()
    {
        try
        {
            NetworkStream stream = _tcpClient.GetStream();
            if (stream.CanWrite)
            {
                byte data = jetsonPingKey.runtimeValue;
                stream.WriteByte(data);
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e);
        }
    }

    private bool PingJetson()
    {
        try
        {
            //1--Get the Connection
            _tcpStream = _tcpClient.GetStream();
            
            //2--Prepare the message
            byte data = jetsonPingKey.runtimeValue;

            //3--Send ping
            //_tcpStream.WriteByte(data);
            NetworkStream stream = _tcpClient.GetStream();
            stream.WriteByte(data);

            byte[] response = new byte[1];
            var tries = 0;
            while (tries < 1)
            {
                stream.Read(response);
                if (response[0] != jetsonPingKey.runtimeValue)
                {
                    Debug.Log(response[0]);
                    tries++;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
        catch(SocketException e)
        {
            Debug.Log(e);
            return false;
        }
        

    }

    #endregion

    #region RECONNECTION

    private void RetryConnection()
    {
        
        Debug.Log("Retrying connection");
        
        _setupScreen.SetActive(true);
        
        //1--Invalidate the connection 
        SetInitialized(false);
        
        //2--Close left-over connections
        CloseConnections();
        
        //3--Restart
        Setup();
    }

    #endregion

    #region UTILS

    private void ConnectedFirstTimeRoutine()
    {
        GameObject.FindObjectOfType<LidarManager>().ActivatePoints();
        _setupScreen.SetActive(false);
    }

    private void SetInitialized(bool init)
    {
        _initialized = init;

        if (init == false)
        {
            _connectedFirstTime = init;
        }
    }

    private IEnumerator CheckOldMessages()
    {
        while (true)
        {
            _udpMessenger.TryRemoveOldMessages();
            yield return new WaitForSeconds(2000);
        }
    }

    private IEnumerator WaitNSeconds(float n)
    {
        yield return new WaitForSeconds(n);
    }

    public void OnDisable()
    {
        CloseConnections();
    }

    public void OnApplicationQuit()
    {
        CloseConnections();

    }

    public void CloseConnections()
    {
        _udpMessenger?.ClosePorts();
        _tcpClient?.Close();
    }

    #endregion
}
