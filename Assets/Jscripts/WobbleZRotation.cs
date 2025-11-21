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

    [Header("Force Release Settings")]
    [Tooltip("Whether grabbing with the left hand should trigger a force release.")]
    public bool forceReleaseLeftOnGrab = true;

    [Tooltip("Whether grabbing with the right hand should trigger a force release.")]
    public bool forceReleaseRightOnGrab = true;

    [Tooltip("Delay (in seconds) before forcing left hand release after it grabs.")]
    public float forceReleaseDelayLeft = 0f;

    [Tooltip("Delay (in seconds) before forcing right hand release after it grabs.")]
    public float forceReleaseDelayRight = 0f;

    private Quaternion initialRotation;

    // For restoring this Level-2 object relative to its parent
    private Transform initialParent;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private bool isWobbling = false;
    private bool isBreaking = false;
    private bool shouldBreakAfterWobble = false;

    private bool leftReleaseScheduled = false;
    private bool rightReleaseScheduled = false;

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
        // Record own transform info (Level-2 object)
        initialRotation = transform.localRotation;
        initialParent = transform.parent;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        if (handEvents == null)
        {
            handEvents = GetComponent<PhysicalHandEvents>();
        }

        ownColliders = GetComponents<Collider>();
        ownRenderers = GetComponents<MeshRenderer>();

        // Cache Level-3 rigidbodies
        Rigidbody[] allBodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in allBodies)
        {
            if (rb.gameObject == this.gameObject)
                continue;

            ChildBodyInfo info = new ChildBodyInfo
            {
                rb = rb,
                originalParent = rb.transform.parent,
                originalLocalPosition = rb.transform.localPosition,
                originalLocalRotation = rb.transform.localRotation,
                originalLocalScale = rb.transform.localScale,
                originalIsKinematic = rb.isKinematic,
                originalUseGravity = rb.useGravity
            };

            childBodies.Add(info);
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

            // Break AFTER wobble ends if flagged
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

        float t = 0f;

        while (t < wobbleDuration)
        {
            t += Time.deltaTime;

            float angle = Mathf.Sin(t * wobbleSpeed) * maxAngle;
            transform.localRotation = initialRotation * Quaternion.Euler(0, 0, angle);

            if (handEvents != null &&
                (handEvents.leftHandGrabbing || handEvents.rightHandGrabbing))
            {
                // Mark to break after current wobble ends
                shouldBreakAfterWobble = true;

                // Schedule left hand force release
                if (handEvents.leftHandGrabbing &&
                    forceReleaseLeftOnGrab &&
                    !leftReleaseScheduled)
                {
                    leftReleaseScheduled = true;
                    StartCoroutine(ForceReleaseHandRoutine(true, forceReleaseDelayLeft));
                }

                // Schedule right hand force release
                if (handEvents.rightHandGrabbing &&
                    forceReleaseRightOnGrab &&
                    !rightReleaseScheduled)
                {
                    rightReleaseScheduled = true;
                    StartCoroutine(ForceReleaseHandRoutine(false, forceReleaseDelayRight));
                }
            }

            yield return null;
        }

        // Reset local rotation after wobble
        transform.localRotation = initialRotation;
        isWobbling = false;
    }

    /// <summary>
    /// Force release a hand via GameManager after an optional delay.
    /// isLeft == true -> left hand; false -> right hand.
    /// </summary>
    IEnumerator ForceReleaseHandRoutine(bool isLeft, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (GameManager.Instance != null)
        {
            if (isLeft)
            {
                GameManager.Instance.grabbingLeft = false;
            }
            else
            {
                GameManager.Instance.grabbingRight = false;
            }
        }
    }

    IEnumerator BreakApartRoutine()
    {
        if (isBreaking) yield break;
        isBreaking = true;

        // Disable own colliders and renderers
        if (ownColliders != null)
        {
            foreach (var col in ownColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        if (ownRenderers != null)
        {
            foreach (var ren in ownRenderers)
            {
                if (ren != null) ren.enabled = false;
            }
        }

        // Let child rigidbodies fall
        for (int i = 0; i < childBodies.Count; i++)
        {
            var info = childBodies[i];
            if (info.rb == null) continue;

            info.rb.transform.SetParent(null, true);
            info.rb.isKinematic = false;
            info.rb.useGravity = true;

            childBodies[i] = info;
        }

        // Wait before restoring
        yield return new WaitForSeconds(breakDuration);

        // Restore Level-2 object's transform relative to its parent
        if (initialParent != null)
        {
            transform.SetParent(initialParent, false);
        }

        transform.localPosition = initialLocalPosition;   // relative to moving scene parent
        transform.localRotation = initialLocalRotation;

        // Re-enable own colliders and renderers
        if (ownColliders != null)
        {
            foreach (var col in ownColliders)
            {
                if (col != null) col.enabled = true;
            }
        }

        if (ownRenderers != null)
        {
            foreach (var ren in ownRenderers)
            {
                if (ren != null) ren.enabled = true;
            }
        }

        // Restore child rigidbodies to original state
        foreach (var info in childBodies)
        {
            if (info.rb == null) continue;

            // Reset physics state
            info.rb.linearVelocity = Vector3.zero;
            info.rb.angularVelocity = Vector3.zero;

            // Re-parent back (if originalParent lost, fall back to this wobble object)
            Transform targetParent = info.originalParent != null ? info.originalParent : transform;
            info.rb.transform.SetParent(targetParent, false);

            // Child local position reset to (0,0,0) relative to its parent
            info.rb.transform.localPosition = Vector3.zero;

            // Restore local rotation (you can switch to identity if you prefer)
            info.rb.transform.localRotation = info.originalLocalRotation;

            // Restore relative size (local scale)
            info.rb.transform.localScale = info.originalLocalScale;

            // Restore kinematic & gravity
            info.rb.isKinematic = true; // or info.originalIsKinematic if you want exact original
            info.rb.useGravity = info.originalUseGravity;
        }

        isBreaking = false;
    }
}
