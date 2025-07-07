using UnityEngine;
using UnityEngine.UI; // Make sure this namespace is correct
using Unity.Robotics.ROSTCPConnector; // Make sure this namespace is correct

public class RosStatusChecker : MonoBehaviour
{
    ROSConnection m_RosConnection;
    public Image statusIndicator; // Reference to the UI Image component

    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
    }

    void Update()
    {
        if (m_RosConnection.HasConnectionThread && !m_RosConnection.HasConnectionError)
        {
            //Debug.Log("ROS Connection Status: Connected and Operational");
            statusIndicator.color = Color.green; // Change to green if connected
            // You are likely connected and can send/receive messages.
        }
        else if (m_RosConnection.HasConnectionThread && m_RosConnection.HasConnectionError)
        {
            statusIndicator.color = Color.red; // Change to red if there's an error
            // Debug.LogWarning("ROS Connection Status: Attempting to connect or Connection Error encountered.");
            // Connection thread is running, but an error flag is set.
            // This could mean it's trying to reconnect or failed.
        }
        else if (!m_RosConnection.HasConnectionThread)
        {
            Debug.Log("ROS Connection Status: Not Connected (Connection thread is not active).");
            // Explicitly disconnected or Connect() was never called or connection thread terminated.
        }
    }

    public bool IsConnected()
    {
        if (m_RosConnection == null)
            m_RosConnection = ROSConnection.GetOrCreateInstance();
        return m_RosConnection.HasConnectionThread && !m_RosConnection.HasConnectionError;
    }
}