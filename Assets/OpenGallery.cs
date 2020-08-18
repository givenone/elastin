using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImageAndVideoPicker;

public class OpenGallery : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
	#if UNITY_ANDROID
	Debug.Log("start");
	AndroidPicker.CheckPermissions();
	Debug.Log("Android Perm");
	#endif
    }

    // Update is called once per frame
    public void BrowseImage()
    {
	#if UNITY_ANDROID
	Debug.Log("browse");
	AndroidPicker.BrowseImage();
	Debug.Log("Android Images");
	#endif
    }
}
