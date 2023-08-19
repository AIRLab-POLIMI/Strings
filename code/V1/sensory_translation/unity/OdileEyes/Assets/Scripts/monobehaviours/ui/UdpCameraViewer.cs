
using UnityEngine;
using UnityEngine.UI;


//receives camera feed on udp socket and shows it on a RawImage, also sends camera angles via udp using key protocol
public class UdpCameraViewer : MonoBehaviour
{
    public RawImage rawImage;
    
    private Texture2D receivedTexture;
    private Texture2D lastValidReceivedTexture;
    
    private void Start()
    {
        receivedTexture = new Texture2D(1, 1);
        lastValidReceivedTexture = new Texture2D(1, 1);
        rawImage.texture = lastValidReceivedTexture;
    }

    void Update(){
        // Create a new texture from data received by network manager
        receivedTexture.LoadImage(NetworkManager.Instance.Data);
        //if image is square continue
        if (receivedTexture.width == receivedTexture.height)
        {
            Debug.LogWarning("Received image is square. Image will not be displayed.");
        }
        else
        {
            lastValidReceivedTexture.LoadImage(NetworkManager.Instance.Data);
        }
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
