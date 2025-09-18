using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    [Header("IK Chain")]
    [Tooltip("The list of joints in the chain, from base to the one before the end effector.")]
    public List<Transform> Joints;

    [Tooltip("The end effector, the 'hand' of the arm.")]
    public Transform EndEffector;

    [Tooltip("The target the arm will try to reach.")]
    public Transform Target;

    [Header("IK Parameters")]
    [Tooltip("The number of iterations per frame. Higher is more accurate but more costly.")]
    [Range(1, 20)]
    public int Iterations = 10;

    [Tooltip("The minimum distance between end effector and target to stop iterating.")]
    public float Tolerance = 0.05f;

    void LateUpdate()
    {
        // If we have a target, solve the IK chain
        if (Target != null)
        {
            SolveIK();
        }
    }

    void SolveIK()
    {
        // Guard clause: We need at least one joint
        if (Joints == null || Joints.Count == 0)
        {
            return;
        }

        // Calculate the initial distance to the target
        float distanceToTarget = Vector3.Distance(EndEffector.position, Target.position);
        
        // Loop through a set number of iterations
        for (int i = 0; i < Iterations && distanceToTarget > Tolerance; i++)
        {
            // Iterate backwards through the joints, from the one closest to the end effector
            for (int j = Joints.Count - 1; j >= 0; j--)
            {
                // Get the current joint
                Transform currentJoint = Joints[j];

                // 1. Vector from the current joint to the end effector
                Vector3 jointToEndEffector = EndEffector.position - currentJoint.position;
                
                // 2. Vector from the current joint to the target
                Vector3 jointToTarget = Target.position - currentJoint.position;

                // 3. Calculate the rotation needed to align the two vectors
                // We use Quaternion.FromToRotation to find the rotation that would turn
                // jointToEndEffector to point in the same direction as jointToTarget.
                Quaternion rotation = Quaternion.FromToRotation(jointToEndEffector, jointToTarget);

                // 4. Apply the rotation to the current joint
                // We multiply the calculated rotation by the joint's current rotation
                // to combine them. Order matters here.
                currentJoint.rotation = rotation * currentJoint.rotation;
            }

            // Recalculate the distance to see if we've reached the target
            distanceToTarget = Vector3.Distance(EndEffector.position, Target.position);
        }
    }
}