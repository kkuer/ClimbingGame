using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Grab State")]
    public bool isGrabbingLeft = false;
    public bool isGrabbingRight = false;

    [Header("Stamina")]
    public float staminaLeft = 0;
    public float staminaRight = 0;
    public float maxStamina = 100f;

    [SerializeField] private float staminaRegenRate = 20f;
    [SerializeField] private float staminaLossRate = 15f;

    [Header("Stamina UI")]
    public Slider leftStam;
    public Slider rightStam;

    public Image leftStamColor;
    public Image rightStamColor;

    [Header("Player Collider")]
    public CapsuleCollider col;

    [Header("Hands")]
    public GameObject leftHand;
    public GameObject rightHand;

    [Header("Hand Materials")]
    public Renderer leftHandRenderer;
    public Renderer rightHandRenderer;

    public Color leftHandFatigueColor = Color.gray;
    public Color rightHandFatigueColor = Color.gray;

    private Material _leftHandMaterial;
    private Material _rightHandMaterial;

    private Color _leftHandInitialColor;
    private Color _rightHandInitialColor;

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

    private void Start()
    {
        staminaLeft = maxStamina;
        staminaRight = maxStamina;

        isGrabbingLeft = false;
        isGrabbingRight = false;

        if (col == null)
        {
            col = GetComponent<CapsuleCollider>();
        }

        // Try to auto-assign renderers if not set
        if (leftHandRenderer == null && leftHand != null)
        {
            leftHandRenderer = leftHand.GetComponentInChildren<Renderer>();
        }

        if (rightHandRenderer == null && rightHand != null)
        {
            rightHandRenderer = rightHand.GetComponentInChildren<Renderer>();
        }

        // Cache materials and initial colors
        if (leftHandRenderer != null)
        {
            _leftHandMaterial = leftHandRenderer.material;
            _leftHandInitialColor = _leftHandMaterial.color;
        }

        if (rightHandRenderer != null)
        {
            _rightHandMaterial = rightHandRenderer.material;
            _rightHandInitialColor = _rightHandMaterial.color;
        }

        // Optional: initialize slider ranges
        if (leftStam != null)
        {
            leftStam.minValue = 0f;
            leftStam.maxValue = maxStamina;
            leftStam.value = staminaLeft;
        }

        if (rightStam != null)
        {
            rightStam.minValue = 0f;
            rightStam.maxValue = maxStamina;
            rightStam.value = staminaRight;
        }
    }

    private void Update()
    {
        HandleStamina();

        if (leftStam != null)
            leftStam.value = staminaLeft;

        if (rightStam != null)
            rightStam.value = staminaRight;

        // UI bar color (red when low)
        if (leftStamColor != null)
        {
            if (staminaLeft <= 33f)
                leftStamColor.color = Color.red;
            else
                leftStamColor.color = Color.white;
        }

        if (rightStamColor != null)
        {
            if (staminaRight <= 33f)
                rightStamColor.color = Color.red;
            else
                rightStamColor.color = Color.white;
        }

        UpdateHandColors();
    }

    private void HandleStamina()
    {
        // ===== Left hand =====
        if (isGrabbingLeft)
        {
            if (staminaLeft > 0f)
            {
                // GameManager.Instance.moveWorld(leftHand);
                staminaLeft -= Time.deltaTime * staminaLossRate;
                staminaLeft = Mathf.Max(staminaLeft, 0f);
            }

            if (staminaLeft <= 0f)
            {
                // leftHand.ForceReleaseFromStamina();
                GameManager.Instance.grabbingRight = false;//
            }
        }
        else
        {
            if (staminaLeft < maxStamina)
            {
                staminaLeft += Time.deltaTime * staminaRegenRate;
                staminaLeft = Mathf.Min(staminaLeft, maxStamina);
                GameManager.Instance.grabbingLeft = false;
            }
        }

        // ===== Right hand =====
        if (isGrabbingRight)
        {
            if (staminaRight > 0f)
            {
                // GameManager.Instance.moveWorld(rightHand);
                staminaRight -= Time.deltaTime * staminaLossRate;
                staminaRight = Mathf.Max(staminaRight, 0f);//
            }

            if (staminaRight <= 0f)
            {
                // rightHand.ForceReleaseFromStamina();
                GameManager.Instance.grabbingRight = false;
            }
        }
        else
        {
            if (staminaRight < maxStamina)
            {
                staminaRight += Time.deltaTime * staminaRegenRate;
                staminaRight = Mathf.Min(staminaRight, maxStamina);
                GameManager.Instance.grabbingRight = false;
            }
        }
    }

    private void UpdateHandColors()
    {
        // Normalize stamina to 0¨C1, then Lerp between fatigue and initial color
        if (_leftHandMaterial != null)
        {
            float tLeft = Mathf.InverseLerp(0f, maxStamina, staminaLeft); // 0 at 0, 1 at max
            _leftHandMaterial.color = Color.Lerp(leftHandFatigueColor, _leftHandInitialColor, tLeft);
        }

        if (_rightHandMaterial != null)
        {
            float tRight = Mathf.InverseLerp(0f, maxStamina, staminaRight);
            _rightHandMaterial.color = Color.Lerp(rightHandFatigueColor, _rightHandInitialColor, tRight);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Wall wall = other.GetComponent<Wall>();
        if (wall != null && wall.gameObject == GameManager.Instance.currentWall && wall.old == false)
        {
            GameManager.Instance.SpawnNewWall();
        }
    }
}
