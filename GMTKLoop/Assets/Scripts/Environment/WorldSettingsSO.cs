using UnityEngine;

[CreateAssetMenu(fileName = "WorldSettings", menuName = "ScriptableObjects/WorldSettings", order = 1)]
public class WorldSettingsSO : ScriptableObject
{
    [Header("Grid Settings")]
    public int width = 25;
    public int height = 25;
    public float cellSize = 5f;
    public Vector3 originPosition;
    public bool autoOffsetCenter = true;

    [Header("Procedural Generation")]
    public int roomCount = 6;
    public int minRoomSize = 4;
    public int maxRoomSize = 8;

    [Header("World Visuals")]
    public Color walkableColor = Color.green;
    public Material wallMaterial;
}
