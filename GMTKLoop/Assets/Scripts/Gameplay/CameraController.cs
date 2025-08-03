using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController i;

    [SerializeField] private CinemachineFollow cmFollow;

    [SerializeField] private Transform followTransform;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 25f;
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private Vector2 moveBoundsX = new Vector2(-50, 50);
    [SerializeField] private Vector2 moveBoundsZ = new Vector2(-50, 50);
    [SerializeField] private bool canMoveFree = false;

    [Header("Rotation Settings")]
    [SerializeField] private float rotateSpeed = 100f;
    [SerializeField] private float mouseRotateSpeed = 100f;
    [SerializeField] private float verticalMinAngle = 20f;
    [SerializeField] private float verticalMaxAngle = 80f;
    [SerializeField] private bool invertMouseY = false;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 400f;
    [SerializeField] private float zoomMin = 10f;
    [SerializeField] private float zoomMax = 100f;

    private float yaw = 0f;
    private float pitch = 60f;
    private float distance = 50f;

    private bool isFollowing = true;
    private Vector3 manualPosition;

    private void Awake()
    {
        if (i == null) i = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateFollowOffset();
        if (followTransform != null)
            transform.position = followTransform.position;
    }

    private void Update()
    {
        HandleZoomInput();
        HandleRotationInput();

        if (isFollowing && followTransform != null)
        {
            transform.position = Vector3.Lerp(transform.position, followTransform.position, Time.deltaTime * 5f);
        }
        else
        {
            HandleMovementInput();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && canMoveFree)
        {
            isFollowing = false;
            manualPosition = transform.position;
        }

        UpdateFollowOffset();
    }

    public static void SetTarget(Transform target)
    {
        i.followTransform = target;
        i.isFollowing = true;
    }

    private void HandleMovementInput()
    {
        float shift = Input.GetKey(KeyCode.LeftShift) ? speedMultiplier : 1f;

        Vector3 forward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, yaw, 0) * Vector3.right;

        Vector3 dir = Vector3.zero;
        dir += forward * Input.GetAxisRaw("Vertical");
        dir += right * Input.GetAxisRaw("Horizontal");
        dir.Normalize();

        manualPosition += dir * moveSpeed * shift * Time.deltaTime;
        manualPosition.x = Mathf.Clamp(manualPosition.x, moveBoundsX.x, moveBoundsX.y);
        manualPosition.z = Mathf.Clamp(manualPosition.z, moveBoundsZ.x, moveBoundsZ.y);
        manualPosition.y = 0;

        transform.position = manualPosition;
    }

    private void HandleRotationInput()
    {
        // Mouse rotation
        if (Input.GetMouseButton(2))
        {
            yaw += Input.GetAxis("Mouse X") * mouseRotateSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * mouseRotateSpeed * Time.deltaTime;
        }

        // Keyboard rotation
        if (Input.GetKey(KeyCode.Q)) yaw -= rotateSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) yaw += rotateSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.R)) pitch += rotateSpeed * Time.deltaTime * (invertMouseY? -1 : 1);
        if (Input.GetKey(KeyCode.F)) pitch -= rotateSpeed * Time.deltaTime * (invertMouseY ? -1 : 1);

        pitch = Mathf.Clamp(pitch, verticalMinAngle, verticalMaxAngle);
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.PageUp)) distance -= zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.PageDown)) distance += zoomSpeed * Time.deltaTime;

        distance = Mathf.Clamp(distance, zoomMin, zoomMax);
    }

    private void UpdateFollowOffset()
    {
        Vector3 offsetDir = Quaternion.Euler(pitch, yaw, 0) * Vector3.back;
        Vector3 offset = offsetDir * distance;
        cmFollow.FollowOffset = Vector3.Lerp(cmFollow.FollowOffset, offset, Time.deltaTime * 10f);
    }
}
