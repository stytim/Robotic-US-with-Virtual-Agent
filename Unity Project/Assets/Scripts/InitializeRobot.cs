using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeRobot : MonoBehaviour
{
    private ArticulationBody[] articulationChain;
    public float stiffness = 10000;
    public float damping = 100;
    public float forceLimit = 1000;
    public float jointFriction = 1;
    public float angularDamping = 1;

    // Start is called before the first frame update
    void Start()
    {
        articulationChain = this.GetComponentsInChildren<ArticulationBody>();
        ChangePhysicalProperties();

    }

    [ContextMenu("Change Physical Properties")]
    void ChangePhysicalProperties()
    {
        foreach (ArticulationBody joint in articulationChain)
        {
            joint.jointFriction = jointFriction;
            joint.angularDamping = angularDamping;
            ArticulationDrive currentDrive = joint.xDrive;
            currentDrive.forceLimit = forceLimit;
            currentDrive.stiffness = stiffness;
            currentDrive.damping = damping;
            joint.xDrive = currentDrive;
        }
    }
}
