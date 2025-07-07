using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KukaRobotJointControl : MonoBehaviour
{
    [Range(-360.0f, 360.0f)]
    public float link_1;

    [Range(-360.0f, 360.0f)]
    public float link_2;

    [Range(-360.0f, 360.0f)]
    public float link_3;

    [Range(-360.0f, 360.0f)]
    public float link_4;

    [Range(-360.0f, 360.0f)]
    public float link_5;

    [Range(-360.0f, 360.0f)]
    public float link_6;

    [Range(-360.0f, 360.0f)]
    public float link_7;

    ArticulationBody[] m_UrdfJoint;
    // Start is called before the first frame update
    void Start()
    {
        m_UrdfJoint = GetComponentsInChildren<ArticulationBody>();
        //SetHome();
    }

    // Update is called once per frame
    void Update()
    {
       
        SetJointValue();
    }

    void SetHome()
    {
        link_1 = 0.0f;
        link_2 = 0.0f;
        link_3 = 0.0f;
        link_4 = 0.0f;
        link_5 = 0.0f;
        link_6 = 0.0f;
        link_7 = 0.0f;
    }

    private void SetJointValue()
    {
        foreach (var body in m_UrdfJoint)
        {
            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                var jointXDrive = body.xDrive;
                switch (body.name)
                {
                    case "iiwa_link_1":
                        jointXDrive.target = link_1;
                        break;
                    case "iiwa_link_2":
                        jointXDrive.target = link_2;
                        break;
                    case "iiwa_link_3":
                        jointXDrive.target = link_3;
                        break;
                    case "iiwa_link_4":
                        jointXDrive.target = link_4;
                        break;
                    case "iiwa_link_5":
                        jointXDrive.target = link_5;
                        break;
                    case "iiwa_link_6":
                        jointXDrive.target = link_6;
                        break;
                    case "iiwa_link_7":
                        jointXDrive.target = link_7;
                        break;
                    default:
                        Debug.LogError("Invalid link name");
                        break;
                }
                body.xDrive = jointXDrive;
            }
        }
    }
}
