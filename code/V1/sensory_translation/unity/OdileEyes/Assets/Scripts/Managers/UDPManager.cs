
using System.Collections;
using Core;
using Oasis.GameEvents;
using UnityEngine;


public class UDPManager : Monosingleton<UDPManager>
{
    [Header("network setup")]
    
    [SerializeField] private EndPointSO _raspberryEndpoint;
    public int myPort = 12345; // UDP port to listen on
    
    [Tooltip("max life of unread udp messages, in MINUTES")]
    [SerializeField] private int maxUdpAge = 2;

    [Tooltip("max amount of unread UDP messages")]
    [SerializeField] private int bufferSize = 50;
    
    [SerializeField] private KeyValueGameEventSO onKeyValueReceived;
    
    private readonly UDPMessenger _udpMessenger = new UDPMessenger();

    private byte[] _data;
    public byte[] Data { get => _data; }
    
    private bool _initialized;
    
    
    protected override void Init()
    {
        SetInitialized(true);
        
        // GENERAL
        _udpMessenger.Init(_raspberryEndpoint.EndPoint, myPort, maxMsgAge: maxUdpAge, bufferSize: bufferSize);

        StartCoroutine(CheckOldMessages());
    }

    private void SetInitialized(bool init)
    {
        Debug.Log($"[UDPMANAGER] - SET FLAG DIO CANE PORCO DIO - flag was: '{_initialized}' - now is: '{init}'");
        _initialized = init;
    }
    
    private IEnumerator CheckOldMessages()
    {
        // check old messages every two minutes
        while (true)
        {
            _udpMessenger.TryRemoveOldMessages();
            
            yield return new WaitForSeconds(2000);
        }
    }
    
    public void SendStringUpdToRasp(string message)
    {
        // only works if INITIALIZED, meaning, if game has started
        // Debug.Log($"[UDP MANAGER][SendStringUpdToRasp] - INIT: '{_initialized}'");
        if (_initialized)
        {
            // Debug.Log($"[UDP MANAGER] - sending message: '{message}' to RASP");
            _udpMessenger.SendUdp(message, _raspberryEndpoint.EndPoint);
        }
    }
    
    void Update()
    {
        if (!_initialized)
            return;
        
        ReceiveMessages();
    }

    private void ReceiveMessages()
    {
        if (_udpMessenger.UnreadMsgsPresent)
        {
            var messages = _udpMessenger.UnreadUdpMessages;

            foreach (var message in messages)
            {
                // check if it's a key-value message
                // otherwise, store the raw data in the input buffer, to be used by the UdpCameraViewer
                if (!CheckKeyValueMessage(message.Msg))
                    _data = message.RawMsg;
            }
        }
    }

    private bool CheckKeyValueMessage(string msg)
    {
        var keyValMsg = KeyValueMsg.ParseKeyValueMsg(msg);

        // Debug.Log($"  ---[CheckKeyValueMessage] - string msg: '{msg}' - KEY VALUE MESSAGE: {keyValMsg} - is it NONE? {keyValMsg == null} - {keyValMsg?.key} - {keyValMsg?.value}");
        
        if (keyValMsg != null && keyValMsg.success)
        {
            // ON KEY VALUE MSG GE
            onKeyValueReceived.Invoke(keyValMsg);
            return true;
        }

        return false;
    }
    
    public void OnDisable()
    {
        _udpMessenger.ClosePorts();
    }

    public void OnApplicationQuit()
    {
        _udpMessenger.ClosePorts();
        
        //end thread
        _initialized = false;
    }
}