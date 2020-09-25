using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    protected Transform Camera;
    protected Transform Parent;

    private Vector3 _LocalRotation;

    private Quaternion Rot;
    private Vector3 Position;

    private Quaternion initialRot;
    private Quaternion initialCamRot;
    private Vector3 initialPosition;


    private float _CameraDistance = 1f;

    public float MouseSensitivity = 10f;
    public float TranslationSensitivity = 0.1f;
    public float ScrollSensitvity = 20f;
    public float OrbitDampening = 0.5f;
    public float ScrollDampening = 1f;

    public bool CameraDisabled = false;

    public HairManager hairManager;

    public void Toggle()
    {
        //Debug.Log(CameraDisabled);
        CameraDisabled = !CameraDisabled;
    }

    public void Reset()
    {
        this.Rot = initialRot;
        this.Position = initialPosition;
        this._CameraDistance = 1f;

        hairManager.ResetHair();
    }
    // Start is called before the first frame update
    void Start()
    {
        this.Camera = this.transform;
        this.Parent = this.transform.parent;
        this.initialPosition = this.transform.localPosition;
        this.initialRot = this.transform.parent.rotation;
        this.initialCamRot = this.transform.rotation;

        this.Position = this.initialPosition;
        this.Rot = this.initialRot;
    }

    // Late Update is called once per frame, after Update() on every game object in the scene.
    void LateUpdate()
    {
        if (!CameraDisabled)
        {
            //Rotation of the Camera based on Mouse Coordinates (Left Click)
            if ( (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && Input.GetMouseButton(0))
            {
                _LocalRotation.x += Input.GetAxis("Mouse X") * MouseSensitivity;
                _LocalRotation.y += Input.GetAxis("Mouse Y") * MouseSensitivity;

                //Clamp the y Rotation to horizon and not flipping over at the top
                if (_LocalRotation.y < -90f)
                    _LocalRotation.y = -90f;
                else if (_LocalRotation.y > 90f)
                    _LocalRotation.y = 90f;

                Mathf.Clamp(_LocalRotation.x, -45f,  45f);
                
                Quaternion QT = Quaternion.Euler(_LocalRotation.y, _LocalRotation.x, 0);
                this.Rot = Quaternion.Lerp(this.Parent.rotation, QT, Time.deltaTime * OrbitDampening);
            }

            // Translation (Right Click). Dont use.
            if ( (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && Input.GetMouseButton(1))
            {
                _LocalRotation.x += Input.GetAxis("Mouse X") * MouseSensitivity;
                _LocalRotation.y += Input.GetAxis("Mouse Y") * MouseSensitivity;
                Debug.Log(_LocalRotation);
                
                this._LocalRotation.x = Mathf.Clamp(this._LocalRotation.x, -0.1f,  0.1f);
                this._LocalRotation.y = Mathf.Clamp(this._LocalRotation.y, -0.1f,  0.1f);

                this.Position = new Vector3(
                    Mathf.Lerp(this.Position.y, this.Position.y + this._LocalRotation.y, Time.deltaTime * OrbitDampening),
                    Mathf.Lerp(this.Position.x, this.Position.x + this._LocalRotation.x, Time.deltaTime * OrbitDampening),
                    this.Position.z);
            }

            //Zooming Input from our Mouse Scroll Wheel
            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                float ScrollAmount = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitvity;

                ScrollAmount *= (this._CameraDistance * 0.3f); // Dampening the value : 가까울수록 줌 느리게 함
                this._CameraDistance += ScrollAmount * -1f;
                this._CameraDistance = Mathf.Clamp(this._CameraDistance, 0.3f,  3f);

                this.Position = new Vector3(this.Position.x, this.Position.y, Mathf.Lerp(this.Position.z, this._CameraDistance, Time.deltaTime * ScrollDampening));
            }            
        }

 
        this.Parent.rotation = this.Rot;
        this.Camera.localRotation = this.initialCamRot;
        this.Camera.localPosition = this.Position;

        
    }
}
