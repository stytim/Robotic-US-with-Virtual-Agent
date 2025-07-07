using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientAvatarControl : MonoBehaviour
{
    public Color skinColor;
    public Material skin;
    private bool isArmFrozen = false;
    // Start is called before the first frame update
    void Start()
    {
        skinColor = skin.color;
    }

    // Update is called once per frame
    void Update()
    {
        skin.color = skinColor;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isArmFrozen)
            {
                UnFreezeArm();
                isArmFrozen = false;
            }
            else
            {
                FreezeArm();
                isArmFrozen = true;
            }
        }
    }

    private void FreezeArm()
    {
        // Freeze the arm
        GameObject rightTarget = GameObject.Find("Right Hand IK Target");
        GameObject leftTarget = GameObject.Find("Left Hand IK Target");

        rightTarget.transform.parent = null;
        leftTarget.transform.parent = null;
        leftTarget.transform.position = transform.position - transform.forward*0.1f;
    }

    private void UnFreezeArm()
    {
        // Unfreeze the arm
        GameObject rightTarget = GameObject.Find("Right Hand IK Target");
        GameObject leftTarget = GameObject.Find("Left Hand IK Target");

        rightTarget.transform.parent = GameObject.Find("RightHandAnchor").transform;
        leftTarget.transform.parent = GameObject.Find("LeftHandAnchor").transform;
    }
}
