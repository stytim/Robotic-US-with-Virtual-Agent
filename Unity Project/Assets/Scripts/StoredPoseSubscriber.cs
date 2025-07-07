using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using std_msgs = RosMessageTypes.Std;

public class StoredPoseSubscriber : MonoBehaviour
{
    ROSConnection m_Ros;
    public string m_TopicName = "/pose_storage_signal";
    MotionSimulation motionSimulation;
    // Start is called before the first frame update
    void Start()
    {
        motionSimulation = GetComponent<MotionSimulation>();
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.Subscribe<std_msgs.StringMsg>(m_TopicName, StoredSignalCallback);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StoredSignalCallback(std_msgs.StringMsg msg)
    {
        if (msg.data == "First")
        {
            Debug.Log("Recorded First Pose");
            motionSimulation.RecordStartPose();
        }
        else if (msg.data == "Second")
        {
            Debug.Log("Recorded Second Pose");
            motionSimulation.RecordEndPose();
        }
    }
}
