using UnityEngine;

public class CuttableAdhesion : MonoBehaviour
{
    
    public bool isCut = false;

    public void Cut()
    {
        if (isCut)
        {
            return; // Already cut, do nothing
        }

        isCut = true; // Additional logic for when the object is cut can be added here

        gameObject.SetActive(false); // Deactivate the object to simulate cutting
    }


}
