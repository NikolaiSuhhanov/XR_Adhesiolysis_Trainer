using UnityEngine;
using UnityEngine.InputSystem;

public class GrasperGrab : MonoBehaviour
{
    public Transform grabPoint; // The point where

    public InputActionReference selectAction; // The input action for grabbing

    private GrabbableIntestine intestineInRange; // The intestine currently in range
    private GrabbableIntestine grabbedIntestine; // The intestine currently grabbed

   private void OnEnable()
    {
        if (selectAction != null)
        {
            
            selectAction.action.performed += OnSelectPerformed;
            selectAction.action.canceled += OnSelectCanceled;
            selectAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (selectAction != null)
        {
            selectAction.action.performed -= OnSelectPerformed;
            selectAction.action.canceled -= OnSelectCanceled;
            selectAction.action.Disable();
        }
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        Grab();
    }

    private void OnSelectCanceled(InputAction.CallbackContext context)
    {
        Release();
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





