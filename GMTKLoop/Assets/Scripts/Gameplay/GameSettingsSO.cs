using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettingsSO : ScriptableObject
{
    public int maxMoveDistance = 3;
    public int maxActionPoints = 6;
}
