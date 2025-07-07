using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class MachineHome : MonoBehaviour
{
    public static MachineHome Instance = null;

    public GameObject robot;
    public bool isRobot = true;

    [SerializeField] string _saveAnchorKey = "anchor";

   // public string path = "/RobotCalibration.txt";

    public Matrix4x4 Origin2Home
    {
        get
        {
            return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        }
    }

    private void Awake()
    {
        Instance = this;

    }
    private void LateUpdate()
    {

    }

    private void Start()
    {
        StartCoroutine(LoadAnchorCoroutine());
    }

    private void OnApplicationFocus(bool focus)
    {
        if(focus)
        {
            StartCoroutine(LoadAnchorCoroutine());
        }
    }

    private IEnumerator LoadAnchorCoroutine()
    {
        Debug.Log("Loading Anchor");
        yield return SpatialAnchorHandler.LoadAnchor(gameObject, _saveAnchorKey);
        Vector3 pos = gameObject.transform.position;
        Quaternion rot = gameObject.transform.rotation;
        robot.transform.position = pos;
        robot.transform.rotation = rot;
        if (isRobot)
            robot.GetComponentInChildren<ArticulationBody>().TeleportRoot(pos, rot);
    }


    public void AddAnchor(Vector3 pos, Quaternion rot)
    {
        StartCoroutine(Calibration(pos, rot));
    }

    private IEnumerator Calibration(Vector3 pos, Quaternion rot)
    {
        yield return SpatialAnchorHandler.EraseAnchor(gameObject);

        robot.transform.position = pos;
        robot.transform.rotation = rot;
        if (isRobot)
            robot.GetComponentInChildren<ArticulationBody>().TeleportRoot(pos, rot);

        yield return SpatialAnchorHandler.CreateAndSaveAnchor(gameObject, _saveAnchorKey);
    }

}
