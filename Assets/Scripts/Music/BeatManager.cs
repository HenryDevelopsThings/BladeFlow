using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    [SerializeField] private float bpm;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Intervals[] intervals;

    void Update()
    {
        foreach (Intervals interval in intervals) {
            float sampledTime = (audioSource.timeSamples / (audioSource.clip.frequency * interval.GetIntervalLength(bpm)));
            interval.CheckForNewInterval(sampledTime);
        }
    }

    public int GetCurrentInterval()
    {
        int currentInterval = -1;

        foreach (Intervals interval in intervals)
        {
            if (interval.lastInterval > currentInterval)
            {
                currentInterval = interval.lastInterval;
            }
        }

        return currentInterval;
    }
}

[System.Serializable]
public class Intervals {
    [SerializeField] private float steps;
    [SerializeField] public UnityEvent trigger;
    [HideInInspector] public int lastInterval;
    
    public float GetIntervalLength(float bpm) {
        return 60f / (bpm * steps);
    }

    public void CheckForNewInterval(float interval) {
        if (Mathf.FloorToInt(interval) != lastInterval) {
            lastInterval = Mathf.FloorToInt(interval);
            trigger.Invoke();
        }
    }
}
