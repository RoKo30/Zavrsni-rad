using UnityEngine;

[CreateAssetMenu(fileName = "RoomPrefab", menuName = "Level/Room Prefab")]
public class RoomDataSO : ScriptableObject
{
    public GameObject prefab;
    public RoomType roomType;
}

public enum RoomType
{
    Room,
    Corridor
}