using UnityEngine;

public class GrasperGrab : MonoBehaviour
{
    public Transform grabPoint; // The point where

    private GrabbableIntestine intestineInRange; // The intestine currently in range
    private GrabbableIntestine grabbedIntestine; // The intestine currently grabbed

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Grab();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Release();
        }
    }

    private void Grab()
    {
        if (intestineInRange != null && grabbedIntestine == null)
        {
            grabbedIntestine = intestineInRange;
            grabbedIntestine.Grab(grabPoint);
        }
    }

    private void Release()
    {
        if (grabbedIntestine != null)
        {
            grabbedIntestine.Release();
            grabbedIntestine = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GrabbableIntestine intestine = other.GetComponent<GrabbableIntestine>();
        if (intestine != null)
        {
            intestineInRange = intestine;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GrabbableIntestine intestine = other.GetComponent<GrabbableIntestine>();
        if (intestine != null && intestine == intestineInRange)
        {
            intestineInRange = null;
        }
    }

}





