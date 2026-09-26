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

    public Transform otherIntestine;
    public float minimumGap = 0.2f;
    public bool isLeftIntestine;

    private Transform grabPoint;
    private Vector3 grabOffset;

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

        // Prevent the two bowel loops from crossing.
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

        // Primary adhesion (kept for backwards compatibility with the working scene).
        desiredPosition = LimitByAdhesion(
            currentPosition,
            desiredPosition,
            adhesionController,
            adhesionPoint
        );

        // Optional additional adhesions.
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

        transform.position = desiredPosition;
    }

    private Vector3 LimitByAdhesion(
        Vector3 currentPosition,
        Vector3 desiredPosition,
        AdhesionController controller,
        Transform point)
    {
        // Once an adhesion is cut its GameObject is inactive, so it must no
        // longer restrict bowel movement.
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
        Vector3 desiredAdhesionPosition = currentAdhesionPosition + movement;

        float maxDistanceSquared = maxDistance * maxDistance;
        float desiredDistanceSquared =
            (desiredAdhesionPosition - otherPoint.position).sqrMagnitude;

        if (desiredDistanceSquared <= maxDistanceSquared)
            return desiredPosition;

        float currentDistanceSquared =
            (currentAdhesionPosition - otherPoint.position).sqrMagnitude;

        if (currentDistanceSquared <= maxDistanceSquared + 0.00001f)
        {
            // Binary search for the furthest allowed point along this move.
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

            return Vector3.Lerp(currentPosition, desiredPosition, low);
        }

        // If already beyond the limit, allow only movement that relieves tension.
        if (desiredDistanceSquared >= currentDistanceSquared)
            return currentPosition;

        return desiredPosition;
    }
}
