using UnityEngine;

[DefaultExecutionOrder(100)]
public class AdhesionController : MonoBehaviour
{

    public Transform pointA;
    public Transform pointB;

    [Header("Tension Settings")]
    public float goodTractionMultiplier = 1.25f;
    public float maxDistanceMultiplier = 1.5f;



    public float maxDistance { get; private set; }
    public float normalDistance  { get; private set; }
public float goodTraction { get; private set; }

    public float CurrentDistance { get; private set; }
    public bool HasGoodTraction { get; private set; }
    public bool IsOverstretched { get; private set; }

    public Renderer adhesionRenderer;

    public Color goodTractionColor = Color.green;
    public Color overstretchedColor = Color.red;
    public Color normalColor = Color.yellow;

    private void Start()
    {
        if (pointA != null && pointB != null)
        {
            normalDistance = Vector3.Distance(pointA.position, pointB.position);
            goodTraction = normalDistance * goodTractionMultiplier;
            maxDistance = normalDistance * maxDistanceMultiplier;
        }
    }

    private void LateUpdate()
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

            float worldDistanse = direction.magnitude;

            transform.rotation = Quaternion.FromToRotation(Vector3.right, direction.normalized);

            float parentScaleX = transform.parent != null ? transform.parent.lossyScale.x : 1f;

            float localLength = worldDistanse / parentScaleX;

            transform.localScale = new Vector3(localLength, 0.06f, 0.06f);
        }
    }

    private void UpdateTension()
    {
        
        if (pointA == null || pointB == null) return;

        CurrentDistance = Vector3.Distance(pointA.position, pointB.position);

        HasGoodTraction = CurrentDistance >= goodTraction && CurrentDistance < maxDistance * 0.95f;

        IsOverstretched = CurrentDistance >= maxDistance * 0.95f;

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
