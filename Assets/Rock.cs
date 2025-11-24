using Leap.PhysicalHands;
using UnityEngine;
using UnityEngine.UIElements;

public class Rock : MonoBehaviour
{
    public void moveWorld(bool left)
    {
        if (GameManager.Instance.gameStarted == false)
        {
            GameManager.Instance.gameStarted = true;
        }

        Vector3 weightedPos;

        if (left)
        {
            Debug.Log("left");
            if (PlayerManager.Instance.isGrabbingLeft)
            {
                GameManager.Instance.grabbingLeft = true;
                weightedPos = new Vector3(PlayerManager.Instance.leftHand.transform.position.x - gameObject.transform.position.x, 
                    PlayerManager.Instance.leftHand.transform.position.y - gameObject.transform.position.y, 0);

                ParentObject.Instance.gameObject.transform.position += weightedPos * (PlayerManager.Instance.staminaLeft / PlayerManager.Instance.maxStamina);
                //ParentObject.Instance.LastLHPosition = PlayerManager.Instance.leftHand.transform.position;

            }
        }
        else
        {
            GameManager.Instance.grabbingRight = true;
            if (PlayerManager.Instance.isGrabbingRight)
            {
                weightedPos = new Vector3(PlayerManager.Instance.rightHand.transform.position.x - gameObject.transform.position.x, 
                    PlayerManager.Instance.rightHand.transform.position.y - gameObject.transform.position.y, 0);

                ParentObject.Instance.gameObject.transform.position += weightedPos * (PlayerManager.Instance.staminaRight / PlayerManager.Instance.maxStamina);
                //ParentObject.Instance.LastRHPosition = PlayerManager.Instance.rightHand.transform.position;

                Debug.Log("moving world with RIGHT hand");
            }
        }
    }
}
