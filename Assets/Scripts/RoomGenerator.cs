using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private RoomGeneratorSO levelData;

    [Header("Room Config")]
    [SerializeField] private int seed = 12345;
    public int numberOfLevels = 3;
    public List<RoomDataSO> roomPool; // Pool of random room types
    public List<RoomDataSO> corridorPool; // Pool of corridor types

    public RoomDataSO startingRoomSO;
    public RoomDataSO nextLevelRoomSO;
    public RoomDataSO endLevelRoomSO;


    [FormerlySerializedAs("numberOfRooms")] public int numberOfRoomsPerLevel = 3;
    private bool lastWasRoom = true;

    [Header("Placement")]

    public float borderPadding = 0.1f;

    public List<List<Room>> placedRooms = new();
    private List<Bounds> debugBounds = new();
    private Room MapStartRoom;

    void Start() {
        seed = levelData.seed;
        numberOfLevels = levelData.numberOfLevels;
        
        Random.InitState(seed);
        MapStartRoom = PlaceRoom(startingRoomSO, Vector3.zero, Quaternion.identity);
        Room startRoom = MapStartRoom;
        
        for (int i = 0; i < numberOfLevels; i++) {
            placedRooms.Add(new List<Room>());
            startRoom = GenerateLevel(startRoom, i);
            if (startRoom == null) {
                break;
            }
        }
    }

    Room GenerateLevel(Room startingRoom, int level)
    {
        //debugBounds.Clear(); // clear previous debug data
        
        //Room startRoom = PlaceRoom(startingRoomSO, Vector3.zero, Quaternion.identity);
        placedRooms[level].Add(startingRoom); // todo ???

        List<(Room fromRoom, Transform freeSocket)> openSockets =            
            new List<(Room fromRoom, Transform freeSocket)>();
        //new List<(Room fromRoom, Transform freeSocket, RoomDataSO data)>();
        
        foreach (var socket in startingRoom.Sockets)
            openSockets.Add((startingRoom, socket));
        //openSockets.Add((startRoom, socket, startingRoomSO));
        
        if(level != 0) openSockets.RemoveAt(0);
        
        int attempts = 0; 
        while (placedRooms[level].Count < numberOfRoomsPerLevel && openSockets.Count > 0 && attempts < 500)
        {
            //Debug.Log("--------------------------------------------------------------");
            //Debug.Log("Open sockets: " + openSockets.Count);
            //Debug.Log($"Level {level}: Attempt {attempts}, OpenSockets: {openSockets.Count}, Rooms: {placedRooms[level].Count}");

            attempts++;

            int index = Random.Range(0, openSockets.Count);
            //Debug.Log(index);
            var (fromRoom, fromSocket) = openSockets[index];
            //var (fromRoom, fromSocket, roomDataSo) = openSockets[index];
            //Debug.Log(fromRoom + " : " + fromSocket.position);
            
            openSockets.RemoveAt(index);

            Vector3 fromSocketDir = fromSocket.forward;
            Vector3 targetDir = -fromSocketDir;
            
            List<RoomDataSO> poolToUse;
            RoomType fromType = fromRoom.RoomType;

            if (fromType == RoomType.Room)
                poolToUse = corridorPool;
            else
                poolToUse = roomPool;

            Shuffle(poolToUse);

            foreach (RoomDataSO candidateSO in poolToUse)
            {
                GameObject candidateObj = Instantiate(candidateSO.prefab, fromRoom.transform.position, Quaternion.identity);
                Room candidateRoom = candidateObj.GetComponent<Room>();
                //Debug.Log(candidateRoom.name);
                // Try every socket in this room
                for (int i = 0; i < candidateRoom.Sockets.Count; i++) {
                    Transform socketInCandidate = candidateRoom.Sockets[i];
                    //Debug.Log("here");
                    Vector3 toSocketDir = socketInCandidate.forward;
                    if (Vector3.Angle(toSocketDir, targetDir) > 1f) {
                        Quaternion rotateRoomForAngle = Quaternion.FromToRotation(toSocketDir, targetDir);
                        candidateObj.transform.rotation = candidateObj.transform.rotation * rotateRoomForAngle;
                        if (candidateObj.transform.up != Vector3.up)
                            candidateObj.transform.Rotate(0f, 0f, 180f);;
                    }

                    candidateRoom.Initialize();
                    socketInCandidate = candidateRoom.Sockets[i];
                    float epsilon = 0.001f;

                    Vector3 padding = new Vector3(
                        Mathf.Abs(fromSocketDir.x) < epsilon ? 0 : borderPadding,
                        0,
                        Mathf.Abs(fromSocketDir.z) < epsilon ? 0 : borderPadding
                    );                    
                    Vector3 offset = (fromSocket.position - socketInCandidate.position) /*+ padding*/;
                    //Debug.Log("offset: " + offset);
                    candidateObj.transform.position += offset;

                    if (candidateRoom != null && !OverlapsAny(candidateRoom)) {
                        candidateRoom.RoomType = candidateSO.roomType;
                        candidateRoom.Level = level;
                        placedRooms[level].Add(candidateRoom);
                        fromRoom.ConnectTo(candidateRoom);
                        candidateRoom.ConnectTo(fromRoom);

                        foreach (var s in candidateRoom.Sockets)
                        {
                            if (s != socketInCandidate) // Skip used socket
                                openSockets.Add((candidateRoom, s));
                        }
                        Bounds b = GetRoomBounds(candidateRoom);
                        debugBounds.Add(b);
                        goto NextRoom;
                    }
                }
                //Debug.Log("Failed to fit the room");
                Destroy(candidateObj);
            }
            NextRoom: ;
        }
        return TryPlaceNextLevelRoom(level);
    }
    bool OverlapsAny(Room newRoom)
    {
        Bounds newBounds = GetRoomBounds(newRoom);
        newBounds.Expand(-borderPadding);
        foreach (Bounds other in debugBounds)
        {
            //Bounds other = GetRoomBounds(room);

            if (newBounds.Intersects(other))
                return true;
        }

        return false;
    }
    Bounds GetRoomBounds(Room room)
    {
        Renderer[] renderers = room.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(room.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        return bounds;
    }
    Room PlaceRoom(RoomDataSO data, Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(data.prefab, position, rotation);
        Room room = obj.GetComponent<Room>();
        room.RoomType = data.roomType;
        //room.Initialize();
        return room;
    }
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
    List<(Room corridor, int depth)> GetCorridorsSortedByDepth() {
        HashSet<Room> visited = new();
        int maxLevel = 0;
        Queue<(Room room, int depth)> queue = new();
        List<(Room corridor, int depth)> corridors = new();

        queue.Enqueue((MapStartRoom, 0));
        visited.Add(MapStartRoom);

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (current.Level > maxLevel) maxLevel = current.Level;

            foreach (Room neighbor in current.ConnectedRooms)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, depth + 1));

                    if (neighbor.ConnectedRooms.Count < neighbor.Sockets.Count)
                    {
                        corridors.Add((neighbor, depth + 1));
                    }
                }
            }
        }

        // Sort descending by level then depth
        corridors.Sort((a, b) =>
        {
            int levelComparison = b.corridor.Level.CompareTo(a.corridor.Level);
            if (levelComparison != 0) return levelComparison;
            return b.depth.CompareTo(a.depth);
        });        
        return corridors;
    }
    
    
    Room TryPlaceNextLevelRoom(int level) {
    Debug.Log("level: " + level);
    if (level == numberOfLevels - 1) {
        Debug.Log("LAST");
    }
    
    List<(Room corridor, int depth)> candidateCorridors = GetCorridorsSortedByDepth();
    Debug.Log("candidateCorridors.Count: " + candidateCorridors.Count);

    RoomDataSO roomToPlaceSO = (level == numberOfLevels - 1) ? endLevelRoomSO : nextLevelRoomSO;

    foreach (var (targetCorridor, depth) in candidateCorridors)
    {
        foreach (Transform socket in targetCorridor.Sockets)
        {
            Vector3 fromSocketDir = socket.forward;
            Vector3 targetDir = -fromSocketDir;

            GameObject obj = Instantiate(roomToPlaceSO.prefab, targetCorridor.transform.position, Quaternion.identity);
            Room candidateRoom = obj.GetComponent<Room>();
            candidateRoom.Initialize();
            Debug.Log(candidateRoom.Sockets.Count);

            for (int i = 0; i < candidateRoom.Sockets.Count; i++)
            {
                Transform candidateSocket = candidateRoom.Sockets[i];
                Vector3 toSocketDir = candidateSocket.forward;

                if(Vector3.Angle(toSocketDir, targetDir) > 1f)
                {
                    Quaternion rotationFix = Quaternion.FromToRotation(toSocketDir, targetDir);
                    obj.transform.rotation = obj.transform.rotation * rotationFix;
                    if (obj.transform.up != Vector3.up)
                        obj.transform.Rotate(0f, 0f, 180f);
                }

                candidateRoom.Initialize();
                candidateSocket = candidateRoom.Sockets[i];

                float epsilon = 0.001f;
                Vector3 padding = new Vector3(
                    Mathf.Abs(fromSocketDir.x) < epsilon ? 0 : borderPadding,
                    0,
                    Mathf.Abs(fromSocketDir.z) < epsilon ? 0 : borderPadding
                );
                Vector3 offset = (socket.position - candidateSocket.position);
                obj.transform.position += offset;

                if (!OverlapsAny(candidateRoom))
                {
                    obj.transform.position -= padding * 3;
                    candidateRoom.RoomType = roomToPlaceSO.roomType;
                    candidateRoom.Initialize();

                    placedRooms[level].Add(candidateRoom);
                    targetCorridor.ConnectTo(candidateRoom);
                    candidateRoom.ConnectTo(targetCorridor);

                    debugBounds.Add(GetRoomBounds(candidateRoom));
                    return candidateRoom;
                }
            }

            Destroy(obj);
        }
    }

    Debug.LogWarning("Failed to place next level room on any corridor.");
    return null;
}

    
    
    
    
    
    //Debug
    void OnDrawGizmos()
    {
        if (debugBounds == null) return;

        Gizmos.color = Color.green;
        foreach (var b in debugBounds)
        {
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}
