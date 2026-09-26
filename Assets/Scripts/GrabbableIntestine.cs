using UnityEngine;

public class GrabbableIntestine : MonoBehaviour
{
    [System.Serializable]
    public class AdditionalAdhesionConstraint
    {
        public AdhesionController adhesionController;
        public Transform adhesionPoint;
    }

    public bool isGrabbed = false;

    [Header("Primary Adhesion")]
    public AdhesionController adhesionController;
    public Transform adhesionPoint;

    [Header("Additional Adhesions")]
    public AdditionalAdhesionConstraint[] additionalAdhesions;

    [Header("Post-Adhesiolysis Mobility")]
    [Tooltip("After every assigned adhesion is cut, the bowel becomes substantially more mobile.")]
    public bool increaseMobilityAfterAllAdhesionsCut = true;

    [Tooltip("How much farther the bowel may travel after complete adhesiolysis, relative to the initial adhesion span.")]
    [Min(1f)]
    public float postAdhesiolysisMobilityMultiplier = 2.5f;

    public Transform otherIntestine;
    public float minimumGap = 0.2f;
    public bool isLeftIntestine;

    private Transform grabPoint;
    private Vector3 grabOffset;

    private Vector3 initialPosition;
    private float postAdhesiolysisMaxTravel;
    private bool releaseMessageShown;

    private void Start()
    {
        initialPosition = transform.position;

        // Use the initial span of the assigned adhesions as a scale-independent
        // reference for the later, more mobile post-adhesiolysis state.
        float referenceSpan = GetLargestInitialAdhesionSpan();

        // Safe fallback if the scene is temporarily missing an assignment.
        if (referenceSpan <= 0.0001f)
            referenceSpan = 0.5f;

        postAdhesiolysisMaxTravel =
            referenceSpan * postAdhesiolysisMobilityMultiplier;
    }

    public void Grab(Transform newGrabPoint)
    {
        if (newGrabPoint == null)
            return;

        isGrabbed = true;
        grabPoint = newGrabPoint;

        // Keep the original distance between the instrument and the intestine.
        grabOffset = transform.position - grabPoint.position;
    }

    public void Release()
    {
        if (!isGrabbed)
            return;

        isGrabbed = false;
        grabPoint = null;
    }

    private void LateUpdate()
    {
        if (!isGrabbed || grabPoint == null)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 desiredPosition = grabPoint.position + grabOffset;

        // Prevent the two bowel loops from passing directly through one another.
        if (otherIntestine != null)
        {
            if (isLeftIntestine)
            {
                float maxX = otherIntestine.position.x - minimumGap;
                desiredPosition.x = Mathf.Min(desiredPosition.x, maxX);
            }
            else
            {
                float minX = otherIntestine.position.x + minimumGap;
                desiredPosition.x = Mathf.Max(desiredPosition.x, minX);
            }
        }

        bool anyAdhesionStillActive = HasAnyActiveAdhesion();

        if (anyAdhesionStillActive)
        {
            // While at least one adhesion remains, every intact adhesion can
            // still limit bowel excursion.
            desiredPosition = LimitByAdhesion(
                currentPosition,
                desiredPosition,
                adhesionController,
                adhesionPoint
            );

            if (additionalAdhesions != null)
            {
                foreach (AdditionalAdhesionConstraint constraint in additionalAdhesions)
                {
                    if (constraint == null)
                        continue;

                    desiredPosition = LimitByAdhesion(
                        currentPosition,
                        desiredPosition,
                        constraint.adhesionController,
                        constraint.adhesionPoint
                    );
                }
            }
        }
        else if (increaseMobilityAfterAllAdhesionsCut)
        {
            // Complete adhesiolysis: adhesion-based limits are gone.
            // The bowel is allowed a much larger excursion, but remains
            // finitely tethered to represent mesenteric attachment.
            Vector3 fromStart = desiredPosition - initialPosition;

            if (fromStart.magnitude > postAdhesiolysisMaxTravel)
            {
                desiredPosition =
                    initialPosition +
                    fromStart.normalized * postAdhesiolysisMaxTravel;
            }

            if (!releaseMessageShown)
            {
                Debug.Log("All adhesions cut. Bowel mobility increased.");
                releaseMessageShown = true;
            }
        }

        transform.position = desiredPosition;
    }

    private bool HasAnyActiveAdhesion()
    {
        if (IsAdhesionActive(adhesionController))
            return true;

        if (additionalAdhesions != null)
        {
            foreach (AdditionalAdhesionConstraint constraint in additionalAdhesions)
            {
                if (constraint != null &&
                    IsAdhesionActive(constraint.adhesionController))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsAdhesionActive(AdhesionController controller)
    {
        return controller != null &&
               controller.gameObject.activeInHierarchy;
    }

    private float GetLargestInitialAdhesionSpan()
    {
        float largest = GetAdhesionSpan(adhesionController, adhesionPoint);

        if (additionalAdhesions != null)
        {
            foreach (AdditionalAdhesionConstraint constraint in additionalAdhesions)
            {
                if (constraint == null)
                    continue;

                largest = Mathf.Max(
                    largest,
                    GetAdhesionSpan(
                        constraint.adhesionController,
                        constraint.adhesionPoint
                    )
                );
            }
        }

        return largest;
    }

    private float GetAdhesionSpan(
        AdhesionController controller,
        Transform point)
    {
        if (controller == null || point == null)
            return 0f;

        Transform otherPoint = controller.GetOtherPoint(point);

        if (otherPoint == null)
            return 0f;

        return Vector3.Distance(point.position, otherPoint.position);
    }

    private Vector3 LimitByAdhesion(
        Vector3 currentPosition,
        Vector3 desiredPosition,
        AdhesionController controller,
        Transform point)
    {
        // A cut adhesion is inactive and no longer restricts bowel movement.
        if (controller == null ||
            point == null ||
            !controller.gameObject.activeInHierarchy)
        {
            return desiredPosition;
        }

        Transform otherPoint = controller.GetOtherPoint(point);
        float maxDistance = controller.maxDistance;

        if (otherPoint == null || maxDistance <= 0f)
            return desiredPosition;

        Vector3 currentAdhesionPosition = point.position;
        Vector3 movement = desiredPosition - currentPosition;
        Vector3 desiredAdhesionPosition =
            currentAdhesionPosition + movement;

        float maxDistanceSquared = maxDistance * maxDistance;
        float desiredDistanceSquared =
            (desiredAdhesionPosition - otherPoint.position).sqrMagnitude;

        if (desiredDistanceSquared <= maxDistanceSquared)
            return desiredPosition;

        float currentDistanceSquared =
            (currentAdhesionPosition - otherPoint.position).sqrMagnitude;

        if (currentDistanceSquared <= maxDistanceSquared + 0.00001f)
        {
            // Find the furthest allowed point along the requested movement.
            float low = 0f;
            float high = 1f;

            for (int i = 0; i < 20; i++)
            {
                float middle = (low + high) * 0.5f;

                Vector3 testPosition = Vector3.Lerp(
                    currentAdhesionPosition,
                    desiredAdhesionPosition,
                    middle
                );

                float testDistanceSquared =
                    (testPosition - otherPoint.position).sqrMagnitude;

                if (testDistanceSquared <= maxDistanceSquared)
                    low = middle;
                else
                    high = middle;
            }

            return Vector3.Lerp(
                currentPosition,
                desiredPosition,
                low
            );
        }

        // If already beyond the limit, allow movement only if it relieves tension.
        if (desiredDistanceSquared >= currentDistanceSquared)
            return currentPosition;

        return desiredPosition;
    }
}
