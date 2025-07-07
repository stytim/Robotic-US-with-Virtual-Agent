using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class TrajectorySubscriber : MonoBehaviour
{
    ROSConnection m_Ros;
    public string m_TopicName = "/trajTopic";
    public LineRenderer trajectoryLine;
    public bool pauseUpdate = false;
    public LLMHandler llmHandler;

    [SerializeField]
    Transform pointCloudParent; // Parent transform for organizing the point cloud

    [SerializeField]
    Transform RealsensePose; // Parent transform for organizing the point cloud

    public GameObject trajectoryPoint;

    // Struct to hold position and rotation
    public struct TrajectoryPoint
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    // List of trajectory points
    public List<TrajectoryPoint> trajectoryPoints = new List<TrajectoryPoint>();
    //private List<GameObject> trajectoryPointObjects = new List<GameObject>(); // Store instantiated objects
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.Subscribe<PoseArrayMsg>(m_TopicName, ReceiveTrajectory);
    }

    // Update is called once per frame
    void Update()
    {
    }


    [ContextMenu("Refresh Trajectory")]
    public void RefreshPointCloud()
    {
        pauseUpdate = false;
    }

    void ReceiveTrajectory(PoseArrayMsg msg)
    {
        Debug.Log("Trajectory msg received");
        if (!pauseUpdate || true)
        {

            pointCloudParent.position = RealsensePose.position;
            pointCloudParent.rotation = RealsensePose.rotation;
            // Get the poses from the message
            PoseMsg[] poses = msg.poses;

            // Clear previous trajectory objects
            //foreach (GameObject obj in trajectoryPointObjects)
            //{
            //    Destroy(obj);
            //}
            //trajectoryPointObjects.Clear();

            // Check if the count matches the existing trajectory points
            if (trajectoryPoints.Count != poses.Length)
            {
                // Clear previous data if count is different
                trajectoryPoints.Clear();

                // Loop through the poses and create new trajectory points
                foreach (PoseMsg pose in poses)
                {
                    Vector3 position = pose.position.From<FLU>();
                    Quaternion rotation = pose.orientation.From<FLU>();
                    trajectoryPoints.Add(new TrajectoryPoint { position = position, rotation = rotation });
                }
            }
            else
            {
                // Modify existing trajectory points if count matches
                for (int i = 0; i < poses.Length; i++)
                {
                    trajectoryPoints[i] = new TrajectoryPoint
                    {
                        position = poses[i].position.From<FLU>(),
                        rotation = poses[i].orientation.From<FLU>()
                    };
                }
            }

            // Update the LineRenderer to visualize the trajectory
            trajectoryLine.useWorldSpace = false;
            //trajectoryLine.material.color = Color.red;
            trajectoryLine.positionCount = trajectoryPoints.Count;
            trajectoryLine.SetPositions(trajectoryPoints.ConvertAll(p => p.position).ToArray());

            Debug.Log("Trajectory updated");
            pauseUpdate = true;

            if (trajectoryPoints.Count > 0)
            {
                llmHandler.DoctorQuestion("System: Preparation Done");
            }

        }
    }
}
