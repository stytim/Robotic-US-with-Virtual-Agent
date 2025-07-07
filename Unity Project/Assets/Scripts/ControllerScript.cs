using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerScript : MonoBehaviour
{
    public Transform rightController;
    public Transform leftController;
    private Transform currentController;
    // Reference to the calibration point prefab
    public GameObject calibrationPointPrefab;
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;
    public float spawnDistance = 0.1f; // 10 cm
    public GameObject robot;
    public MachineHome machineHome;

    public OVRInput.Axis1D targetTrigger;

    MaLibU.Calibration.Kabsch kabsch;
    List<Transform> refPoints;

    public int maxPoints = 5;

    int count = 0;

    private Vector3 relativeTranslation;
    private Quaternion relativeRotation;
    // Start is called before the first frame update
    private void Awake()
    {
        relativeTranslation = robot.transform.position - machineHome.transform.position;
        relativeRotation = Quaternion.Inverse(machineHome.transform.rotation) * robot.transform.rotation;
    }
    void Start()
    {
        kabsch = GetComponent<MaLibU.Calibration.Kabsch>();
        refPoints = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.One, controller))
        {
            currentController = rightController;
        }
        else if (OVRInput.GetUp(OVRInput.Button.Three, controller))
        {
            currentController = leftController;
        }

        float triggerValue = OVRInput.Get(targetTrigger, controller);
        // If user has just released Button A of right controller in this frame
        if ((OVRInput.GetUp(OVRInput.Button.One, controller) || OVRInput.GetUp(OVRInput.Button.Three, controller)) && count < maxPoints)
        {
            // Only proceed if the trigger was NOT held down during the release
            if (triggerValue < 0.1f)
            {
                Vector3 spawnPosition = currentController.position + currentController.forward * spawnDistance;
                SpawnCalibrationPoint(spawnPosition);
                count++;
            }
        }

        if (OVRInput.GetUp(OVRInput.Button.Two, controller) || OVRInput.GetUp(OVRInput.Button.Four, controller))
        {
            if (triggerValue < 0.1f)
            {
                if (refPoints.Count < maxPoints)
                {
                    Debug.Log("Need" + maxPoints  + "reference points to calibrate");
                    return;
                }
                kabsch.referencePoints = refPoints.ToArray();
                Vector3 pos;
                Quaternion rot;
                kabsch.align(out rot, out pos);
                rot.eulerAngles = new Vector3(0, rot.eulerAngles.y, 0);
                machineHome.AddAnchor(pos, rot);
            }
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick) || OVRInput.GetUp(OVRInput.Button.SecondaryThumbstick))
        {
            Debug.Log(" Deleting all reference points");
            count = 0;
            foreach (Transform refPoint in refPoints)
            {
                Destroy(refPoint.gameObject);
            }
            refPoints.Clear();
        }

    }

    void SpawnCalibrationPoint(Vector3 spawnPosition)
    {
        // Instantiate the calibration point prefab at the calculated position and rotation
        GameObject refPoint = Instantiate(calibrationPointPrefab, spawnPosition, Quaternion.identity);

        // Add the spawned calibration point to the list of reference points
        refPoints.Add(refPoint.transform);

    }

}