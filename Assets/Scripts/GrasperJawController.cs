using UnityEngine;
using UnityEngine.InputSystem;

public class GrasperJawController : MonoBehaviour
{
    [Header("Jaw Pivots")]
    public Transform leftJawPivot;
    public Transform rightJawPivot;

    [Header("Input")]
    [Tooltip("Use the same Select action that is assigned to GrasperGrab.")]
    public InputActionReference selectAction;

    [Header("Jaw Angles")]
    [Range(1f, 30f)]
    public float openAngle = 12f;

    [Range(0f, 10f)]
    public float closedAngle = 1f;

    [Header("Motion")]
    [Tooltip("Angular speed of opening and closing in degrees per second.")]
    public float angularSpeed = 180f;

    [Tooltip("Start with the jaws open.")]
    public bool startOpen = true;

    public bool IsClosed { get; private set; }

    private float leftX;
    private float leftZ;
    private float rightX;
    private float rightZ;

    private float currentLeftY;
    private float currentRightY;

    private void Awake()
    {
        CacheBaseRotations();

        if (startOpen)
            SetImmediate(false);
        else
            SetImmediate(true);
    }

    private void OnEnable()
    {
        if (selectAction == null)
            return;

        selectAction.action.performed += OnSelectPerformed;
        selectAction.action.canceled += OnSelectCanceled;

        // GrasperGrab usually enables this same action already.
        // Enabling here as well is safe, but we intentionally do not disable
        // the shared action in OnDisable.
        if (!selectAction.action.enabled)
            selectAction.action.Enable();
    }

    private void OnDisable()
    {
        if (selectAction == null)
            return;

        selectAction.action.performed -= OnSelectPerformed;
        selectAction.action.canceled -= OnSelectCanceled;
    }

    private void Update()
    {
        if (leftJawPivot == null || rightJawPivot == null)
            return;

        float targetLeftY = IsClosed ? -closedAngle : -openAngle;
        float targetRightY = IsClosed ? closedAngle : openAngle;

        currentLeftY = Mathf.MoveTowardsAngle(
            currentLeftY,
            targetLeftY,
            angularSpeed * Time.deltaTime
        );

        currentRightY = Mathf.MoveTowardsAngle(
            currentRightY,
            targetRightY,
            angularSpeed * Time.deltaTime
        );

        leftJawPivot.localRotation = Quaternion.Euler(
            leftX,
            currentLeftY,
            leftZ
        );

        rightJawPivot.localRotation = Quaternion.Euler(
            rightX,
            currentRightY,
            rightZ
        );
    }

    public void CloseJaws()
    {
        IsClosed = true;
    }

    public void OpenJaws()
    {
        IsClosed = false;
    }

    public void SetClosed(bool closed)
    {
        IsClosed = closed;
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        CloseJaws();
    }

    private void OnSelectCanceled(InputAction.CallbackContext context)
    {
        OpenJaws();
    }

    private void CacheBaseRotations()
    {
        if (leftJawPivot != null)
        {
            Vector3 e = leftJawPivot.localEulerAngles;
            leftX = NormalizeAngle(e.x);
            leftZ = NormalizeAngle(e.z);
            currentLeftY = NormalizeAngle(e.y);
        }

        if (rightJawPivot != null)
        {
            Vector3 e = rightJawPivot.localEulerAngles;
            rightX = NormalizeAngle(e.x);
            rightZ = NormalizeAngle(e.z);
            currentRightY = NormalizeAngle(e.y);
        }
    }

    private void SetImmediate(bool closed)
    {
        IsClosed = closed;

        float leftY = closed ? -closedAngle : -openAngle;
        float rightY = closed ? closedAngle : openAngle;

        currentLeftY = leftY;
        currentRightY = rightY;

        if (leftJawPivot != null)
            leftJawPivot.localRotation = Quaternion.Euler(leftX, leftY, leftZ);

        if (rightJawPivot != null)
            rightJawPivot.localRotation = Quaternion.Euler(rightX, rightY, rightZ);
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}
