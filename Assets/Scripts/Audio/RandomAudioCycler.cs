using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RandomAudioCycler : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] clips;

    private List<AudioClip> clipPool = new List<AudioClip>();
    private AudioClip lastPlayed;

    public void TriggerAction()
    {
        if (clips.Length == 0)
        {
            Debug.LogWarning("No audio clips assigned!");
            return;
        }

        if (clipPool.Count == 0)
        {
            // Refresh the pool with all clips except the last played
            foreach (AudioClip clip in clips)
            {
                if (clip != lastPlayed) clipPool.Add(clip);
            }
        }

        int index = Random.Range(0, clipPool.Count);
        AudioClip selected = clipPool[index];
        clipPool.RemoveAt(index);

        lastPlayed = selected;
        source.PlayOneShot(selected);
        Debug.Log("Random clip played: " + selected.name);
    }
    void Start()
    {
        StartCoroutine(AmbientLoop());
    }

    IEnumerator AmbientLoop()
    {
        while (true)
        {
            // Wait until the previous clip has finished
            while (source.isPlaying)
            {
                yield return null;
            }

            float wait = Random.Range(5f, 20f); // delay between clips
            yield return new WaitForSeconds(wait);

            TriggerAction(); // plays the next clip
        }
    }


}

