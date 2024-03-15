using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_apoio : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(movement());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator movement()
    {
        float[] thetas_5 = { 147.113f, 169.598f, 165.823f, 163.228f, 146.802f };
        HingeJoint hinge = GetComponent<HingeJoint>();

        JointMotor motor = hinge.motor;
        motor.force = 20;
        motor.targetVelocity = 12;
        hinge.motor = motor;

        JointLimits limits = hinge.limits;
        limits.max = thetas_5[0];
        hinge.limits = limits;

        yield return new WaitForSeconds(20);

        motor.targetVelocity = 12;
        hinge.motor = motor;

        limits.max = thetas_5[1];
        hinge.limits = limits;

        yield return new WaitForSeconds(20);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_5[2];
        hinge.limits = limits;

        yield return new WaitForSeconds(20);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_5[3];
        hinge.limits = limits;

        yield return new WaitForSeconds(10);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_5[4];
        hinge.limits = limits;

        yield break;
    }
}
