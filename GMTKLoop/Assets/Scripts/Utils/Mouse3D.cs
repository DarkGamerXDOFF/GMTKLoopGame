using UnityEngine;

public static class Mouse3D
{
    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            return raycastHit.point;
        else
        {
            //Debug.LogError("Mouse WorldPosition Not Found");
            return Vector3.zero;
        }
    }
}

public static class Mouse3D<T> 
{
    public static T GetMouseClickComponent()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
        {
            T component = raycastHit.collider.GetComponent<T>();
            if (component != null)
                return component;
            //else
            //    Debug.LogError($"Component of type {typeof(T)} not found on mouse clicked object");
        }
        else
            Debug.LogError("Mouse WorldPosition Not Found");

        return default;
    }
}

