using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSound : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Water"))
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.splashSound, transform.position);
        }
    }
}
