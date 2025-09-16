using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioClip splashSound;
    public AudioClip criticalSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // このオブジェクトがシーンをまたいでも消えないように
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}
