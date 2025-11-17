using UnityEngine;

public class Wall : MonoBehaviour
{
    public Transform nextPos;
    public Transform prevPos;

    public BoxCollider col;

    public bool old;

    private void Start()
    {
        old = false;
    }
}
