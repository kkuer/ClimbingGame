using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WaterLevelUI : MonoBehaviour
{
    [Header("Water panel (RectTransform, suggested bottom-anchor)")]
    public RectTransform waterPanel;

    [Header("Slider handle RectTransform for overlap detection")]
    public RectTransform sliderHandle;

    [Header("Water rising speed (UI pixels / second)")]
    public float riseSpeed = 50f;

    [Header("Required overlap time (seconds)")]
    public float requiredOverlapTime = 10f;

    [Header("GameOver scene name")]
    public string gameOverSceneName = "GameOver";

    private float _overlapTimer = 0f;
    private Vector3[] _cornersA = new Vector3[4];
    private Vector3[] _cornersB = new Vector3[4];

    private void Reset()
    {
        // Try auto-locating RectTransform (optional)
        if (waterPanel == null)
            waterPanel = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (waterPanel == null || sliderHandle == null) return;

        // 1. Increase panel height upward
        Vector2 size = waterPanel.sizeDelta;
        size.y += riseSpeed * Time.deltaTime;
        waterPanel.sizeDelta = size;

        // 2. Detect overlap between water and slider handle
        bool overlapping = IsOverlapping(waterPanel, sliderHandle);

        if (overlapping)
        {
            _overlapTimer += Time.deltaTime;

            if (_overlapTimer >= requiredOverlapTime)
            {
                enabled = false;

                if (!string.IsNullOrEmpty(gameOverSceneName))
                {
                    SceneManager.LoadScene(gameOverSceneName);
                }
                else
                {
                    Debug.LogError("[WaterLevelUI] gameOverSceneName is empty, cannot load scene.");
                }
            }
        }
        else
        {
            _overlapTimer = 0f; // reset timer when no longer overlapping
        }
    }

    /// Check if two UI RectTransforms overlap on screen.
    private bool IsOverlapping(RectTransform a, RectTransform b)
    {
        a.GetWorldCorners(_cornersA);
        b.GetWorldCorners(_cornersB);

        Vector3 aMin = _cornersA[0];
        Vector3 aMax = _cornersA[2];
        Rect rectA = new Rect(aMin, aMax - aMin);

        Vector3 bMin = _cornersB[0];
        Vector3 bMax = _cornersB[2];
        Rect rectB = new Rect(bMin, bMax - bMin);

        return rectA.Overlaps(rectB);
    }
}
