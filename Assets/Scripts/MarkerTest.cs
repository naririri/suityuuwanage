using UnityEngine;

public class MarkerTest : MonoBehaviour
{
    public Camera topDownCamera;
    public GameObject ringMarker;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ringMarker.transform.position += Vector3.right * 1f;
            Debug.Log("Space move: " + ringMarker.transform.position);
        }
        
        Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ringMarker.SetActive(true);
            ringMarker.transform.position = hit.point + Vector3.up * 0.1f;
        }
    }
}
