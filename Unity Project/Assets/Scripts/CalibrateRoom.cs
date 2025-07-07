using System.Collections;
using System.Collections.Generic;
using MaLibU.Calibration;
using UnityEngine;

public class CalibrateRoom : MonoBehaviour
{
    [Header ("Room")]
    public Transform[] roomInPoints;
    public MachineHome roomMachineHome;
    public GameObject room;

    [Header("Robot")]
    public Transform[] robotInPoints;
    public MachineHome robotMachineHome;
    public GameObject robot;

    Kabsch kabschCalib;
    ControllerScript controllerScript;
    // Start is called before the first frame update
    void Start()
    {
        kabschCalib = GetComponent<Kabsch>();
        controllerScript = GetComponent<ControllerScript>();
    }

    // Update is called once per frame
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, Screen.height - 80, 140, 120));
        if (GUILayout.Button("Calibrate Room"))
        {
            StartCalibrateRoom();
        }
        if (GUILayout.Button("Calibrate Robot"))
        {
            StartCalibrateRobot();
        }
        GUILayout.EndArea();
    }

    void StartCalibrateRoom()
    {
        kabschCalib.inPoints = roomInPoints;
        controllerScript.robot = room;
        controllerScript.machineHome = roomMachineHome;
    }

    void StartCalibrateRobot()
    {
        kabschCalib.inPoints = robotInPoints;
        controllerScript.robot = robot;
        controllerScript.machineHome = robotMachineHome;
    }
}
