using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        TargetScore target = collision.gameObject.GetComponent<TargetScore>();
        if (target != null)
        {
            Debug.Log("ヒット！得点: " + target.pointValue);
            ScoreManager.Instance.AddScore(target.pointValue);

            // オプション：リングを止める
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
