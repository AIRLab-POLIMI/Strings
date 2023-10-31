
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;


//receives camera feed on udp socket and shows it on a RawImage, also sends camera angles via udp using key protocol
public class UdpCameraViewer : MonoBehaviour
{
    // public string videoStreamURL = "https://"; // Replace with your camera's streaming URL
    
    public RawImage rawImage;
    
    private Texture2D _videoTexture;
    
    private Texture2D receivedTexture;
    private Texture2D lastValidReceivedTexture;
    
    private bool _textureInitialized = false;
    
    
    private void Start()
    {
        receivedTexture = new Texture2D(1, 1);
        lastValidReceivedTexture = new Texture2D(1, 1);
        rawImage.texture = lastValidReceivedTexture;
        
        // _textureInitialized = false;
        //
        // _videoTexture = new Texture2D(2, 2);
        
        // rawImage.texture = _videoTexture;
        
        // StartCoroutine(UpdateVideoTexture());
    }
    
    
    void Update(){
        // UpdateCamera();
        UpdateCameraOld();
    }
    
    // IEnumerator UpdateVideoTexture()
    // {
    //     while (true)
    //     {
    //         using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(videoStreamURL))
    //         {
    //             www.method = UnityWebRequest.kHttpVerbGET; // Set the request method to GET
    //
    //             yield return www.SendWebRequest();
    //
    //             if (www.isNetworkError || www.isHttpError)
    //             {
    //                 Debug.Log("Error fetching video stream: " + www.error);
    //             }
    //             else
    //             {
    //                 _videoTexture.LoadImage(www.downloadHandler.data);
    //                 rawImage.texture = _videoTexture;
    //                 _videoTexture.Apply();
    //                 
    //                 Debug.LogError("GOOD FRAME: " + www.error);
    //             }
    //         }
    //     }
    // }
    
    
    void UpdateCamera()
    {
        var rawData = UDPManager.Instance.TryGetFrame();
    
        if (rawData == null)
        {
            Debug.Log("diocane");
            return;
        }
        
        if (!_textureInitialized || _videoTexture == null)
        {
            _videoTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
    
            // Update the videoTexture with the received frame data.
            _videoTexture.LoadRawTextureData(rawData);
            
            if (_videoTexture == null)
                return;
    
            _videoTexture.Apply();
            
            // Adjust the raw image aspect ratio
            float aspectRatio = (float)_videoTexture.width / _videoTexture.height;
            rawImage.rectTransform.sizeDelta = new Vector2(rawImage.rectTransform.rect.height * aspectRatio, rawImage.rectTransform.rect.height);
            
            rawImage.texture = _videoTexture;
            _textureInitialized = true;
        }
        else
        {
            // Update the videoTexture with the received frame data.
            _videoTexture.LoadRawTextureData(rawData);
            _videoTexture.Apply();
        }    
    }
    
    
    void UpdateCameraOld()
    {
        var rawData = UDPManager.Instance.TryGetFrame();
    
        if (rawData == null)
        {
            Debug.Log("diocane");
            return;
        }
        
        // Create a new texture from data received by network manager
        // receivedTexture.LoadImage(NetworkManager.Instance.Data);
        receivedTexture.LoadImage(rawData);
        //if image is square continue
        if (receivedTexture.width == receivedTexture.height)
        {
            Debug.LogWarning("Received image is square. Image will not be displayed.");
        }
        else
        {
            // lastValidReceivedTexture.LoadImage(NetworkManager.Instance.Data);
            lastValidReceivedTexture.LoadImage(rawData);
        }
        
        // lastValidReceivedTexture.LoadImage(rawData);
        
        //if lastvalid is not null apply
        if (lastValidReceivedTexture != null)
        {
            //set rawimage resolution to received image's one
            //rawImage.rectTransform.sizeDelta = new Vector2(receivedTexture.width, receivedTexture.height);
            receivedTexture.Apply();
    
            // Adjust the raw image aspect ratio
            float aspectRatio = (float)receivedTexture.width / receivedTexture.height;
            rawImage.rectTransform.sizeDelta = new Vector2(rawImage.rectTransform.rect.height * aspectRatio, rawImage.rectTransform.rect.height);
    
            // Uncomment the next line if you want to flip the texture vertically (useful for some cameras)
            // receivedTexture.Apply(false, true);
        }
        else
        {
            Debug.LogWarning("IT'S ALL OVER.");
        }
    }
    
    // Print on GUI the camera's angles
    /*void OnGUI()
    {
        // GUI background style
        GUIStyle customStyle = new GUIStyle(GUI.skin.box);
        customStyle.fontSize = 20;
        customStyle.normal.textColor = Color.white;
        GUI.backgroundColor = Color.grey;

        // Show angles in the center of the screen
        GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100),
            "Camera Angles: \n" + "X: " + _camera.transform.eulerAngles.x + "\nY: " + _camera.transform.eulerAngles.y,
            customStyle);
    }*/
}
