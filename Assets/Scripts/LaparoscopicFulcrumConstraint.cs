using UnityEngine;

/// <summary>
/// Constrains a laparoscopic instrument so its shaft always passes through a fixed trocar point.
/// The XR controller can still control angulation, axial insertion/retraction and roll.
/// Designed to work with XR Device Simulator now and a real XR controller later.
/// </summary>
[DefaultExecutionOrder(500)]
public class LaparoscopicFulcrumConstraint : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Tracked XR controller/interactor transform driving this instrument.")]
    public Transform controllerTarget;

    [Tooltip("Fixed point representing the trocar / abdominal wall fulcrum.")]
    public Transform trocarPoint;

    [Header("Insertion")]
    [Tooltip("Use the instrument's current scene position as the starting insertion depth. Recommended for setup.")]
    public bool useCurrentPoseAsInitial = true;

    [Tooltip("Distance from the trocar to the instrument root along the inward shaft direction at startup.")]
    public float initialInsertionDepth = 0f;

    [Tooltip("Minimum permitted insertion depth.")]
    public float minInsertionDepth = -0.05f;

    [Tooltip("Maximum permitted insertion depth.")]
    public float maxInsertionDepth = 0.12f;

    [Tooltip("How strongly controller movement toward/away from the trocar changes insertion depth.")]
    public float insertionSensitivity = 1f;

    [Header("Rotation")]
    [Tooltip("Preserve controller roll around the shaft axis.")]
    public bool followControllerRoll = true;

    [Tooltip("Optional limit for instrument angulation around the trocar.")]
    public bool limitTilt = false;

    [Range(5f, 80f)]
    public float maxTiltDegrees = 45f;

    [Header("Smoothing")]
    public bool smoothMotion = false;

    [Min(0.01f)]
    public float positionSharpness = 25f;

    [Min(0.01f)]
    public float rotationSharpness = 25f;

    public float CurrentInsertionDepth { get; private set; }

    private float initialControllerDistance;
    private float initialAxialDistance;
    private Vector3 initialInwardDirection;
    private Vector3 initialOutsideDirection;
    private Quaternion modelRotationOffset = Quaternion.identity;
    private bool initialized;

    private void Start()
    {
        InitializeConstraint();
    }

    [ContextMenu("Reinitialize Fulcrum Constraint")]
    public void InitializeConstraint()
    {
        if (controllerTarget == null || trocarPoint == null)
        {
            initialized = false;
            return;
        }

        Vector3 toTrocar = trocarPoint.position - controllerTarget.position;
        if (toTrocar.sqrMagnitude < 0.000001f)
        {
            initialized = false;
            return;
        }

        initialControllerDistance = toTrocar.magnitude;
        initialInwardDirection = toTrocar.normalized;
        initialOutsideDirection = -initialInwardDirection;
        initialAxialDistance = Vector3.Dot(
            controllerTarget.position - trocarPoint.position,
            initialOutsideDirection
        );

        if (useCurrentPoseAsInitial)
        {
            initialInsertionDepth = Vector3.Dot(
                transform.position - trocarPoint.position,
                initialInwardDirection
            );

            // Expand the initial clamp automatically if the existing pose is outside
            // the default range, so enabling the constraint does not cause a sudden jump.
            minInsertionDepth = Mathf.Min(minInsertionDepth, initialInsertionDepth - 0.02f);
            maxInsertionDepth = Mathf.Max(maxInsertionDepth, initialInsertionDepth + 0.02f);
        }

        CurrentInsertionDepth = Mathf.Clamp(
            initialInsertionDepth,
            minInsertionDepth,
            maxInsertionDepth
        );

        Quaternion initialAim = BuildAimRotation(initialInwardDirection);
        modelRotationOffset = Quaternion.Inverse(initialAim) * transform.rotation;

        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            InitializeConstraint();
            if (!initialized)
                return;
        }

        Vector3 controllerRelative =
            controllerTarget.position - trocarPoint.position;

        // Separate angulation from insertion:
        // - lateral controller motion changes the instrument angle around the trocar;
        // - forward/back motion changes insertion depth only.
        // This prevents the instrument from flipping when the controller approaches
        // or even passes the trocar plane.
        float axialDistance = Vector3.Dot(
            controllerRelative,
            initialOutsideDirection
        );

        Vector3 lateralOffset =
            controllerRelative - initialOutsideDirection * axialDistance;

        float aimDistance = Mathf.Max(initialControllerDistance, 0.05f);

        Vector3 virtualHandlePosition =
            trocarPoint.position +
            initialOutsideDirection * aimDistance +
            lateralOffset;

        Vector3 inwardVector =
            trocarPoint.position - virtualHandlePosition;

        if (inwardVector.sqrMagnitude < 0.000001f)
            return;

        Vector3 inwardDirection = inwardVector.normalized;

        if (limitTilt)
        {
            inwardDirection = Vector3.RotateTowards(
                initialInwardDirection,
                inwardDirection,
                Mathf.Deg2Rad * maxTiltDegrees,
                0f
            ).normalized;
        }

        float controllerTravel =
            (initialAxialDistance - axialDistance) * insertionSensitivity;

        CurrentInsertionDepth = Mathf.Clamp(
            initialInsertionDepth + controllerTravel,
            minInsertionDepth,
            maxInsertionDepth
        );

        Vector3 targetPosition =
            trocarPoint.position + inwardDirection * CurrentInsertionDepth;

        Quaternion targetRotation =
            BuildAimRotation(inwardDirection) * modelRotationOffset;

        if (smoothMotion)
        {
            float positionT = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            float rotationT = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                positionT
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationT
            );
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }

    private Quaternion BuildAimRotation(Vector3 inwardDirection)
    {
        Vector3 upReference = followControllerRoll && controllerTarget != null
            ? controllerTarget.up
            : Vector3.up;

        Vector3 projectedUp = Vector3.ProjectOnPlane(upReference, inwardDirection);

        if (projectedUp.sqrMagnitude < 0.0001f)
        {
            projectedUp = Vector3.ProjectOnPlane(Vector3.up, inwardDirection);

            if (projectedUp.sqrMagnitude < 0.0001f)
                projectedUp = Vector3.ProjectOnPlane(Vector3.right, inwardDirection);
        }

        return Quaternion.LookRotation(inwardDirection, projectedUp.normalized);
    }

    private void OnDrawGizmosSelected()
    {
        if (trocarPoint == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(trocarPoint.position, 0.015f);

        if (controllerTarget != null)
        {
            Gizmos.DrawLine(controllerTarget.position, trocarPoint.position);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            trocarPoint.position,
            trocarPoint.position + transform.forward * 0.25f
        );
    }
}
