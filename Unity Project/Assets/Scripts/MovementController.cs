// using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Look At")]
    public AudioSource audioSource;
    public Transform hmd;
    public Transform probe;
    private Transform eyeTarget;
    public float maximumLookAtDistance;
    public float lookAtSpeed;
    [Header("Hand Interaction")]
    // public InteractionObject rightHandTarget;
    // public InteractionObject leftHandTarget;
    public float maximumInteractionDistance = 1f;

    public Transform head;

    // private LookAtIK lookAt;
    private float lookAtWeight;
    private Transform lastTarget = null;
    private float lastWeight = 0;
    private Coroutine lookAtRoutine;
    // private InteractionSystem interactionSystem;
    private Coroutine interactionRoutine;
    private bool isInteracting = false;

    private Coroutine noddingCoroutine;
    private bool isVoiceDetected = false; // Tracks voice detection status

    // Start is called before the first frame update
    void Start()
    {
        // lookAt = GetComponent<LookAtIK>();
        // interactionSystem = GetComponent<InteractionSystem>();
        eyeTarget = hmd;
    }

    // Update is called once per frame
    void Update()
    {
        HandleLookAt();
    }

    private void HandleLookAt()
    {
        // if (lookAt == null) return;

        Transform target = audioSource?.isPlaying == true ? hmd : eyeTarget;
        float weight = ShouldLookAtTarget(target) ? 1 : 0;

        LookAtTarget(target, weight);
    }

    private bool ShouldLookAtTarget(Transform target)
    {
        return true;//target != null && Vector3.Distance(head.position, target.position) <= maximumLookAtDistance;
    }

    private IEnumerator DelayedNod(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the delay (1 second)
        LookAtTarget(hmd, 1);  // Start nodding after the delay
        yield return new WaitForSeconds(delay);

    }
    void OnGUI() 
    {
        // if (interactionSystem == null) return;

        GUILayout.BeginArea(new Rect(0, 0, 120, 60));

        if (GUILayout.Button("Start Interaction")) {
            StartInteraction();
        }

        if (GUILayout.Button("Stop Interaction")) {
            StopInteraction();
        }

        GUILayout.EndArea();
    }

    [ContextMenu("Start Interaction")]
    public void PointTrajectory()
    {
        // interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, rightHandTarget, true);
    }

    public void StopPointing()
    {
        // interactionSystem.StopInteraction(FullBodyBipedEffector.RightHand);
    }

    public void StartInteraction()
    {
        if (interactionRoutine == null)
        {
            interactionRoutine = StartCoroutine(InteractionRoutine());
            Debug.Log("Interaction started");
        }
    }

    public void StopInteraction()
    {
        if (interactionRoutine != null)
        {
            StopCoroutine(interactionRoutine);
            interactionRoutine = null;
            isInteracting = false;

            // if (rightHandTarget != null)
            // {
            //     interactionSystem.StopInteraction(FullBodyBipedEffector.RightHand);
            // }

            // if (leftHandTarget != null)
            // {
            //     interactionSystem.StopInteraction(FullBodyBipedEffector.LeftHand);
            // }

            Debug.Log("Interaction stopped");
        }
    }

    public void LookAtUser()
    {
        LookAtTarget(hmd, 1);
    }

    public void LookAtObject(bool isTargetObject)
    {
        eyeTarget = isTargetObject ? probe : hmd;
    }

    private void LookAtTarget(Transform target, float targetWeight)
    {
        if ((target == lastTarget) && (lastWeight == targetWeight)) return;

        lastTarget = target;
        lastWeight = targetWeight;

        if (lookAtRoutine != null) {
            StopCoroutine(lookAtRoutine);
            lookAtRoutine = null;
        }
        
        lookAtRoutine = StartCoroutine(LookAtRoutine(target, targetWeight));
    }

    private IEnumerator LookAtRoutine(Transform target, float targetWeight)
    {
        while (true) {
            // lookAt.solver.IKPosition = Vector3.Lerp(lookAt.solver.IKPosition, target.position, Time.deltaTime * lookAtSpeed);
            // lookAt.solver.IKPositionWeight = Mathf.Lerp(lookAt.solver.IKPositionWeight, targetWeight, Time.deltaTime * lookAtSpeed);
            yield return null;
        }
    }

    private IEnumerator InteractionRoutine()
    {
        Vector3 doctorPos = head.position;

        while(true) 
        {
            //if (rightHandTarget != null) {
            //    Vector3 targetPos = rightHandTarget.transform.position;
            //    float distance = Vector3.Distance(doctorPos, targetPos);

            //    if ((distance <= maximumInteractionDistance) && !isInteracting) {
            //        interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, rightHandTarget, true);
            //        isInteracting = true;
            //    }
            //    else if (distance > maximumInteractionDistance) {
            //        interactionSystem.StopInteraction(FullBodyBipedEffector.RightHand);
            //        isInteracting = false;
            //    }
            //}

            // if (leftHandTarget != null) {
            //     Vector3 targetPos = leftHandTarget.transform.position;
            //     float distance = Vector3.Distance(doctorPos, targetPos);

            //     if ((distance <= maximumInteractionDistance) && !isInteracting) {
            //         interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, leftHandTarget, true);
            //         isInteracting = true;
            //     }
            //     else if (distance > maximumInteractionDistance) {
            //         interactionSystem.StopInteraction(FullBodyBipedEffector.LeftHand);
            //         isInteracting = false;
            //     }
            // }

            yield return null;
        }
    }
}
