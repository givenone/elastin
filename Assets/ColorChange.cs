using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    // Start is called before the first frame update
    public void ChangeColor()
	{
		GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV());
	}
}
