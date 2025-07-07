using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Iiwa;


public class KukaJointStatesSubscriber : MonoBehaviour
{
    ROSConnection m_Ros;
    ArticulationBody[] m_UrdfJoint;
    public string m_TopicName = "/iiwa/state/JointPosition";
    //public bool m_UseSimulationTopic = false;
    public string m_SimulationTopicName = "/iiwa/joint_states";

    // Start is called before the first frame update
    void Start()
    {
        m_UrdfJoint = GetComponentsInChildren<ArticulationBody>();
        m_Ros = ROSConnection.GetOrCreateInstance();
        
        //if (m_UseSimulationTopic)
            m_Ros.Subscribe<JointStateMsg>(m_SimulationTopicName, SetSimulationJoints);
       // else
            m_Ros.Subscribe<JointPositionMsg>(m_TopicName, SetJoints);
    }   

    // Update is called once per frame
    void Update()
    {

    }

    void SetJoints(JointPositionMsg msg)
    {
        var positions = new float[]
        {
        (float)msg.position.a1,
        (float)msg.position.a2,
        (float)msg.position.a3,
        (float)msg.position.a4,
        (float)msg.position.a5,
        (float)msg.position.a6,
        (float)msg.position.a7
        };

        // Set the joint angles if the type is revolute
        for (int i = 0; i < m_UrdfJoint.Length; i++)
        {
            var body = m_UrdfJoint[i];
            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                var jointXDrive = body.xDrive;
                jointXDrive.target = positions[i - 1] * Mathf.Rad2Deg;
                body.xDrive = jointXDrive;
            }
        }
    }

    void SetSimulationJoints(JointStateMsg msg)
    {
        var positions = msg.position;
        // Set the joint angles if the type is revolute
        for (int i = 0; i < m_UrdfJoint.Length; i++)
        {
            var body = m_UrdfJoint[i];
            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                var jointXDrive = body.xDrive;
                jointXDrive.target = (float)positions[i - 1] * Mathf.Rad2Deg;
                body.xDrive = jointXDrive;
            }
        }
    }

    public void DisableArticulationBody(GameObject robot)
    {
        ArticulationBody[] articulationBody = robot.GetComponentsInChildren<ArticulationBody>();
        foreach (var body in articulationBody)
        {
            body.enabled = false;
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromTopic();
    }

    void OnApplicationQuit()
    {
        UnsubscribeFromTopic();
    }

    private void UnsubscribeFromTopic()
    {
        if (m_Ros != null)
        {
            m_Ros.Unsubscribe(m_TopicName);
            Debug.Log("Unsubscribed from " + m_TopicName + " topic.");

            m_Ros.Unsubscribe(m_SimulationTopicName);
            Debug.Log("Unsubscribed from " + m_SimulationTopicName + " topic.");
            Destroy(m_Ros);
        }
    }
}
