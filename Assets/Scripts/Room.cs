using System;
using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour {
    public List<Transform> Sockets = new List<Transform>();
    public List<Room> ConnectedRooms = new();
    public RoomType RoomType;
    public int Level { get; set; }


    public void Awake() {
        Initialize();
    }

    public void Initialize() 
    {
        Sockets.Clear();
        Transform socketParent = transform.Find("Sockets");
        if (socketParent == null) return;

        foreach (Transform child in socketParent)
        {
            if (child.name.StartsWith("Socket_")) Sockets.Add(child);
        }
    }

    public void ConnectTo(Room otherRoom)
    {
        if (!ConnectedRooms.Contains(otherRoom))
            ConnectedRooms.Add(otherRoom);
    }
    
    void OnDrawGizmosSelected()
    {
        if (Sockets == null) return;

        Gizmos.color = Color.cyan;
        foreach (Transform socket in Sockets)
        {
            if (socket == null) continue;

            Gizmos.DrawSphere(socket.position, 1.7f);

            // Draw a line indicating the socket forward dir
            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(socket.position, socket.position + socket.forward * 0.5f);
        }
    }
}