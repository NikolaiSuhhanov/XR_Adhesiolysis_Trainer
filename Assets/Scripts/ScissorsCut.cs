using UnityEngine;

public class ScissorsCut : MonoBehaviour
{
    
    public AdhesionController adhesionController; // Reference to the AdhesionController script

    private CuttableAdhesion adhesionInRange; // Reference to the CuttableAdhesion script

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

        if (adhesionController == null) 
        
        {
            Debug.Log("AdhesionController is not assigned.");
            return;
        }

        if (!adhesionController.HasGoodTraction)
        {
            Debug.Log("Scissors do not have good traction. Cannot cut.");
            return;
        }

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
