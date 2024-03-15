using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_eixo : MonoBehaviour
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
        float[] thetas_3 = { -37.639f, -16.339f, -18.86f, -16.665f, -20.52f };
        HingeJoint hinge = GetComponent<HingeJoint>();

        JointMotor motor = hinge.motor;
        motor.force = 50;
        motor.targetVelocity = -17;
        hinge.motor = motor;

        JointLimits limits = hinge.limits;
        limits.min = thetas_3[0];
        hinge.limits = limits;

        yield return new WaitForSeconds(10);

        motor.force = 0.1f;
        motor.targetVelocity = 0.5f;
        hinge.motor = motor;

        limits.max = thetas_3[1];
        hinge.limits = limits;

        yield return new WaitForSeconds(10);

        motor.force = 50;
        motor.targetVelocity = -17;
        hinge.motor = motor;

        limits.min = thetas_3[2];
        hinge.limits = limits;

        yield return new WaitForSeconds(10);

        motor.force = 1;
        motor.targetVelocity = 0.5f;
        hinge.motor = motor;

        limits.max = thetas_3[3];
        hinge.limits = limits;

        yield return new WaitForSeconds(10);

        motor.force = 50;
        motor.targetVelocity = -17;
        hinge.motor = motor;

        limits.min = thetas_3[4];
        hinge.limits = limits;

        yield break;
    }
}
