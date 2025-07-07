using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Sit")]
    public void SitDown()
    {
        StartCoroutine(SitDownWithDelay(6f)); // Delay of 2 seconds
    }

    private IEnumerator SitDownWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        animator.SetTrigger("sit"); // Trigger the "sit" animation
    }

    [ContextMenu("Stand")]
    public void StandUp()
    {
        animator.SetTrigger("stand");
    }
    
}
