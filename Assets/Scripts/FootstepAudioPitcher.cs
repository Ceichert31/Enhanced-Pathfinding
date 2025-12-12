using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Taken from another horror game of mine (It's called Below Deck on Itch, you should check it out)
/// </summary>
public class FootstepAudioPitcher : MonoBehaviour
{
    [Header("Scriptable Object Reference")]
    /* [SerializeField] private AudioPitcherSO audioPitcherSO;
     [SerializeField] private SoundEventChannel soundEventChannel;*/

    [SerializeField]
    private List<AudioClip> clips;

    public float volumeMin = 0.6f;
    public float volumeMax = 1f;
    public float pitchMin = 0.9f;
    public float pitchMax = 1.4f;

    [Header("Footstep Settings")]
    [Tooltip("How long it takes to play when you start walking")]
    [SerializeField] private float playSpeed = 1f;

    [Tooltip("Pause between footsteps")]
    [SerializeField] private float audioInterval = 0.3f;

    private InputController playerController;
    public AudioSource audioSource;

    private float currentTime;

    float audioAcc = 0;

    void Start()
    {
        playerController = GetComponent<InputController>();

        //audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (playerController.IsSprinting)
        {
            playSpeed = 2;
        }
        else
        {
            playSpeed = 1;
        }

        if (!playerController.IsGrounded || !playerController.IsMoving)
        {
            currentTime = 0f;
          /*  if (audioAcc > 0)
            {
                audioAcc -=  * Time.deltaTime;
                soundEventChannel.CurrentSoundLevel -= audioPitcherSO.audioLevel * Time.deltaTime;
            }*/
        }
        else if (playerController.IsMoving)
        {
            currentTime += playSpeed / 10 * Time.deltaTime;
            //audioAcc += audioPitcherSO.audioLevel * Time.deltaTime;
            //soundEventChannel.CurrentSoundLevel += audioPitcherSO.audioLevel * Time.deltaTime;
        }

        if (currentTime > audioInterval)
        {
            currentTime = 0.0f;

            //audioPitcherSO.Play(audioSource);

            AudioClip clip = clips[Random.Range(0, clips.Count)];
            float volume = Random.Range(volumeMin, volumeMax);
            float pitch = Random.Range(pitchMin, pitchMax);
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }
    }
}