using UnityEngine;
using TMPro;

public class EndSceneManager : MonoBehaviour
{
    [Tooltip("TMP text that displays the highest height reached.")]
    public TMP_Text highestHeightText;

    private void Start()
    {
        if (highestHeightText == null)
        {
            Debug.LogError("[EndSceneManager] highestHeightText is not assigned.");
            return;
        }

        // Read from PlayerPrefs
        float highest = PlayerPrefs.GetFloat("HighestClimb", 0f);

        highestHeightText.text = $"Height Reached:\n{highest:F1} m";
    }
}
