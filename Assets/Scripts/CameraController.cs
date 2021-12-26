using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform cameraTransform;
    Transform followTransform;
    Player player;
    float movementTime = 10;
    float rotationSpeed = 100;
    Vector3 zoomSpeed = new Vector3(0, -200, 200);
    float maxZoomIn = 20;
    float maxZoomOut = 100;

    Vector3 newPosition;
    Quaternion newRotation;
    Vector3 newZoom;

    Vector3 rotationStartPosition;
    Vector3 rotationCurrentPosition;

    private void Start()
    {
        player = Player.Instance;
        cameraTransform = Camera.main.transform;
        followTransform = player.transform;
        newPosition = ES3.Load("cameraPosition", transform.position);
        newRotation = ES3.Load("cameraRotation", transform.rotation);
        newZoom = ES3.Load("cameraZoom", cameraTransform.localPosition);
        if (followTransform)
            newPosition = followTransform.position;
    }

    private void LateUpdate()
    {
        if (followTransform)
            newPosition = followTransform.position;

        HandleCameraInput();

        transform.position = Vector3.Lerp(transform.position, newPosition, movementTime * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, movementTime * Time.deltaTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, movementTime * Time.deltaTime);
    }

    void HandleCameraInput()
    {
        newRotation *= Quaternion.Euler(Vector3.up * Input.GetAxis("RightHorizontal") * rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.Q))
            newRotation *= Quaternion.Euler(Vector3.up * rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.E))
            newRotation *= Quaternion.Euler(Vector3.up * -rotationSpeed * Time.deltaTime);
        if (Input.GetMouseButtonDown(2))
            rotationStartPosition = Input.mousePosition;
        if (Input.GetMouseButton(2))
        {
            rotationCurrentPosition = Input.mousePosition;
            Vector3 difference = rotationStartPosition - rotationCurrentPosition;
            rotationStartPosition = rotationCurrentPosition;
            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5));
        }

        if (Input.GetKey(KeyCode.R))
            newZoom += zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.F))
            newZoom -= zoomSpeed * Time.deltaTime;
        if (Input.mouseScrollDelta.y != 0)
            newZoom += Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime * 5;
        newZoom += Input.GetAxis("RightVertical") * zoomSpeed * Time.deltaTime;
        newZoom.y = Mathf.Clamp(newZoom.y, maxZoomIn, maxZoomOut);
        newZoom.z = Mathf.Clamp(newZoom.z, -maxZoomOut, -maxZoomIn);
    }

    private void OnApplicationQuit()
    {
        if (player)
        {
            ES3.Save("cameraPosition", transform.position);
            ES3.Save("cameraRotation", transform.rotation);
            ES3.Save("cameraZoom", cameraTransform.localPosition);
        }
    }
}

