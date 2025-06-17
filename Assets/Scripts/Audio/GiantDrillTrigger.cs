using System.Collections;
using UnityEngine;

public class GiantDrillTrigger : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clip;

    public float minInterval = 60f;   // Minimum time between actions
    public float maxInterval = 2f * 60f;   // Maximum time between actions

    void Start()
    {
        StartCoroutine(RandomActionRoutine());
        Debug.Log("Timer called!");
    }

    IEnumerator RandomActionRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            Debug.Log("Timer set:" + waitTime / 60);
            yield return new WaitForSeconds(waitTime);
            TriggerAction();
        }
    }

    void TriggerAction()
    {
        
        Debug.Log("Action triggered at: " + Time.time);
        source.PlayOneShot(clip);
   
    }

    
}
