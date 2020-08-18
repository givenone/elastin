using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurlChange : MonoBehaviour
{
	public Vector3 RotateAmount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
	
	public void ChangeCurl()
	{
	        transform.Rotate(30.0f, 30.0f, 30.0f, Space.Self);
	}
}
