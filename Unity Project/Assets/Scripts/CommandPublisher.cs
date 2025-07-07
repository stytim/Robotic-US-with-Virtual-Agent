using RosMessageTypes.Std;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class CommandPublisher : MonoBehaviour
{
    ROSConnection m_Ros;
    public string m_TopicName = "/robot_command";
    public string command = "none";
    // Start is called before the first frame update
    void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<StringMsg>(m_TopicName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Test Command")]
    void TestCommand()
    {
        PublishRobotCommand(command);
    }

    public void PublishRobotCommand(string command)
    {
        StringMsg msg = new StringMsg();
        msg.data = command.ToLower();
        m_Ros.Publish(m_TopicName, msg);
        //Debug.Log("Command published");
    }
}
