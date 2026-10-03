using UnityEngine;

public class ScissorsCut : MonoBehaviour
{
    private CuttableAdhesion adhesionInRange;

    [SerializeField] private TrainingMetrics trainingMetrics;

    private void Update()
    {
        // Desktop fallback kept for testing without a headset.
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            TryCut();
        }
    }

    public void TryCut()
    {
        if (adhesionInRange == null)
        {
            Debug.Log("No adhesion in range to cut.");
            return;
        }

        AdhesionController adhesionController =
            adhesionInRange.GetComponent<AdhesionController>();

        if (adhesionController != null &&
            !adhesionController.HasGoodTraction)
        {
            if (trainingMetrics != null)
            {
                trainingMetrics.RegisterInvalidCutAttempt();
            }
        }

        // Existing cutting logic remains unchanged.
        adhesionInRange.Cut();
    }

    private void OnTriggerEnter(Collider other)
    {
        CuttableAdhesion adhesion = other.GetComponent<CuttableAdhesion>();

        if (adhesion != null)
        {
            adhesionInRange = adhesion;
            Debug.Log("Adhesion in range to cut.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CuttableAdhesion adhesion = other.GetComponent<CuttableAdhesion>();

        if (adhesion != null && adhesion == adhesionInRange)
        {
            adhesionInRange = null;
            Debug.Log("Adhesion out of range.");
        }
    }
}
