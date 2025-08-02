using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController i;

    [SerializeField] private Transform target;

    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            // Set the camera position to the target's position
            transform.position = target.position;
            // Optionally, you can also set the camera rotation to match the target's rotation
            //transform.rotation = target.rotation;
        }
    }

    public static void SetTarget(Transform newTarget)
    {
        i.target = newTarget;
    }
}
