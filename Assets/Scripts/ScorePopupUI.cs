using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorePopupUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform popupContainer;     // Canvas内の任意のRectTransform（親）
    public TextMeshProUGUI popupPrefab;      // 「+10」などのTMP(U)Iプレハブ

    [Header("Anim")]
    public Vector2 moveOffset = new Vector2(0f, 80f);
    public float duration = 0.8f;

    void OnEnable()
    {
        ScoreManager.OnScoreAddedAt += HandleScoreAddedAt;
    }
    void OnDisable()
    {
        ScoreManager.OnScoreAddedAt -= HandleScoreAddedAt;
    }

    void HandleScoreAddedAt(int amount, Vector3 worldPos)
    {
        // ワールド座標 -> スクリーン座標
        Vector3 screen = Camera.main != null
            ? Camera.main.WorldToScreenPoint(worldPos)
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        // スクリーン -> Canvasローカル座標
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                popupContainer, screen, null, out Vector2 local))
        {
            StartCoroutine(SpawnPopup("+" + amount, local));
        }
    }

    IEnumerator SpawnPopup(string text, Vector2 localPos)
    {
        if (popupPrefab == null || popupContainer == null) yield break;

        var popup = Instantiate(popupPrefab, popupContainer);
        var rt = popup.rectTransform;

        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = localPos;

        popup.text = text;
        popup.alpha = 1f;
        popup.transform.localScale = Vector3.one;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            rt.anchoredPosition = Vector2.Lerp(localPos, localPos + moveOffset, k);
            popup.alpha = Mathf.Lerp(1f, 0f, k);
            yield return null;
        }
        Destroy(popup.gameObject);
    }
}
