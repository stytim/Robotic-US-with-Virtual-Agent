using UnityEngine;
using UnityEngine.Events;

// Ensure you have the Meta XR SDK imported so OVRInput is available.

public class TriggerButtonComboDetector : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("The OVRInput Axis1D trigger to monitor (e.g., SecondaryHandTrigger for right index).")]
    public OVRInput.Axis1D targetTrigger = OVRInput.Axis1D.SecondaryHandTrigger; // Left Index Trigger

    [Tooltip("The OVRInput Button to monitor (e.g., Button.One for X/A button).")]
    public OVRInput.Button targetButton = OVRInput.Button.One; // X button on Left Touch

    [Tooltip("The OVRInput Button to monitor (e.g., Button.One for X/A button).")]
    public OVRInput.Button secondTargetButton = OVRInput.Button.Two; // X button on Left Touch

    [Tooltip("The controller to check the inputs on (e.g., LTouch for left).")]
    public OVRInput.Controller targetController = OVRInput.Controller.LTouch; // Left Controller

    [Tooltip("How much the trigger needs to be pressed (0 to 1) to be considered 'held'.")]
    [Range(0.01f, 1.0f)]
    public float triggerPressThreshold = 0.5f; // Trigger needs to be at least half-pressed

    [Header("Event Trigger")]
    [Tooltip("Assign functions here that should run when the combo is detected.")]
    public UnityEvent onComboActionTriggered; // The event that will be invoked


    public UnityEvent onSecondComboActionTriggered; // The second event that will be invoked

    public GameObject leftController;
    public GameObject rightController;

    private bool isleftController = false;

    // --- Private Variables ---
    private bool isTriggerHeld = false;

    public GameObject convUI;
    private Vector3 uiPos;

    private void Start()
    {
        uiPos = convUI.transform.localPosition;
    }

    void Update()
    {
        // --- Check Trigger State ---
        // Get the current value of the specified trigger axis
        float triggerValue = OVRInput.Get(targetTrigger, targetController);

        // Determine if the trigger is currently considered held down
        isTriggerHeld = (triggerValue >= triggerPressThreshold);

        // --- Check for Combo Condition ---
        // We only proceed if the trigger is currently held down
        if (isTriggerHeld)
        {
            // Check if the target button was *just pressed down* in this frame
            // OVRInput.GetDown() returns true only for the single frame the button is initially pressed.
            if (OVRInput.GetDown(targetButton, targetController))
            {
                // Both conditions met: Trigger is held AND button was just pressed.
                Debug.Log($"Combo Detected! Invoking assigned actions for {targetTrigger} + {targetButton} on {targetController}.");
                onComboActionTriggered.Invoke();
            }

            if (OVRInput.GetDown(secondTargetButton, targetController))
            {
                // Both conditions met: Trigger is held AND button was just pressed.
                Debug.Log($"Second Combo Detected! Invoking assigned actions for {targetTrigger} + {targetButton} on {targetController}.");
                onSecondComboActionTriggered.Invoke();
            }

        }




        // Check left or right controller trgger
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) >= triggerPressThreshold)
        {
            isleftController = true;
            convUI.transform.parent = leftController.transform;
            Debug.Log("Left Controller Trigger Pressed");
        }
        else if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) >= triggerPressThreshold)
        {
            isleftController = false;
            convUI.transform.parent = rightController.transform;
            Debug.Log("Right Controller Trigger Pressed");
        }

        convUI.transform.localPosition = uiPos;
        convUI.transform.localRotation = Quaternion.identity;

        // Optional: You could add visual/haptic feedback here based on isTriggerHeld state
        if (isTriggerHeld) 
        {
            convUI.SetActive(true); 
        }
        else
        { 
            convUI.SetActive(false); 
        }
    }
}
