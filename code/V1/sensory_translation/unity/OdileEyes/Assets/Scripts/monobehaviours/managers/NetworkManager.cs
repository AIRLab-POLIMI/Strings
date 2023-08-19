
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using Core;
using TMPro;


//receives camera feed on udp socket and shows it on a RawImage, also sends camera angles via udp using key protocol
public class NetworkManager : Monosingleton<NetworkManager>
{
    // UDP settings
    public string serverIP = "192.168.0.103"; // Change this to the IP address of the receiver
    public int serverPort = 40616;       // Change this to the port number of the receiver
    private int udpPort = 12345; // UDP port to listen on
    private UdpClient _udpClient;
    private bool _isReceiving = false;
    private byte[] _data;
    private Thread _receiveThread;
    
    // UI information
    [SerializeField] private TextMeshProUGUI text;
    public int maxDisplayedMessages = 8;
    private readonly List<string> _displayedMessages = new();
    
    public byte[] Data { get => _data; }
    
    
    private void Start()
    {
        // FOR THE FUTURE
        // _cameraX = new SensorValue("_ax");
        // _cameraX = new SensorValue("_ay");
        
        _udpClient = new UdpClient(udpPort);
        _udpClient.Client.ReceiveTimeout = 2000; // Set the UDP socket timeout to 2 seconds
        
        //start receive frames on separate thread using threading
        var receiveThread = new Thread(new ThreadStart(ReceiveFrames));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }
    
    public void SendMsg(string msg) 
    {
        if (msg == "")
        {
            return;
        }
        
        //replace all "," with "."
        msg = msg.Replace(",", ".");
        msg = msg + "\n";
        
        byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
        _udpClient.Send(data, data.Length, serverIP, serverPort);
        //log
        Debug.Log("Sent: " + msg);
        
        // Add message to displayed messages
        _displayedMessages.Add(msg);
        if (_displayedMessages.Count > maxDisplayedMessages)
            _displayedMessages.RemoveAt(0);
        
        // Update text: display all messages in displayedMessages list, separated by a new line
        text.text = "";
        foreach (string message in _displayedMessages)
            text.text += message + "\n";
    }
    
    private void ReceiveFrames()
    {
        _isReceiving = true;
        while (_isReceiving)
        {
            try
            {
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, udpPort);
                _data = _udpClient.Receive(ref remoteIpEndPoint);
            }
            catch (SocketException e)
            {
                Debug.LogWarning("Socket exception: " + e.Message);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error receiving frame: " + e.Message);
            }
        }
    }

    private void OnApplicationQuit()
    {
        //end thread
        _isReceiving = false;
        //close thread
        _receiveThread.Abort();

        _udpClient.Close();
    }
}
