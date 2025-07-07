using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchUSProbePositions : MonoBehaviour
{
    public Transform robotUSProbe;
    public Transform doctorUSProbe;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((robotUSProbe != null) && (doctorUSProbe != null)) {
            doctorUSProbe.position = robotUSProbe.position;
            doctorUSProbe.rotation = robotUSProbe.rotation;
        }
    }
}
