using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankDrive : MonoBehaviour
{
    private Vector2 movementInput; //store movement input, updated through onMove method
    public Rigidbody rigid;
    public WheelCollider FL, FR, ML, MR, BL, BR;
    public float drivespeed, rotationspeed;


    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
        // get movement input, store it in movementinput var which is vector 2(x and y axis)
    }
    void FixedUpdate()
    {
        float leftSideTorque = (movementInput.y * drivespeed) + (movementInput.x * rotationspeed);
        float rightSideTorque = (movementInput.y * drivespeed) - (movementInput.x * rotationspeed);

        FL.motorTorque = leftSideTorque;
        ML.motorTorque = leftSideTorque;
        BL.motorTorque = leftSideTorque;

        FR.motorTorque = rightSideTorque;
        MR.motorTorque = rightSideTorque;
        BR.motorTorque = rightSideTorque;

    }
}
