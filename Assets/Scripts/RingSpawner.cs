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

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // クリック（またはタップ）
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                 Vector3 spawnPos = hit.point + Vector3.up * spawnHeight;
                // Instantiate(ringPrefab, spawnPos, Quaternion.identity);
                // Before（rotation 指定なし）
                //Instantiate(ringPrefab, spawnPos, Quaternion.identity);

                // After（rotation を指定して寝かせる）
                Quaternion rotation = Quaternion.Euler(90, 0, 0);
                Instantiate(ringPrefab, spawnPos, rotation);

            }
        }
    }
}
