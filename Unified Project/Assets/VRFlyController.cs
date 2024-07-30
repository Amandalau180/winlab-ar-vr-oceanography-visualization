using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem; // Ensure you have the Input System package installed

public class VRFlyController : MonoBehaviour
{
    public OVRCameraRig cameraRig;
    public float baseSpeed = 2.0f;
    public float speedUpFactor = 2.0f;
    public float slowDownFactor = 0.5f;

    // Joystick and trigger inputs
    private Vector2 rightJoystickInput;
    private float leftTriggerInput;
    private float rightTriggerInput;

    void Start()
    {

    }

    void Update()
    {
        ReadInputs();

        ApplyMovement();
    }

    void ReadInputs()
    {
        if (XRSettings.isDeviceActive)
        {
            rightJoystickInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            leftTriggerInput = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            rightTriggerInput = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        }
    }

    void ApplyMovement()
    {
        if (rightJoystickInput != Vector2.zero)
        {
            Vector3 forwardMovement = cameraRig.centerEyeAnchor.forward * rightJoystickInput.y;
            Vector3 strafeMovement = cameraRig.centerEyeAnchor.right * rightJoystickInput.x;

            float currentSpeed = baseSpeed;

            if (rightTriggerInput > 0)
            {
                currentSpeed *= speedUpFactor * rightTriggerInput;
            }
            else if (leftTriggerInput > 0)
            {
                currentSpeed *= slowDownFactor * (1 - leftTriggerInput);
            }

            cameraRig.transform.position += (forwardMovement + strafeMovement) * currentSpeed * Time.deltaTime;
        }
    }
}
