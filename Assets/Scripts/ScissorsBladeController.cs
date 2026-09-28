using UnityEngine;
using UnityEngine.InputSystem;

public class ScissorsBladeController : MonoBehaviour
{
    [Header("Blade Pivots")]
    public Transform leftBladePivot;
    public Transform rightBladePivot;

    [Header("Input")]
    [Tooltip("Assign the right-hand Select action used by the scissors controller.")]
    public InputActionReference selectAction;

    [Header("Cut Logic")]
    [Tooltip("Existing ScissorsCut component on CutTrigger.")]
    public ScissorsCut scissorsCut;

    [Header("Blade Angles")]
    [Range(2f, 35f)]
    public float openAngle = 14f;

    [Range(0f, 8f)]
    public float closedAngle = 1f;

    [Tooltip("Cut is triggered when the blades close to this angle or smaller.")]
    [Range(0.5f, 10f)]
    public float cutTriggerAngle = 3f;

    [Header("Motion")]
    [Tooltip("Angular opening/closing speed in degrees per second.")]
    public float angularSpeed = 220f;

    public bool startOpen = true;

    public bool IsClosed { get; private set; }

    private float leftBaseX;
    private float leftBaseZ;
    private float rightBaseX;
    private float rightBaseZ;

    private float currentLeftY;
    private float currentRightY;

    private bool cutTriggeredThisClosure;

    private void Awake()
    {
        CacheBaseRotations();
        SetImmediate(!startOpen);
    }

    private void OnEnable()
    {
        if (selectAction == null)
            return;

        selectAction.action.performed += OnSelectPerformed;
        selectAction.action.canceled += OnSelectCanceled;

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
        if (leftBladePivot == null || rightBladePivot == null)
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

        leftBladePivot.localRotation = Quaternion.Euler(
            leftBaseX,
            currentLeftY,
            leftBaseZ
        );

        rightBladePivot.localRotation = Quaternion.Euler(
            rightBaseX,
            currentRightY,
            rightBaseZ
        );

        if (IsClosed && !cutTriggeredThisClosure)
        {
            float leftAbs = Mathf.Abs(Mathf.DeltaAngle(0f, currentLeftY));
            float rightAbs = Mathf.Abs(Mathf.DeltaAngle(0f, currentRightY));

            if (leftAbs <= cutTriggerAngle && rightAbs <= cutTriggerAngle)
            {
                cutTriggeredThisClosure = true;

                if (scissorsCut != null)
                    scissorsCut.TryCut();
            }
        }
    }

    public void CloseBlades()
    {
        IsClosed = true;
    }

    public void OpenBlades()
    {
        IsClosed = false;
        cutTriggeredThisClosure = false;
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        CloseBlades();
    }

    private void OnSelectCanceled(InputAction.CallbackContext context)
    {
        OpenBlades();
    }

    private void CacheBaseRotations()
    {
        if (leftBladePivot != null)
        {
            Vector3 e = leftBladePivot.localEulerAngles;
            leftBaseX = NormalizeAngle(e.x);
            leftBaseZ = NormalizeAngle(e.z);
            currentLeftY = NormalizeAngle(e.y);
        }

        if (rightBladePivot != null)
        {
            Vector3 e = rightBladePivot.localEulerAngles;
            rightBaseX = NormalizeAngle(e.x);
            rightBaseZ = NormalizeAngle(e.z);
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

        if (leftBladePivot != null)
            leftBladePivot.localRotation = Quaternion.Euler(leftBaseX, leftY, leftBaseZ);

        if (rightBladePivot != null)
            rightBladePivot.localRotation = Quaternion.Euler(rightBaseX, rightY, rightBaseZ);

        cutTriggeredThisClosure = false;
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}
