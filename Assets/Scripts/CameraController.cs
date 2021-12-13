using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public Transform cameraTransform;
    public Transform followTransform;

    float movementTime = 10;
    float rotationSpeed = 100;
    Vector3 zoomSpeed = new Vector3(0, -200, 200);
    float maxZoomIn = 25;
    float maxZoomOut = 100;

    Vector3 newPosition;
    Quaternion newRotation;
    Vector3 newZoom;

    Vector3 rotationStartPosition;
    Vector3 rotationCurrentPosition;

    private void Start()
    {
        if (instance && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
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

    private void OnDestroy()
    {
        if (this == instance)
            instance = null;
    }
}

