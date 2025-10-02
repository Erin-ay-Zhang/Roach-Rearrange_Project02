using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid settings")]
    public int width = 3;
    public int height = 3;
    public float cellSize = 6f;

    [Header("Auto populate from Room objects in scene?")]
    public bool autoPopulateFromChildren = true;

    public Room[,] grid;

    private Room selectedA = null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        grid = new Room[width, height];
        if (autoPopulateFromChildren) InitializeFromChildren();
    }

    public void InitializeFromChildren()
    {
        Room[] allRooms = FindObjectsOfType<Room>();
        foreach (Room r in allRooms)
        {
            if (r.hasDoor.Length != 4)
            {
                r.hasDoor = new bool[4]; // 防止 IndexOutOfRange
            }

            if (r.gridPos.x >= 0 && r.gridPos.x < width &&
                r.gridPos.y >= 0 && r.gridPos.y < height)
            {
                grid[r.gridPos.x, r.gridPos.y] = r;
                r.transform.position = GridToWorld(r.gridPos);
            }
            else
            {
                Debug.LogWarning($"Room {r.name} gridPos {r.gridPos} out of bounds");
            }
        }

        // 更新所有门
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] != null)
                    UpdateDoorsForRoom(grid[x, y]);
    }

    public Vector3 GridToWorld(Vector2Int coord)
    {
        return new Vector3(coord.x * cellSize, 0f, coord.y * cellSize);
    }

    public Room GetNeighbor(Room room, Side side)
    {
        Vector2Int n = room.gridPos;
        switch (side)
        {
            case Side.Up: n += Vector2Int.up; break;
            case Side.Down: n += Vector2Int.down; break;
            case Side.Left: n += Vector2Int.left; break;
            case Side.Right: n += Vector2Int.right; break;
        }

        if (n.x < 0 || n.x >= width || n.y < 0 || n.y >= height) return null;
        return grid[n.x, n.y];
    }

    public bool IsConnected(Room a, Side s)
    {
        Room neighbor = GetNeighbor(a, s);
        if (neighbor == null) return false;
        Side opp = (Side)(((int)s + 2) % 4);
        return a.HasDoor(s) && neighbor.HasDoor(opp);
    }

    private void MoveEntitiesWithRoom(Room room)
    {
        if (room == null) return;

        // 移动 Cockroach
        Cockroach[] roaches = room.GetComponentsInChildren<Cockroach>(true);
        foreach (var r in roaches)
        {
            r.currentRoom = room;
            r.transform.SetParent(room.transform, true);
            r.SendMessage("PickNewTarget", SendMessageOptions.DontRequireReceiver);
        }

        // 移动 Player
        PlayerController player = room.GetComponentInChildren<PlayerController>(true);
        if (player != null)
        {
            player.currentRoom = room;
            player.transform.SetParent(room.transform, true);
            player.transform.position = room.GetRandomPointInside() + Vector3.up * 0.5f;
        }
    }


    public void SwapRooms(Room a, Room b)
    {
        if (a == null || b == null) return;

        Vector2Int pa = a.gridPos;
        Vector2Int pb = b.gridPos;

        // 交换 grid 数组
        grid[pa.x, pa.y] = b;
        grid[pb.x, pb.y] = a;

        // 交换 gridPos
        a.gridPos = pb;
        b.gridPos = pa;

        // 移动父物体位置
        Vector3 tempPos = a.transform.position;
        a.transform.position = b.transform.position;
        b.transform.position = tempPos;

        // === 🟢 移动房间里的蟑螂 ===
        foreach (var roach in new List<Cockroach>(a.roachesInside))
        {
            roach.currentRoom = a;
            roach.transform.position = a.GetRandomPointInside();
        }
        foreach (var roach in new List<Cockroach>(b.roachesInside))
        {
            roach.currentRoom = b;
            roach.transform.position = b.GetRandomPointInside();
        }

        // === 🟢 如果玩家在这两个房间里，更新位置和 currentRoom ===
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            if (player.currentRoom == a)
            {
                player.transform.position = a.GetRandomPointInside() + Vector3.up * 0.5f;
                player.currentRoom = a;
            }
            else if (player.currentRoom == b)
            {
                player.transform.position = b.GetRandomPointInside() + Vector3.up * 0.5f;
                player.currentRoom = b;
            }
        }

        // 更新门逻辑
        UpdateAdjacencyAround(pa);
        UpdateAdjacencyAround(pb);

        //新增：交换后让里面的实体跟着房间
        MoveEntitiesWithRoom(a);
        MoveEntitiesWithRoom(b);


        Debug.Log($"Swapped rooms: {a.name} <-> {b.name}");
    }

    public void UpdateAdjacencyAround(Vector2Int coord)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        if (IsValidCoord(coord) && grid[coord.x, coord.y] != null)
            UpdateDoorsForRoom(grid[coord.x, coord.y]);

        foreach (var d in dirs)
        {
            Vector2Int n = coord + d;
            if (IsValidCoord(n) && grid[n.x, n.y] != null)
                UpdateDoorsForRoom(grid[n.x, n.y]);
        }
    }

    private bool IsValidCoord(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;
    }

    public void UpdateDoorsForRoom(Room r)
    {
        if (r == null) return;
        for (int i = 0; i < 4; i++)
        {
            if (r.hasDoor.Length != 4) continue;
            Side s = (Side)i;
            Room neighbor = GetNeighbor(r, s);
            bool active = neighbor != null && r.HasDoor(s) && neighbor.HasDoor((Side)(((int)s + 2) % 4));
            r.SetDoorActive(s, active);
        }
    }

    public void OnRoomClicked(Room r)
    {
        if (selectedA == null)
        {
            selectedA = r;
            HighlightRoom(r, true);
        }
        else if (selectedA == r)
        {
            HighlightRoom(selectedA, false);
            selectedA = null;
        }
        else
        {
            //SwapRooms(selectedA, r);
            HighlightRoom(selectedA, false);
            selectedA = null;
        }
    }

    void HighlightRoom(Room r, bool on)
    {
        Renderer[] rends = r.GetComponentsInChildren<Renderer>();
        foreach (var rend in rends)
            rend.material.color = on ? Color.yellow : Color.white;
    }
}
