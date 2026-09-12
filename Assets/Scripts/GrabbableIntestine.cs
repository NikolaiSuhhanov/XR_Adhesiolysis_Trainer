using UnityEngine;

public class GrabbableIntestine : MonoBehaviour
{
    public bool isGrabbed = false;

    [Header("Adhesion")]
    public AdhesionController adhesionController;

    public Transform adhesionPoint;

    private Transform grabPoint;

    private Vector3 grabOffset;

    public Transform otherIntestine;

    public float minimumGap = 0.3f;

    public bool isLeftIntestine;

    public void Grab(Transform newGrabPoint)
    {
        if (isGrabbed) return;

        if (newGrabPoint == null) return;
        
        isGrabbed = true;

        grabPoint = newGrabPoint;

        grabOffset = transform.position - grabPoint.position;

    }

    public void Release()
    {
        if (!isGrabbed) return;
        isGrabbed = false;
        grabPoint = null;
    }

    private void LateUpdate()
    {
        if (!isGrabbed) return;
        
        if (grabPoint == null) return;

        if (adhesionController == null) return;

        if (adhesionPoint == null) return;

        MoveIntestine();
    }

    private void MoveIntestine()
    {
        Vector3 desiredIntestinePosition = grabPoint.position + grabOffset;

        if (otherIntestine != null)
        {
            if (isLeftIntestine)
            {
               float maxX = otherIntestine.position.x - minimumGap;

                if (desiredIntestinePosition.x > maxX)
                {
                      desiredIntestinePosition.x = maxX;
                }
            }

            else
            {
                float minX = otherIntestine.position.x + minimumGap;
                if (desiredIntestinePosition.x < minX)
                {
                    desiredIntestinePosition.x = minX;
                }
            }

        }


        Vector3 movement = desiredIntestinePosition - transform.position;
        Vector3 desiredAdhesionPointPosition = adhesionPoint.position + movement;

        Transform otherPoint = adhesionController.GetOtherPoint(adhesionPoint);

        if (otherPoint == null) return;

        Vector3 direction = desiredAdhesionPointPosition - otherPoint.position;

        float desiredDistance = direction.magnitude;

        if (desiredDistance <= adhesionController.maxDistance)
        {
            transform.position = desiredIntestinePosition;
            adhesionPoint.position = desiredAdhesionPointPosition;
        }
        else
        {
            Vector3 clampedDirection = direction.normalized * adhesionController.maxDistance;
            adhesionPoint.position = otherPoint.position + clampedDirection;
            Vector3 clampedIntestinePosition = adhesionPoint.position - grabOffset;
            transform.position = clampedIntestinePosition;
        }



    }


}
