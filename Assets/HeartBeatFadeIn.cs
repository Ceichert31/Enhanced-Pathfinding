using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBeatFadeIn : MonoBehaviour
{
    public AudioSource heartBeatAudioSource;

    public float heartBeatFadeInMultiplier = 0.5f;
    private void Update()
    {
        if (heartBeatAudioSource.volume > 0.6) return;

        heartBeatAudioSource.volume += Time.deltaTime * heartBeatFadeInMultiplier;
    }
}
