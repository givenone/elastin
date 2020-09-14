using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;

public class CameraScript : MonoBehaviour
{
    WebCamTexture webcamTexture;
    RawImage rawImage;
    string path = "Assets/demo_files/";
    public void Start()
    {
        if (rawImage == null || !rawImage.enabled ) {
            webcamTexture = new WebCamTexture();
            rawImage = GetComponent<RawImage>();
            rawImage.enabled = true;
            rawImage.texture = webcamTexture;
            rawImage.material.mainTexture = webcamTexture;
            webcamTexture.Play();
        }
    }
    void Update()
     {
         //This is to take the picture, save it and stop capturing the camera image.
         if(Input.GetMouseButtonDown(0) & rawImage.enabled)
         {
             SaveImage();
             webcamTexture.Stop();
             rawImage.enabled = false;
         }
     }
    void SaveImage()
    {
         //Create a Texture2D with the size of the rendered image on the screen.
         Texture2D texture = new Texture2D(rawImage.texture.width, rawImage.texture.height, TextureFormat.ARGB32, false);
         
         //Save the image to the Texture2D
         texture.SetPixels(webcamTexture.GetPixels());
         texture.Apply();
 
         //Encode it as a PNG.
         byte[] bytes = texture.EncodeToPNG();
         
         //Save it in a file.
         File.WriteAllBytes(path + "SavedScreen.png", bytes);
    }
}