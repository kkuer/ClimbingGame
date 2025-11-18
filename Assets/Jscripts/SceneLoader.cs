using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string targetSceneName = "SampleScene";

    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("SceneLoader: targetSceneName is empty.");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}
