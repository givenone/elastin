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
    void Start()
    {
        webcamTexture = new WebCamTexture();
        rawImage = GetComponent<RawImage>();
        rawImage.texture = webcamTexture;
        rawImage.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }
    public void TakePhoto(){

        // yield return new WaitForEndOfFrame ();

        var width = 320;
        var height = 240;

        Texture2D texture = new Texture2D(width, height);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // Encode texture into PNG
        var bytes = texture.EncodeToPNG();
        //Destroy(texture);


        File.WriteAllBytes (path + "SavedScreen.png", bytes);

    }
}