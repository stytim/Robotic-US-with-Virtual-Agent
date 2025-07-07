using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using std_msgs = RosMessageTypes.Std;
using RosMessageTypes.Std;

public class StartEndSignalPublisher : MonoBehaviour
{
    public MovementController movementController;
    ROSConnection m_Ros;
    //public string m_TopicName = "/start_signal";
    public string m_FinalPoseTopic = "/robot/end";
    // Start is called before the first frame update
    void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        //m_Ros.RegisterPublisher<BoolMsg>(m_TopicName);
        m_Ros.Subscribe<BoolMsg>(m_FinalPoseTopic, FinalPoseSignal);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void SendStartSignal(bool isStart)
    //{
    //    Debug.Log("Sending start signal: " + isStart);
    //    BoolMsg msg = new BoolMsg();
    //    msg.data = isStart;
    //    m_Ros.Publish(m_TopicName, msg);
    //    Debug.Log("Published start signal: " + isStart);
    //}

    public void FinalPoseSignal(BoolMsg msg)
    {
        if (msg.data)
        {
            Debug.Log("Final pose reached");
            // Stop annimation, TTS "Final pose reached"
            GetComponent<ConversationHandler>().procedureEnded = true;
            movementController.StopInteraction();
        }
    }



}
