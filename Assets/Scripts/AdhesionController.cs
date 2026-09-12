using UnityEngine;

public class AdhesionController : MonoBehaviour
{

    public Transform pointA;
    public Transform pointB;

    [Header("Tension Settings")]
    public float maxDistance = 2.0f;
    public float normalDistance = 1.52f;
    public float goodTraction = 1.8f;

    public float CurrentDistance { get; private set; }
    public bool HasGoodTraction { get; private set; }
    public bool IsOverstretched { get; private set; }

    public Renderer adhesionRenderer;

    public Color goodTractionColor = Color.green;
    public Color overstretchedColor = Color.red;
    public Color normalColor = Color.yellow;


    private void Update()
    {
        UpdateAdhesion();
        UpdateTension();

        UpdateAdhesionColor();
    }

    private void UpdateAdhesionColor()
    {
        if (adhesionRenderer != null)
        {
            if (IsOverstretched)
            {
                adhesionRenderer.material.color = overstretchedColor;
            }
            else if (HasGoodTraction)
            {
                adhesionRenderer.material.color = goodTractionColor;
            }
            else
            {
                adhesionRenderer.material.color = normalColor;
            }


        }
    }

    private void UpdateAdhesion()
    {
        if (pointA != null && pointB != null)
        {
            transform.position = (pointA.position + pointB.position) / 2f;

            Vector3 direction = pointB.position - pointA.position;

            float distance = direction.magnitude;

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
            }
                     

            Vector3 scale = transform.localScale;
            scale.x = distance;
            transform.localScale = scale;   
        }
    }

    private void UpdateTension()
    {
        
        if (pointA == null || pointB == null) return;

        CurrentDistance = Vector3.Distance(pointA.position, pointB.position);

        HasGoodTraction = CurrentDistance >= goodTraction && CurrentDistance < maxDistance * 0.975f;

        IsOverstretched = CurrentDistance >= maxDistance * 0.975f;

    }

    public Transform GetOtherPoint(Transform point)
    {
        if (point == pointA)
        {
            return pointB;
        }
        else if (point == pointB)
        {
            return pointA;
        }
        else
        {
            return null;
        }
    }
}
