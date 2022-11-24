using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldScript : MonoBehaviour
{
    public DateTime startTime;

    private TimeSpan remainingTime;
    private TimeSpan tenSeconds;

    Vector3 velocity = Vector3.zero;

    private Vector3 targetPosition1;
    private Vector3 targetPosition2;
    
    void Start()
    {
        startTime = new DateTime(2021, 9, 10, 20, 52, 20); // set the time starting first round
        tenSeconds = new TimeSpan(0, 0, 10);
        targetPosition1 = transform.position + new Vector3(0f, 2.25f, 0f);
        targetPosition2 = transform.position + new Vector3(0f, 0f, 10f);
    }

    void Update()
    {
        remainingTime = startTime-DateTime.Now;

        // switch from Clock Panel to Time Panel 10seconds before startingTime
        if(TimeSpan.Compare(remainingTime, tenSeconds) != 1 && TimeSpan.Compare(remainingTime, TimeSpan.Zero) != -1){
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition1, ref velocity, 2.0f);
        }

        // vanish Clock Panel after startingTime
        if(TimeSpan.Compare(DateTime.Now-startTime, TimeSpan.Zero) == 1){
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition2, ref velocity, 1.0f);
        }

        if(TimeSpan.Compare(DateTime.Now-startTime, tenSeconds) == 1){
            Destroy(this.gameObject);
            this.enabled = false;
        }

    }
    
}
