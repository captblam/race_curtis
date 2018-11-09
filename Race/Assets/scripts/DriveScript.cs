using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveScript : MonoBehaviour
{

    public int numberOfGears;
    private float gearSpread;
    public float maxTurnAngle;
    public float maxTorque;
    public WheelCollider fRight;
    public WheelCollider fLeft;
    public WheelCollider bRight;
    public WheelCollider bLeft;
    public Vector3 centerOfMassAdjustment = new Vector3(0f, -0.9f, 0f);
    public float spoilerRatio = 0.1f;
    public float decelerationTorque;
    public Transform wheelTransformFL;
    public GameObject leftBrakeLight;

    public GameObject rightBrakeLight;

    public Texture2D idleLightTex;

    public Texture2D brakeLightTex;

    public Texture2D reverseLightTex;
    public float topSpeed = 150;
    public float maxReverseSpeed = 100;
    private float currentSpeed;
    public float maxBrakeTorque = 100;

    private bool applyHandbrake = false;
    public float handbrakeForwardSlip = 0.04f;

    public float handbrakeSidewaysSlip = 0.08f;

    public Transform wheelTransformFR;

    public Transform wheelTransformBL;

    public Transform wheelTransformBR;

    private Rigidbody body;

    void Start()

    {
        //calculate the spread of top speed over the number of gears.

        gearSpread = topSpeed / numberOfGears;
        //lower center of mass for roll-over resistance

        body = GetComponent<Rigidbody>();

        body.centerOfMass += centerOfMassAdjustment;

    }




    void Update()

    {

        float rotationThisFrame = 360 * Time.deltaTime;

        wheelTransformFL.Rotate(0, -fLeft.rpm / rotationThisFrame, 0);

        wheelTransformFR.Rotate(0, -fRight.rpm / rotationThisFrame, 0);

        wheelTransformBL.Rotate(0, -bLeft.rpm / rotationThisFrame, 0);

        wheelTransformBR.Rotate(0, -bRight.rpm / rotationThisFrame, 0);
        //Adjust the wheels heights based on the suspension.

        UpdateWheelPositions();

        //Determine what texture to use on our brake lights right now.

        DetermineBreakLightState();
        //adjust engine sound

        EngineSound();
    }

    //move wheels based on their suspension.

    void UpdateWheelPositions()

    {

        WheelHit contact = new WheelHit();

        if (fLeft.GetGroundHit(out contact))

        {

            Vector3 temp = fLeft.transform.position;

            //Note: trans.up works for my model, you might need trans.right if you rotated a cylinder!

            temp.y = (contact.point + (fLeft.transform.right * fLeft.radius)).y;

            wheelTransformFL.position = temp;

        }
        if (fRight.GetGroundHit(out contact))

        {

            Vector3 temp = fRight.transform.position;

            temp.y = (contact.point + (fRight.transform.right * fRight.radius)).y;

            wheelTransformFR.position = temp;

        }

        if (bLeft.GetGroundHit(out contact))

        {

            Vector3 temp = bLeft.transform.position;

            temp.y = (contact.point + (bLeft.transform.right * bLeft.radius)).y;

            wheelTransformBL.position = temp;

        }

        if (bRight.GetGroundHit(out contact))

        {

            Vector3 temp = bRight.transform.position;

            temp.y = (contact.point + (bRight.transform.right * bRight.radius)).y;

            wheelTransformBR.position = temp;

        }

    }




    // FixedUpdate is called once per physics frame
    void FixedUpdate()
    {
        //Spoilers add down pressure based on the car’s speed. (Upside-down lift)

        Vector3 localVelocity = transform.InverseTransformDirection(body.velocity);

        body.AddForce(-transform.up * (localVelocity.z * spoilerRatio), ForceMode.Impulse);
        //front wheel steering

        fLeft.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;

        fRight.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;

        //rear wheel drive

        bLeft.motorTorque = Input.GetAxis("Vertical") * maxTorque;

        bRight.motorTorque = Input.GetAxis("Vertical") * maxTorque;


        //apply deceleration when pressing the breaks or lightly when not pressing the gas.

        if (Input.GetAxis("Vertical") <= -0.5f && localVelocity.z > 0)

        {

            bLeft.brakeTorque = decelerationTorque + maxTorque;

            bRight.brakeTorque = decelerationTorque + maxTorque;

        }

        else if (Input.GetAxis("Vertical") == 0)
        {

            bLeft.brakeTorque = decelerationTorque;

            bRight.brakeTorque = decelerationTorque;

        }

        else
        {

            bLeft.brakeTorque = 0;

            bRight.brakeTorque = 0;

        }
        //calculate max speed in KM/H (condensed calculation)

        currentSpeed = bLeft.radius * bLeft.rpm * Mathf.PI * 0.12f;

        if (currentSpeed < topSpeed)

        {

            //rear wheel drive.

            bLeft.motorTorque = Input.GetAxis("Vertical") * maxTorque;

            bLeft.motorTorque = Input.GetAxis("Vertical") * maxTorque;

        }

        else

        {

            //can't go faster, already at top speed that engine produces.

            bLeft.motorTorque = 0;

            bLeft.motorTorque = 0;

        }
        //Handbrake controls

        if (Input.GetButton("Jump"))

        {

            applyHandbrake = true;

            fLeft.brakeTorque = maxBrakeTorque;

            fRight.brakeTorque = maxBrakeTorque;

        }

        else

        {

            applyHandbrake = false;

            fLeft.brakeTorque = 0;

            fRight.brakeTorque = 0;
            //apply deceleration when not pressing the gas or when breaking in either direction.
            if (!applyHandbrake && ((Input.GetAxis("Vertical") <= -0.5f && localVelocity.z > 0) ||
                (Input.GetAxis("Vertical") >= 0.5f && localVelocity.z < 0)))

            {

                bLeft.brakeTorque = decelerationTorque + maxTorque;

                bRight.brakeTorque = decelerationTorque + maxTorque;

            }

            else if (!applyHandbrake && Input.GetAxis("Vertical") == 0)

            {

                bLeft.brakeTorque = decelerationTorque;

                bRight.brakeTorque = decelerationTorque;

            }

            else

            {

                bLeft.brakeTorque = 0;

                bRight.brakeTorque = 0;

            }
            
        }
        //Handbrake controls

        if (Input.GetButton("Jump"))

        {

            applyHandbrake = true;

            fLeft.brakeTorque = maxBrakeTorque;

            fRight.brakeTorque = maxBrakeTorque;

            //Wheels are locked, so power slide!

            if (GetComponent<Rigidbody>().velocity.magnitude > 1)

            {

                SetSlipValues(handbrakeForwardSlip, handbrakeSidewaysSlip);

            }

            else //skid to a stop, regular friction enabled.

            {

                SetSlipValues(1f, 1f);

            }

        }

        else

        {

            applyHandbrake = false;

            fLeft.brakeTorque = 0;

            fRight.brakeTorque = 0;

            SetSlipValues(1f, 1f);

        }

        //apply deceleration when not pressing the gas or when breaking in either direction.

        if (!applyHandbrake && ((Input.GetAxis("Vertical") <= -0.5f && localVelocity.z > 0)
        || (Input.GetAxis("Vertical") >= 0.5f && localVelocity.z < 0)))

        {

            bLeft.brakeTorque = decelerationTorque + maxTorque;

            bRight.brakeTorque = decelerationTorque + maxTorque;

        }
    }
    void SetSlipValues(float forward, float sideways)

    {

        //Change the stiffness values of wheel friction curve and then reapply it.

        WheelFrictionCurve tempStruct = bRight.forwardFriction;

        tempStruct.stiffness = forward;

        bRight.forwardFriction = tempStruct;

        tempStruct = bRight.sidewaysFriction;

        tempStruct.stiffness = sideways;

        bRight.sidewaysFriction = tempStruct;

        tempStruct = bLeft.forwardFriction;

        tempStruct.stiffness = forward;

        bLeft.forwardFriction = tempStruct;

        tempStruct = bLeft.sidewaysFriction;

        tempStruct.stiffness = sideways;

        bLeft.sidewaysFriction = tempStruct;

    }

    void DetermineBreakLightState()

    {

        if ((currentSpeed > 0 && Input.GetAxis("Vertical") < 0)

        || (currentSpeed < 0 && Input.GetAxis("Vertical") > 0)

        || applyHandbrake)

        {

            leftBrakeLight.GetComponent<Renderer>().material.mainTexture = brakeLightTex;

            rightBrakeLight.GetComponent<Renderer>().material.mainTexture = brakeLightTex;

        }

        else if (currentSpeed < 0 && Input.GetAxis("Vertical") < 0)

        {

            leftBrakeLight.GetComponent<Renderer>().material.mainTexture = reverseLightTex;

            rightBrakeLight.GetComponent<Renderer>().material.mainTexture = reverseLightTex;

        }

        else

        {

            leftBrakeLight.GetComponent<Renderer>().material.mainTexture = idleLightTex;

            rightBrakeLight.GetComponent<Renderer>().material.mainTexture = idleLightTex;

        }

    }
    void EngineSound()

    {

        //going forward calculate how far along that gear we are and the pitch sound.

        if (currentSpeed > 0)

        {

            if (currentSpeed > topSpeed)

            {

                GetComponent<AudioSource>().pitch = 1.75f;

            }

            else

            {

                GetComponent<AudioSource>().pitch = ((currentSpeed % gearSpread) / gearSpread) + 0.75f;

            }

        }

        //when reversing we have only one gear.

        else

        {

            GetComponent<AudioSource>().pitch = (currentSpeed / maxReverseSpeed) + 0.75f;

        }

    }

}

