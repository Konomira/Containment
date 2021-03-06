using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FlyCamera : MonoBehaviourPunCallbacks
{
    public enum MouseButton { Left = 0, Right = 1, Middle = 2 }

    [Header("Dependencies"),Tooltip("Camera to control")]
    public new Camera camera = null;

    [Header("Settings")]

    public bool startWithFreeLook = false;

    public bool fly;

    [Tooltip("Button to be held down for freelook")]
    public MouseButton lookButton = MouseButton.Right;


    [Tooltip("Key to be held down for freelook")]
    public KeyCode lookKey = KeyCode.LeftAlt;

    [Tooltip("Key to toggle freelook on or off")]
    public KeyCode lookToggle = KeyCode.Space;

    [Tooltip("Key to be held down to boost camera movement speed")]
    public KeyCode boostKey = KeyCode.LeftShift;

    [Space]
    public float horizontalSensitivity = 1.0f;
    public float verticalSensitivity = 1.0f;
    [Space]
    public bool invertXAxis = false;
    public bool invertYAxis = false;
    
    [Space,Tooltip("Max up/down angle in degrees")]
    public float maxYAngle = 60.0f;

    [Space,Tooltip("Time scaled camera movement speed")]
    public float moveSpeed = 20.0f;

    [Tooltip("Movement speed is multiplied by this when the boost button is held")]
    public float boostFactor = 2.0f;

    [Space]
    public List<Transform> cameraPositions;

    private Transform lookTarget = null;
    private Coroutine transition = null;
    private bool freelookOn = false;

    private Vector3 positionLastFrame;
    private float yRotationLastFrame;
    
    private void Start()
    {
        freelookOn = startWithFreeLook;
        if (lookTarget == null)
            lookTarget = new GameObject("CameraTarget").transform;
        lookTarget.position = camera.transform.position + camera.transform.forward;
    }

    private void Update()
    {
        Look3D();

        if(fly)
            Fly3D();
        else
            Walk3D();
    }

    private int networkDelayFrames;
    private void FixedUpdate()
    {
        networkDelayFrames = (networkDelayFrames + 1) % 3;

        if(networkDelayFrames != 0) return;
        
        var tr = camera.transform;

        if(camera.transform.position != positionLastFrame || tr.rotation.eulerAngles.y != yRotationLastFrame)
        {
            var posData = new NetworkPlayerSync.PositionData()
            {
                Position = camera.transform.position, 
                YRotation = tr.rotation.eulerAngles.y, 
                ID = PhotonNetwork.LocalPlayer.ActorNumber,
            };
             
            NetworkEvent.RaiseEvent(NetworkEvent.EventCode.Movement, posData.ToObjectArray());
        }

        positionLastFrame = camera.transform.position;
        yRotationLastFrame = tr.rotation.eulerAngles.y;
    }

    private void Look3D()
    {
        if (transition != null)
            return;

        if (Input.GetKeyDown(lookToggle))
            freelookOn = !freelookOn;

        if (!Input.GetMouseButton((int)lookButton) && !Input.GetKey(lookKey) && !freelookOn)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Rotates the look target around the camera instead of directly changing the camera's rotation
        // Doing it this way allows us to remove the need for a camera parent and also gives us
        // control for following other targets if necessary

        var mouse = new Vector2(Input.GetAxis("Mouse X") * horizontalSensitivity, -Input.GetAxis("Mouse Y") * verticalSensitivity);

        if (invertXAxis)
            mouse.x *= -1;

        if (invertYAxis)
            mouse.y *= -1;

        // Grab current position in case the rotation causes the target to exceed maxYAngle
        var oldPos = lookTarget.position;
        lookTarget.RotateAround(camera.transform.position, camera.transform.right, mouse.y);

        var forwardHorizon = camera.transform.forward;
        forwardHorizon.y = 0.0f;

        if (Vector3.Angle(forwardHorizon, (lookTarget.position - camera.transform.position)) > maxYAngle)
            lookTarget.position = oldPos;

        lookTarget.RotateAround(camera.transform.position, Vector3.up, mouse.x);

        camera.transform.LookAt(lookTarget.position, Vector3.up);
    }

    private void Fly3D()
    {
        if (transition != null)
            return;

        var speed = Input.GetKey(boostKey) ? moveSpeed * boostFactor : moveSpeed;

        var delta = new Vector3(Input.GetAxis("HorizontalLR"), Input.GetAxis("Vertical"), Input.GetAxis("HorizontalFB")) * speed * Time.deltaTime;
        camera.transform.Translate(delta, Space.Self);

        lookTarget.transform.position = camera.transform.position + camera.transform.forward;
    }

    private void Walk3D()
    {
        if(transition != null) return;
        
        var speed = Input.GetKey(boostKey) ? moveSpeed * boostFactor : moveSpeed;
        
        var delta = new Vector3(Input.GetAxis("HorizontalLR"), Input.GetAxis("Vertical"), Input.GetAxis("HorizontalFB")) * speed * Time.deltaTime;

        var tr = camera.transform;
        
        //TODO: Base y position on floor + player height
        var oldYPos = tr.position.y;
        
        camera.transform.Translate(delta, Space.Self);
        var pos = tr.position;
        pos.y = oldYPos;

        tr.position = pos;
        
        lookTarget.transform.position = pos + tr.forward;
    }
    
    private IEnumerator SmoothTransition(int index, float stepSpeed = 0.1f)
    {
        yield return StartCoroutine(SmoothTransition(cameraPositions[index], stepSpeed));
    }

    private IEnumerator SmoothTransition(Transform target, float stepSpeed)
    {
        while (Vector3.Distance(camera.transform.position, target.position) > stepSpeed || Quaternion.Angle(camera.transform.rotation, target.rotation) > stepSpeed)
        {
            camera.transform.position = Vector3.MoveTowards(camera.transform.position, target.position, stepSpeed);
            camera.transform.rotation = Quaternion.Lerp(camera.transform.rotation, target.rotation, stepSpeed);

            lookTarget.position = camera.transform.position + camera.transform.forward;

            yield return null;
        }

        camera.transform.position = target.position;
        camera.transform.rotation = target.rotation;
        transition = null;
    }

    public void Transition(Transform target, float stepSpeed = 0.1f) => transition = StartCoroutine(SmoothTransition(target, stepSpeed));
    public void Transition(int index, float stepSpeed = 0.1f) => transition = StartCoroutine(SmoothTransition(cameraPositions[index], stepSpeed));
    
}