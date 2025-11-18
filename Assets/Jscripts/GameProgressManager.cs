using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameProgressManager : MonoBehaviour
{
    [Header("Target object to track (e.g., player or world root)")]
    public Transform target;

    [Header("Progress slider (0 = start, 1 = reached -15y)")]
    public Slider progressSlider;

    [Header("Settings")]
    public float targetDistance = 15f;
    public string endSceneName = "EndScene";

    private float _initialY;

    private void Start()
    {
        // Auto-locate player if target is not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("[GameProgressManager] No target assigned, and no object with tag 'Player' found.");
                enabled = false;
                return;
            }
        }

        // Record initial Y position
        _initialY = target.position.y;

        // Initialize slider
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }
        else
        {
            Debug.LogWarning("[GameProgressManager] progressSlider is not assigned.");
        }
    }

    private void Update()
    {
        if (target == null || progressSlider == null) return;

        float currentY = target.position.y;

        // Distance moved downward since start
        float movedDistance = _initialY - currentY;

        // Normalize to 0~1
        float t = movedDistance / targetDistance;
        t = Mathf.Clamp01(t);

        progressSlider.value = t;

        // Load End Scene once progress reaches 1
        if (t >= 1f)
        {
            enabled = false;

            if (!string.IsNullOrEmpty(endSceneName))
            {
                SceneManager.LoadScene(endSceneName);
            }
            else
            {
                Debug.LogError("[GameProgressManager] endSceneName is empty, cannot load scene.");
            }
        }
    }
}
