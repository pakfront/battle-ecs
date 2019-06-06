// MIT License

// Copyright (c) 2017 Panagiotis Migkotzidis

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;

[RequireComponent(typeof(Camera))]

public class RTSCameraController : MonoBehaviour {

    public float ScreenEdgeBorderThickness = 5.0f; // distance from screen edge. Used for mouse movement

    [Header("Camera Mode")]
    [Space]
    public bool RTSMode = true;
    public bool FlyCameraMode = false;

    [Header("Movement Speeds")]
    [Space]
    public float minPanSpeed = 1;
    public float maxPanSpeed = 1;
    public float secToMaxSpeed = 1; //seconds taken to reach max speed;
    public float zoomSpeed = 1;

    [Header("Movement Limits")]
    [Space]
    public bool enableMovementLimits;
    public Vector2 heightLimit;
    public Vector2 lenghtLimit;
    public Vector2 widthLimit;
    private Vector2 zoomLimit;

    private float panSpeed;
    private Vector3 initialPos;
    private Vector3 panMovement;
    private Vector3 pos;
    private Quaternion rot;
    private bool rotationActive = false;
    private Vector3 lastMousePosition;
    private Quaternion initialRot;
    private float panIncrease = 0.0f;

    [Header("Rotation")]
    [Space]
    public bool rotationEnabled;
    public float rotateSpeed;





    // Use this for initialization
    void Start () {
        initialPos = transform.position;
        initialRot = transform.rotation;
        zoomLimit.x = 15;
        zoomLimit.y = 65;
	}
	
	
	void Update () {

        # region Camera Mode

        //check that ony one mode is choosen
        if (RTSMode == true) FlyCameraMode = false;
        if (FlyCameraMode == true) RTSMode = false;

        # endregion

        #region Movement

            panMovement = Vector3.zero;

            if (Input.GetKey(KeyCode.S) 
                || (Input.mousePosition.y <= Screen.height && Input.mousePosition.y >= Screen.height - ScreenEdgeBorderThickness))
            {
                panMovement += Vector3.forward * panSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.W) 
                || (Input.mousePosition.y == 0 && Input.mousePosition.y <= ScreenEdgeBorderThickness))
            {
                panMovement -= Vector3.forward * panSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D) 
                || (Input.mousePosition.x >= ScreenEdgeBorderThickness && Input.mousePosition.x <= ScreenEdgeBorderThickness))
            {
                panMovement += Vector3.left * panSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A) 
                || (Input.mousePosition.x <- Screen.width && Input.mousePosition.x >= Screen.width - ScreenEdgeBorderThickness))
            {
                panMovement += Vector3.right * panSpeed * Time.deltaTime;
                //pos.x += panSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                panMovement += Vector3.up * panSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.E))
            {
                panMovement += Vector3.down * panSpeed * Time.deltaTime;
            }

            if(RTSMode) transform.Translate(panMovement, Space.World);
            else if(FlyCameraMode) transform.Translate(panMovement, Space.Self);


        //increase pan speed
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) 
            || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)
            || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q)
            || Input.mousePosition.y >= Screen.height - ScreenEdgeBorderThickness
            || Input.mousePosition.y <= ScreenEdgeBorderThickness
            || Input.mousePosition.x <= ScreenEdgeBorderThickness
            || Input.mousePosition.x >= Screen.width - ScreenEdgeBorderThickness)
        {
            panIncrease += Time.deltaTime / secToMaxSpeed;
            panSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, panIncrease);
        }
        else
        {
            panIncrease = 0;
            panSpeed = minPanSpeed;
        }

        #endregion

        #region Zoom

        Camera.main.fieldOfView -= Input.mouseScrollDelta.y * zoomSpeed;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView,zoomLimit.x,zoomLimit.y);

        #endregion

        #region mouse rotation

        if (rotationEnabled)
        {
            // Mouse Rotation
            if (Input.GetMouseButton(0))
            {
                rotationActive = true;
                Vector3 mouseDelta;
                if (lastMousePosition.x >= 0 &&
                    lastMousePosition.y >= 0 &&
                    lastMousePosition.x <= Screen.width &&
                    lastMousePosition.y <= Screen.height)
                    mouseDelta = Input.mousePosition - lastMousePosition;
                else
                {
                    mouseDelta = Vector3.zero;
                }
                var rotation = Vector3.up * Time.deltaTime * rotateSpeed * mouseDelta.x;
                rotation += Vector3.left * Time.deltaTime * rotateSpeed * mouseDelta.y;

                transform.Rotate(rotation, Space.World);

                // Make sure z rotation stays locked
                rotation = transform.rotation.eulerAngles;
                rotation.z = 0;
                transform.rotation = Quaternion.Euler(rotation);
            }

            if (Input.GetMouseButtonUp(0))
            {
                rotationActive = false;
                if (RTSMode) transform.rotation = Quaternion.Slerp(transform.rotation, initialRot, 0.5f * Time.time);
            }

            lastMousePosition = Input.mousePosition;

        }


        #endregion


        #region boundaries

        if (enableMovementLimits == true)
        {
            //movement limits
            pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, heightLimit.x, heightLimit.y);
            pos.z = Mathf.Clamp(pos.z, lenghtLimit.x, lenghtLimit.y);
            pos.x = Mathf.Clamp(pos.x, widthLimit.x, widthLimit.y);
            transform.position = pos;
        }
        


        #endregion

    }

}



