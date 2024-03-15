using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Base2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Movement());
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator Movement()
    {
        yield return new WaitForSeconds(10);
        float[] thetas_1 = { -11.6327f, -45.9491f, -79.67f, -121.659f, -153.435f };

        HingeJoint hinge = GetComponent<HingeJoint>();

        JointMotor motor = hinge.motor;
        motor.force = 20;
        motor.targetVelocity = -12;
        hinge.motor = motor;

        JointLimits limits = hinge.limits;
        limits.max = thetas_1[0];
        hinge.limits = limits;

        yield return new WaitForSeconds(3);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_1[1];
        hinge.limits = limits;

        yield return new WaitForSeconds(3);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_1[2];
        hinge.limits = limits;

        yield return new WaitForSeconds(3);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_1[3];
        hinge.limits = limits;

        yield return new WaitForSeconds(3);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_1[4];
        hinge.limits = limits;

        yield break;
    }
}
