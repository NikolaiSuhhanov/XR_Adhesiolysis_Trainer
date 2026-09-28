using UnityEngine;

public class ScissorsCut : MonoBehaviour
{
    private CuttableAdhesion adhesionInRange;

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

        // Each adhesion checks its own AdhesionController.
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
