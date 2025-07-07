using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryFollower : MonoBehaviour
{
    public TrajectorySubscriber trajectorySubscriber;
    public LineRenderer trajectoryLine;
    public float duration = 3f; // Total time to traverse the trajectory
    public AnimationCurve speedCurve; // Speed curve for non-uniform movement
    public MovementController movementController;

    private Transform trajectoryPoint;
    private bool isMoving = false;

    void Start()
    {
        trajectoryPoint = trajectorySubscriber.trajectoryPoint.transform;
    }

    [ContextMenu("Start Movement")]
    public void StartMoving()
    {
        if (!isMoving && trajectoryLine.positionCount > 1)
        {
            movementController.PointTrajectory();
            StartCoroutine(MoveAlongTrajectory());
        }
    }

    private IEnumerator MoveAlongTrajectory()
    {
        isMoving = true;
        List<TrajectorySubscriber.TrajectoryPoint> points_l = trajectorySubscriber.trajectoryPoints;
        Vector3[] points = new Vector3[trajectoryLine.positionCount];
        trajectoryLine.GetPositions(points);

        if (points.Length < 2)
        {
            Debug.LogWarning("Not enough points to move along the trajectory.");
            isMoving = false;
            yield break;
        }

        trajectoryPoint.position = trajectoryLine.transform.TransformPoint(points[0]);
       // trajectoryPoint.rotation = points_l[0].rotation;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 start = trajectoryLine.transform.TransformPoint(points[i]);
            Vector3 end = trajectoryLine.transform.TransformPoint(points[i + 1]);

            Quaternion startRot = points_l[i].rotation;
            Quaternion endRot = points_l[i + 1].rotation;

            float segmentDistance = Vector3.Distance(start, end);
            float segmentDuration = duration * (segmentDistance / GetTotalDistance(points)); // Proportional time per segment

            float elapsedTime = 0f;

            while (elapsedTime < segmentDuration)
            {
                float t = elapsedTime / segmentDuration;
                float speedFactor = speedCurve.Evaluate(t); // Speed curve adjustment

                trajectoryPoint.position = Vector3.Lerp(start, end, t * speedFactor);
                //trajectoryPoint.rotation = Quaternion.Slerp(startRot, endRot, t * speedFactor);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure exact position & rotation at the end of the segment
            trajectoryPoint.position = end;
            //trajectoryPoint.rotation = endRot;
        }

        isMoving = false;
        movementController.StopPointing();
    }

    // Helper function to get total distance of the trajectory
    private float GetTotalDistance(Vector3[] points)
    {
        float distance = 0f;
        for (int i = 1; i < points.Length; i++)
        {
            distance += Vector3.Distance(
                trajectoryLine.transform.TransformPoint(points[i - 1]),
                trajectoryLine.transform.TransformPoint(points[i])
            );
        }
        return distance;
    }
}
