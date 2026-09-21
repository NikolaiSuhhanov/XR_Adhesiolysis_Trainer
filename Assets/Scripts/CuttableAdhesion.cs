using UnityEngine;

public class CuttableAdhesion : MonoBehaviour
{
    
    public bool isCut = false;

    public AdhesionController adhesionController;

    public void Cut()
    {
        if (isCut)
        {
            return; // Already cut, do nothing
        }

        if (adhesionController == null)
        {
            Debug.LogWarning("AdhesionController is not assigned.");
            return;
        }

        if (!adhesionController.HasGoodTraction)
        {
            Debug.Log("Cannot cut: Adhesion does not have good traction.");
            return;
        }

        isCut = true;

        Debug.Log("Adhesion has been cut.");

        gameObject.SetActive(false);


    }


}
