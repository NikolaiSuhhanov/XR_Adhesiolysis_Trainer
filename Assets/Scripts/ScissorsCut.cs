using UnityEngine;

public class ScissorsCut : MonoBehaviour
{
    private CuttableAdhesion adhesionInRange;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            TryCut();
        }
    }

    private void TryCut()
    {
        if (adhesionInRange == null)
        {
            Debug.Log("No adhesion in range to cut.");
            return;
        }

        // Each adhesion now checks its OWN AdhesionController.
        // This allows multiple adhesions to be cut independently.
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
