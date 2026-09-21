using UnityEngine;

public class GrabbableIntestine : MonoBehaviour
{
    public bool isGrabbed = false;

    [Header("Adhesion")]

    public AdhesionController adhesionController;

    public Transform adhesionPoint;

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

        // Keep the original distance between
        // the instrument and the intestine.

        grabOffset =
            transform.position - grabPoint.position;
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


        // 1. Current intestine position

        Vector3 currentPosition = transform.position;


        // 2. Desired position based on the grasper

        Vector3 desiredPosition =
            grabPoint.position + grabOffset;


        // 3. Prevent the intestines from crossing

        if (otherIntestine != null)
        {
            if (isLeftIntestine)
            {
                float maxX =
                    otherIntestine.position.x - minimumGap;

                desiredPosition.x =
                    Mathf.Min(desiredPosition.x, maxX);
            }
            else
            {
                float minX =
                    otherIntestine.position.x + minimumGap;

                desiredPosition.x =
                    Mathf.Max(desiredPosition.x, minX);
            }
        }


        // 4. Limit adhesion stretching

        if (adhesionController != null &&
            adhesionPoint != null)
        {
            Transform otherPoint =
                adhesionController.GetOtherPoint(adhesionPoint);

            float maxDistance =
                adhesionController.maxDistance;


            if (otherPoint != null && maxDistance > 0f)
            {
                Vector3 currentAdhesionPosition =
                    adhesionPoint.position;


                // Calculate where the adhesion point
                // would move together with the intestine.

                Vector3 movement =
                    desiredPosition - currentPosition;


                Vector3 desiredAdhesionPosition =
                    currentAdhesionPosition + movement;


                float maxDistanceSquared =
                    maxDistance * maxDistance;


                float desiredDistanceSquared =
                    (desiredAdhesionPosition -
                     otherPoint.position).sqrMagnitude;


                // Check whether the desired movement
                // would overstretch the adhesion.

                if (desiredDistanceSquared >
                    maxDistanceSquared)
                {
                    float currentDistanceSquared =
                        (currentAdhesionPosition -
                         otherPoint.position).sqrMagnitude;


                    if (currentDistanceSquared <=
                        maxDistanceSquared + 0.00001f)
                    {
                        // Find the furthest allowed position
                        // along the desired movement path.

                        float low = 0f;

                        float high = 1f;


                        for (int i = 0; i < 20; i++)
                        {
                            float middle =
                                (low + high) * 0.5f;


                            Vector3 testPosition =
                                Vector3.Lerp(
                                    currentAdhesionPosition,
                                    desiredAdhesionPosition,
                                    middle
                                );


                            float testDistanceSquared =
                                (testPosition -
                                 otherPoint.position).sqrMagnitude;


                            if (testDistanceSquared <=
                                maxDistanceSquared)
                            {
                                low = middle;
                            }
                            else
                            {
                                high = middle;
                            }
                        }


                        desiredPosition =
                            Vector3.Lerp(
                                currentPosition,
                                desiredPosition,
                                low
                            );
                    }
                    else
                    {
                        // If the adhesion is already outside
                        // its limit, prevent further stretching.

                        if (desiredDistanceSquared >=
                            currentDistanceSquared)
                        {
                            desiredPosition =
                                currentPosition;
                        }
                    }
                }
            }
        }


        // 5. Move the intestine only once.
        // The adhesion point follows automatically
        // because it is a child of this object.

        transform.position = desiredPosition;
    }
}