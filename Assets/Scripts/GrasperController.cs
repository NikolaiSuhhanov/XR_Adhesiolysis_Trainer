using UnityEngine;

public class GrasperController : MonoBehaviour
{    
    public float movementSpeed = 5f; // Speed of the grasper movement
    public float rotationSpeed = 100f; // Speed of the grasper rotation

    void Update()
    {
        MoveGrasper();
        RotateGrasper();

    }

    private void MoveGrasper()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        float depthInput = 0f;

        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            depthInput = 1f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            depthInput = -1f;
        }

        Vector3 movement = new Vector3(horizontalInput, verticalInput, depthInput) * movementSpeed * Time.deltaTime;

        transform.Translate(movement, Space.World);




    }

    private void RotateGrasper()
    {
        
        float rotationInput = 0f;
            
        if (Input.GetKey(KeyCode.Z))
        {
            rotationInput = 1f;
        }
        else if (Input.GetKey(KeyCode.X))
        {
            rotationInput = -1f;
        }

        transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);  





    }

}
