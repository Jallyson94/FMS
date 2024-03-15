using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Elos : MonoBehaviour
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
        float[] thetas_2 = { 94.7522f, 95.9365f, 94.6825f, 89.896f, 77.3213f };
        HingeJoint hinge = GetComponent<HingeJoint>();

        JointMotor motor = hinge.motor;
        motor.force = 20;
        motor.targetVelocity = 12;
        hinge.motor = motor;

        JointLimits limits = hinge.limits;
        limits.max = thetas_2[0];
        hinge.limits = limits;

        yield return new WaitForSeconds(5);

        motor.targetVelocity = 12;
        hinge.motor = motor;

        limits.max = thetas_2[1];
        hinge.limits = limits;

        yield return new WaitForSeconds(5);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_2[2];
        hinge.limits = limits;

        yield return new WaitForSeconds(5);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_2[3];
        hinge.limits = limits;

        yield return new WaitForSeconds(5);

        motor.targetVelocity = -12;
        hinge.motor = motor;

        limits.min = thetas_2[4];
        hinge.limits = limits;

        yield break;
    }
}
