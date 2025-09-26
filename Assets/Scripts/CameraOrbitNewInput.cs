using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CameraOrbitNewInput : MonoBehaviour
{
    public GameController _gameController;
    public Transform target;
    public float rotationSensitivity = 0.15f;
    public float minPitch = 10f;
    public float maxPitch = 80f;

    public float minDistance = 1f;
    public float maxDistance = 50f;
    public float zoomPerNotch = 100.0f;

    [Header("White Turn Camera")]
    public float whiteYaw = 0f;
    public float whitePitch = -90f;
    public float whiteDistance = 10f;

    [Header("Black Turn Camera")]
    public float blackYaw = 180f;
    public float blackPitch = 90f;
    public float blackDistance = 10f;

    public float returnSpeed = 2f;

    private float _yaw;
    private float Yaw
    {
        get => _yaw;
        set
        {
            _yaw = value % 360f;
            if (_yaw < 0f) _yaw += 360f;
        }
    }
    private float _pitch;
    private float Pitch
    {
        get => _pitch;
        set
        {
            _pitch = Mathf.Clamp(value, minPitch, maxPitch);
        }
    }

    private float distance;

    private Coroutine returnRoutine;

    void Start()
    {
        Vector3 offset = transform.position - target.position;
        distance = offset.magnitude;
        Vector3 angles = transform.eulerAngles;
        Yaw = angles.y;
        Pitch = angles.x;
    }

    void Update()
    {
        if (Mouse.current.middleButton.isPressed)
        {
            if (returnRoutine != null)
            {
                StopCoroutine(returnRoutine);
                returnRoutine = null;
            }

            Vector2 delta = Mouse.current.delta.ReadValue();
            Yaw += delta.x * rotationSensitivity;
            Pitch -= delta.y * rotationSensitivity;
            Pitch = Mathf.Clamp(Pitch, minPitch, maxPitch);
        }
        else if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            FlipCamera(2.0f);
        }

        float scrollY = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scrollY) > 0.01f)
        {
            float notches = scrollY / 120f;
            distance = Mathf.Clamp(distance - notches * zoomPerNotch, minDistance, maxDistance);
        }

        ApplyCameraTransform();
    }

    void ApplyCameraTransform()
    {
        Quaternion rot = Quaternion.Euler(Pitch, Yaw, 0f);
        Vector3 offset = rot * new Vector3(0f, 0f, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }

    public IEnumerator ReturnToTurnView(float delay, float animationSpeed)
    {
        yield return new WaitForSeconds(delay);
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        if (_gameController.IsWhiteTurn())
            returnRoutine = StartCoroutine(ReturnToView(whiteYaw, whitePitch, whiteDistance, animationSpeed));
        else
            returnRoutine = StartCoroutine(ReturnToView(blackYaw, blackPitch, blackDistance, animationSpeed));
    }

    public void FlipCamera(float delay, float animationSpeed = 0)
    {
        returnRoutine = StartCoroutine(ReturnToTurnView(delay, animationSpeed));
    }


    IEnumerator ReturnToView(float targetYaw, float targetPitch, float targetDistance, float animationSpeed)
    {
        if (animationSpeed == 0) animationSpeed = returnSpeed;
        float limit = 1200;
        while (limit > 0 && (Mathf.Abs(Mathf.DeltaAngle(Yaw, targetYaw)) > 1 ||
               Mathf.Abs(Mathf.DeltaAngle(Pitch, targetPitch)) > 1 ||
               Mathf.Abs(distance - targetDistance) > 0.02f))
        {
            limit--;
            Yaw = Mathf.LerpAngle(Yaw, targetYaw, Time.deltaTime * animationSpeed);
            Pitch = Mathf.Lerp(Pitch, targetPitch, Time.deltaTime * animationSpeed);
            distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * animationSpeed);

            ApplyCameraTransform();
            yield return null;
        }

        Yaw = targetYaw;
        Pitch = targetPitch;
        distance = targetDistance;
        ApplyCameraTransform();
    }




}
