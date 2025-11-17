using Leap;
using UnityEngine;

public class GrabTest : MonoBehaviour
{
    public GrabDetector grabDetector;
    public bool left;
    public bool right;

    private void Update()
    {
        if (grabDetector.IsGrabbing)
        {
            if (left)
            {
                PlayerManager.Instance.isGrabbingLeft = false;
            }
            if (right)
            {
                PlayerManager.Instance.isGrabbingRight = false;
            }
        }
        else
        {
            if (left)
            {
                PlayerManager.Instance.isGrabbingLeft = true;
            }
            if (right)
            {
                PlayerManager.Instance.isGrabbingRight = true;
            }
        }
    }
}
