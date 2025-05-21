using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

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
    }

    public void PlayEffect(GameObject effectPrefab, Vector3 position, float lifetime = 2f)
    {
        if (effectPrefab != null)
        {
            GameObject effectObj = Instantiate(effectPrefab, position, Quaternion.identity);

            // ParticleSystem を明示的に再生（PlayOnAwakeオフ対策）
            ParticleSystem ps = effectObj.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }

            StartCoroutine(DestroyEffectAfterDelay(effectObj, lifetime));
        }
    }

    private IEnumerator DestroyEffectAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }
}
