using UnityEngine;

public class ScissorsController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float rotationSpeed = 100f;

    private void Update()
    {
        MoveScissors();
        RotateScissors();
    }


    private void MoveScissors()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;
        float depthInput = 0f;

        if (Input.GetKey(KeyCode.RightArrow ))
        {
            horizontalInput = 1f;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            verticalInput = 1f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalInput = -1f;
        }

        if (Input.GetKey(KeyCode.Slash))
        {
            depthInput = 1f;
        }
        if (Input.GetKey(KeyCode.Period))
        {
            depthInput = -1f;
        }

        Vector3 movement = new Vector3(horizontalInput, verticalInput, depthInput) * movementSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
    private void RotateScissors()
    {
        float rotationInput = 0f;

        if (Input.GetKey(KeyCode.Semicolon))
        {
            rotationInput = 1f;
        }
        else if (Input.GetKey(KeyCode.Quote))
        {
            rotationInput = -1f;
        }

        transform.Rotate(Vector3.forward, rotationInput * rotationSpeed * Time.deltaTime);




    }

}
