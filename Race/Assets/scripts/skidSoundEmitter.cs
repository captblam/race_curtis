﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]

public class skidSoundEmitter : MonoBehaviour

{

    public float skidAt = 1.5f;

    public int soundEmissionPerSecond = 10;

    public AudioClip skidSound;

    private float soundDelay;

    private WheelCollider attachedWheel;

    void Start()

    {

        //always grab the component only once via start, don’t do this in Update().

        attachedWheel = transform.GetComponent<WheelCollider>();

    }
    void Update()

    {

        WheelHit hit;

        if (attachedWheel.GetGroundHit(out hit))

        {

            float frictionValue = Mathf.Abs(hit.sidewaysSlip);

            if (skidAt <= frictionValue && soundDelay <= 0)

            {

                AudioSource.PlayClipAtPoint(skidSound, hit.point);

                soundDelay = 1f;

            }

        }

        soundDelay -= Time.deltaTime * soundEmissionPerSecond;

    }

}

