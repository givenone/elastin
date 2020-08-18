using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderEnable : MonoBehaviour
{
	GameObject slider = null;
	bool is_visible;

	public void DisableSlider()
	{		
		if (slider == null) {
			slider = GameObject.Find ("Slider");
			Debug.Log(slider);
		}
		{
			is_visible = false;
			Debug.Log(slider);
			Debug.Log("set to false");
		slider.SetActive (is_visible);
		}
	}
    public void EnableSlider()
	{
		if (slider == null) {
			slider = GameObject.Find ("Slider");
			Debug.Log(slider);
		}
		{
			is_visible = true;
			Debug.Log(slider);
			Debug.Log("set to true");
		}
		slider.SetActive (is_visible);
	}
}


