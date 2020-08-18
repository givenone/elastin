using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LengthChange : MonoBehaviour
{
	public float scale = 1.0f;
    // Start is called before the first frame update
	void Start()
	{

	}
	void Update()
	{

	}
    public void ChangeLength(float new_scale)
	{
		if (scale != new_scale){
			transform.localScale = new Vector3(new_scale, new_scale, new_scale);
		}
		scale = new_scale;
	}
}
