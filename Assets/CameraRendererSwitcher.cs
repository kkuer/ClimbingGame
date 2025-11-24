using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraRendererSwitcher : MonoBehaviour
{
    public static CameraRendererSwitcher Instance { get; private set; }

    public Camera targetCamera;

    private UniversalAdditionalCameraData camData;
    private Coroutine routine;

    private void Awake()
    {
        Instance = this;

        if (targetCamera == null)
            targetCamera = Camera.main;

        camData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
        if (camData == null)
            camData = targetCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
    }

    public void SwitchTo1ForSeconds(float duration)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(SwitchRoutine(duration));
    }

    private IEnumerator SwitchRoutine(float duration)
    {
        // 默认是 0
        camData.SetRenderer(1);

        yield return new WaitForSeconds(duration);

        // 切回默认 renderer 0
        camData.SetRenderer(0);

        routine = null;
    }
}
