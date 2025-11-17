using UnityEngine;

public class ParentObject : MonoBehaviour
{
    public static ParentObject Instance { get; private set; }

    public Vector3 LastLHPosition;
    public Vector3 LastRHPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        LastLHPosition = PlayerManager.Instance.leftHand.transform.position;
        LastRHPosition = PlayerManager.Instance.rightHand.transform.position;
    }

    private void Update()
    {
        
    }
}
