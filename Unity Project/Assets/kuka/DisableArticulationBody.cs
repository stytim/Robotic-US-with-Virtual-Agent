using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableArticulationBody : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Disable Articulation Bodies")]
    private void OnEnable()
    {
        ArticulationBody[] articulationBodies = GetComponentsInChildren<ArticulationBody>();
        foreach (ArticulationBody articulationBody in articulationBodies)
        {
            articulationBody.enabled = false;
        }
    }


    [ContextMenu("Enable Articulation Bodies")]
    private void OnDisable()
    {
        ArticulationBody[] articulationBodies = GetComponentsInChildren<ArticulationBody>();
        foreach (ArticulationBody articulationBody in articulationBodies)
        {
            articulationBody.enabled = true;
        }
    }

    
}
