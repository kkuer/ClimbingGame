using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap.PhysicalHands;

public class WobbleZRotation : MonoBehaviour
{
    [Header("Wobble Settings")]
    public float maxAngle = 10f;
    public float wobbleSpeed = 2f;
    public Vector2 intervalRange = new Vector2(2f, 5f);
    public float wobbleDuration = 1.5f;

    [Header("Break Settings")]
    public PhysicalHandEvents handEvents;
    public float breakDuration = 3f;

    [Header("Drop Motion")]
    [Tooltip("Drop speed along -localZ for BOTH parent and child objects.")]
    public float dropSpeed = 1.0f;

    [Header("Force Release Settings")]
    public bool forceReleaseLeftOnGrab = true;
    public bool forceReleaseRightOnGrab = true;
    public float forceReleaseDelayLeft = 0f;
    public float forceReleaseDelayRight = 0f;

    private Quaternion initialRotation;

    // Record: parent's (Level-2 object) original transform
    private Transform initialParent;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private bool isWobbling = false;
    private bool isBreaking = false;
    private bool shouldBreakAfterWobble = false;

    private bool leftReleaseScheduled = false;
    private bool rightReleaseScheduled = false;

    private bool leftWasGrabbing = false;
    private bool rightWasGrabbing = false;

    private Collider[] ownColliders;
    private Renderer[] ownRenderers;

    private struct ChildBodyInfo
    {
        public Rigidbody rb;
        public Transform originalParent;
        public Vector3 originalLocalPosition;
        public Quaternion originalLocalRotation;
        public Vector3 originalLocalScale;
        public bool originalIsKinematic;
        public bool originalUseGravity;
    }

    private List<ChildBodyInfo> childBodies = new List<ChildBodyInfo>();

    private void Start()
    {
        initialRotation = transform.localRotation;
        initialParent = transform.parent;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        if (handEvents == null)
            handEvents = GetComponent<PhysicalHandEvents>();

        ownColliders = GetComponents<Collider>();
        ownRenderers = GetComponents<MeshRenderer>();

        Rigidbody[] allBodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in allBodies)
        {
            if (rb.gameObject == this.gameObject) continue;

            childBodies.Add(new ChildBodyInfo
            {
                rb = rb,
                originalParent = rb.transform.parent,
                originalLocalPosition = rb.transform.localPosition,
                originalLocalRotation = rb.transform.localRotation,
                originalLocalScale = rb.transform.localScale,
                originalIsKinematic = rb.isKinematic,
                originalUseGravity = rb.useGravity
            });
        }

        StartCoroutine(WobbleRoutine());
    }

    IEnumerator WobbleRoutine()
    {
        while (true)
        {
            float wait = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(wait);

            yield return StartCoroutine(WobbleOnce());

            if (shouldBreakAfterWobble && !isBreaking)
            {
                yield return StartCoroutine(BreakApartRoutine());
                shouldBreakAfterWobble = false;
            }
        }
    }

    IEnumerator WobbleOnce()
    {
        isWobbling = true;
        leftReleaseScheduled = false;
        rightReleaseScheduled = false;

        leftWasGrabbing = handEvents != null && handEvents.leftHandGrabbing;
        rightWasGrabbing = handEvents != null && handEvents.rightHandGrabbing;

        float t = 0f;

        while (t < wobbleDuration)
        {
            t += Time.deltaTime;

            float angle = Mathf.Sin(t * wobbleSpeed) * maxAngle;
            transform.localRotation = initialRotation * Quaternion.Euler(0, 0, angle);

            if (handEvents != null)
            {
                bool leftNow = handEvents.leftHandGrabbing;
                bool rightNow = handEvents.rightHandGrabbing;

                // Grab start ¡ú schedule force release
                if (!leftWasGrabbing && leftNow && forceReleaseLeftOnGrab && !leftReleaseScheduled)
                {
                    leftReleaseScheduled = true;
                    StartCoroutine(ForceReleaseHandRoutine(true, forceReleaseDelayLeft));
                }

                if (!rightWasGrabbing && rightNow && forceReleaseRightOnGrab && !rightReleaseScheduled)
                {
                    rightReleaseScheduled = true;
                    StartCoroutine(ForceReleaseHandRoutine(false, forceReleaseDelayRight));
                }

                // Detect *release during wobble* ¡ú mark break
                bool leftReleased = leftWasGrabbing && !leftNow;
                bool rightReleased = rightWasGrabbing && !rightNow;

                if (leftReleased || rightReleased)
                {
                    shouldBreakAfterWobble = true;
                }

                leftWasGrabbing = leftNow;
                rightWasGrabbing = rightNow;
            }

            yield return null;
        }

        transform.localRotation = initialRotation;
        isWobbling = false;
    }

    IEnumerator ForceReleaseHandRoutine(bool isLeft, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (GameManager.Instance != null)
        {
            if (isLeft) GameManager.Instance.grabbingLeft = false;
            else GameManager.Instance.grabbingRight = false;
        }
    }

    IEnumerator BreakApartRoutine()
    {
        if (isBreaking) yield break;
        isBreaking = true;

        // Disable own colliders & renderers (Level-2 object)
        foreach (var col in ownColliders) if (col != null) col.enabled = false;
        foreach (var ren in ownRenderers) if (ren != null) ren.enabled = false;

        float elapsed = 0f;

        while (elapsed < breakDuration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            // ---- Parent also drops along -local Z ----
            Vector3 parentPos = transform.localPosition;
            parentPos.z -= dropSpeed * dt;
            transform.localPosition = parentPos;

            // ---- Children drop along -local Z ----
            foreach (var info in childBodies)
            {
                if (info.rb == null) continue;

                Transform t = info.rb.transform;
                Vector3 lp = t.localPosition;
                lp.z -= dropSpeed * dt;
                t.localPosition = lp;
            }

            yield return null;
        }

        // ---- Restore parent transform ----
        if (initialParent != null)
            transform.SetParent(initialParent, false);

        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;

        // Restore own colliders & renderers
        foreach (var col in ownColliders) if (col != null) col.enabled = true;
        foreach (var ren in ownRenderers) if (ren != null) ren.enabled = true;

        // ---- Restore children ----
        foreach (var info in childBodies)
        {
            if (info.rb == null) continue;

            Transform t = info.rb.transform;

            Transform targetParent = info.originalParent != null ? info.originalParent : transform;
            t.SetParent(targetParent, false);

            t.localPosition = Vector3.zero;
            t.localRotation = info.originalLocalRotation;
            t.localScale = info.originalLocalScale;

            info.rb.isKinematic = info.originalIsKinematic;
            info.rb.useGravity = info.originalUseGravity;
        }

        isBreaking = false;
    }
}
