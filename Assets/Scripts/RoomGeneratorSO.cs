using UnityEngine;


[CreateAssetMenu(fileName = "RoomGeneratorSO", menuName = "Level/RoomGeneratorData")]
public class RoomGeneratorSO : ScriptableObject {
    public int seed;
    public int numberOfLevels;
}
