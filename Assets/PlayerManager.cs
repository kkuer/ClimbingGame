using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.UI;
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    
    public bool isGrabbingLeft = false;
    public bool isGrabbingRight = false;
    
    public float staminaLeft = 0;
    public float staminaRight = 0;
    public float maxStamina = 100f;

    [SerializeField] private float staminaRegenRate = 20f; 
    [SerializeField] private float staminaLossRate = 15f;

    public Slider leftStam;
    public Slider rightStam;

    public Image leftStamColor;
    public Image rightStamColor;

    public CapsuleCollider col;

    public GameObject leftHand;
    public GameObject rightHand;

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
    }

    private void Update()
    {
        HandleStamina();
        leftStam.value = staminaLeft;
        rightStam.value = staminaRight;

        if (staminaLeft <= 33)
        {
            leftStamColor.color = Color.red;
        }
        else
        {
            leftStamColor.color = Color.white;
        }

        if (staminaRight <= 33)
        {
            rightStamColor.color = Color.red;
        }
        else
        {
            rightStamColor.color = Color.white;
        }
    }

    private void HandleStamina()
    {
        
        if (isGrabbingLeft)
        {
            if (staminaLeft > 0f)
            {
                //GameManager.Instance.moveWorld(leftHand);
                staminaLeft -= Time.deltaTime * staminaLossRate;
                staminaLeft = Mathf.Max(staminaLeft, 0f);
            }

            
            if (staminaLeft <= 0f)
            {
                //leftHand.ForceReleaseFromStamina();
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

        
        if (isGrabbingRight)
        {
            if (staminaRight > 0f)
            {
                //GameManager.Instance.moveWorld(rightHand);
                staminaRight -= Time.deltaTime * staminaLossRate;
                staminaRight = Mathf.Max(staminaRight, 0f);
            }

            if (staminaRight <= 0f)
            {
                //rightHand.ForceReleaseFromStamina();
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

    private void OnTriggerEnter(Collider other)
    {
        Wall wall = other.GetComponent<Wall>();
        if (wall != null && wall.gameObject == GameManager.Instance.currentWall && wall.old == false)
        {
            GameManager.Instance.SpawnNewWall();
        }
    }
}
