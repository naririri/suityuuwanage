using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSpawner : MonoBehaviour
{
    void Start()
{
    GameObject[] rings = GameObject.FindGameObjectsWithTag("Ring");
    foreach (GameObject ring in rings)
    {
        Destroy(ring);
    }
}

    public GameObject ringPrefab; // リングのプレハブ
    public float spawnHeight = 10f; // 水面からの高さ
    public Camera dropCamera; // Inspectorで設定
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // クリック（またはタップ）
        {
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Ray ray = dropCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 spawnPos = hit.point + Vector3.up * spawnHeight;
                // Instantiate(ringPrefab, spawnPos, Quaternion.identity);
                // Before（rotation 指定なし）
                //Instantiate(ringPrefab, spawnPos, Quaternion.identity);

                // After（rotation を指定して寝かせる）
                // Quaternion rotation = Quaternion.Euler(90, 0, 0);
                // Instantiate(ringPrefab, spawnPos, rotation);
                Quaternion rotation = Quaternion.Euler(0, 0, 0); // Unity側で真っ直ぐにしたい方向に調整
                Instantiate(ringPrefab, spawnPos, rotation);

                // 🎧 着水音を再生（SoundManagerを通して一元管理）
                if (SoundManager.Instance != null && SoundManager.Instance.splashSound != null)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.splashSound, spawnPos);
                }
            }
        }
    }
}
