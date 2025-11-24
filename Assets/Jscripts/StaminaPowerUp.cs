using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StaminaPowerUp : MonoBehaviour
{
    [Header("Power Up Settings")]
    
    public float restoreAmount = 40f;

    [Header("Camera Renderer Settings")]
    public bool changeRendererOnPickup = true;
    public int specialRendererIndex = 1; 
    public float rendererDuration = 3f;   

    public void OnGrab(bool left)
    {
        if (left)
        {
            if (PlayerManager.Instance.isGrabbingLeft)
            {
                ApplyStamina();
                Destroy(gameObject);
                
            }
        }
        else
        {

            if (PlayerManager.Instance.isGrabbingRight)
            {

                ApplyStamina();
                Destroy(gameObject);
                
            }
        }
    }

    private void ApplyStamina()
    {
        var pm = PlayerManager.Instance;

        pm.staminaLeft = Mathf.Min(pm.staminaLeft + restoreAmount, pm.maxStamina);
        pm.staminaRight = Mathf.Min(pm.staminaRight + restoreAmount, pm.maxStamina);

        Debug.Log("Stamina Powerup used. Both hands restored.");


        if (CameraRendererSwitcher.Instance != null)
        {
            CameraRendererSwitcher.Instance.SwitchTo1ForSeconds(3f);
        }
    }
}
