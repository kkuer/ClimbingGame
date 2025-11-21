using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndlessClimbManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root of the moving environment (e.g. ParentObject).")]
    public Transform worldRoot;

    [Tooltip("The water surface object (child of worldRoot).")]
    public Transform waterTransform;

    [Tooltip("The player (camera or body). Player itself does NOT move in Y.")]
    public Transform playerTransform;

    [Header("Water Rising Settings")]
    [Tooltip("Initial rising speed of the water (units per second, in local Y).")]
    public float waterInitialSpeed = 0.5f;

    [Tooltip("Acceleration of the water rise (units per second squared).")]
    public float waterAcceleration = 0.05f;

    [Tooltip("Maximum rising speed of the water (units per second).")]
    public float waterMaxSpeed = 3f;

    [Header("UI - TMP Texts")]
    [Tooltip("Displays the player's climbed height (based on worldRoot movement).")]
    public TMP_Text playerHeightText;

    [Tooltip("Displays how much the water has risen (local to worldRoot).")]
    public TMP_Text waterHeightText;

    [Tooltip("Displays the vertical gap between player and water (world space).")]
    public TMP_Text gapText;

    [Header("Game Over Settings")]
    [Tooltip("How many seconds the water must stay above the player before game over.")]
    public float waterAboveDurationToLose = 5f;

    [Tooltip("Scene name to load when the player loses.")]
    public string endSceneName = "EndScene";

    // Internal
    private float _worldStartY;
    private float _waterStartLocalY;
    private float _currentWaterSpeed;
    private float _waterAboveTimer;

    private void Start()
    {
        // Validate references
        if (worldRoot == null)
        {
            Debug.LogError("[EndlessClimbManager] worldRoot is not assigned. " +
                           "Assign your moving environment root (e.g. ParentObject).");
            enabled = false;
            return;
        }

        if (waterTransform == null)
        {
            Debug.LogError("[EndlessClimbManager] waterTransform is not assigned. " +
                           "Assign your water surface object (child of worldRoot).");
            enabled = false;
            return;
        }

        if (playerTransform == null)
        {
            // Try to auto-find player by tag
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                playerTransform = p.transform;
            }
            else
            {
                Debug.LogError("[EndlessClimbManager] playerTransform is not assigned " +
                               "and no object with tag 'Player' was found.");
                enabled = false;
                return;
            }
        }

        // Record starting positions
        _worldStartY = worldRoot.position.y;
        _waterStartLocalY = waterTransform.localPosition.y;

        _currentWaterSpeed = waterInitialSpeed;
        _waterAboveTimer = 0f;
    }

    private void Update()
    {
        // ===== 1. Update water rising (in local Y under worldRoot) =====
        _currentWaterSpeed += waterAcceleration * Time.deltaTime;
        _currentWaterSpeed = Mathf.Min(_currentWaterSpeed, waterMaxSpeed);

        Vector3 localPos = waterTransform.localPosition;
        localPos.y += _currentWaterSpeed * Time.deltaTime;
        waterTransform.localPosition = localPos;

        // ===== 2. Calculate heights =====
        // Player climbed height:
        // In your setup the PLAYER is static and the WORLD moves.
        // If the world moves DOWN when the player "climbs up",
        // then (startY - currentY) will be positive as you climb.
        float climbedHeight = _worldStartY - worldRoot.position.y;
        if (climbedHeight < 0f) climbedHeight = 0f; // just in case the sign is reversed

        // Water height relative to environment (ignores worldRoot movement)
        float waterHeight = waterTransform.localPosition.y - _waterStartLocalY;

        // Gap in world space (positive if player is above water)
        float gap = playerTransform.position.y - waterTransform.position.y;

        // ===== 3. Update UI texts =====
        if (playerHeightText != null)
            playerHeightText.text = $"Player Height: {climbedHeight:F1} m";

        if (waterHeightText != null)
            waterHeightText.text = $"Water Height: {waterHeight:F1} m";

        if (gapText != null)
            gapText.text = $"Gap: {gap:F1} m";

        // ===== 4. Check if water is above player =====
        if (waterTransform.position.y >= playerTransform.position.y)
        {
            _waterAboveTimer += Time.deltaTime;

            if (_waterAboveTimer >= waterAboveDurationToLose)
            {
                LoadEndScene();
            }
        }
        else
        {
            _waterAboveTimer = 0f;
        }
    }

    private void LoadEndScene()
    {
        if (string.IsNullOrEmpty(endSceneName))
        {
            Debug.LogError("[EndlessClimbManager] endSceneName is empty, cannot load scene.");
            return;
        }

        enabled = false;
        SceneManager.LoadScene(endSceneName);
    }
}
