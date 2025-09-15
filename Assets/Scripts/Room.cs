using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Room : MonoBehaviour
{
    [Header("Grid")]
    public Vector2Int gridPos;

    [Header("Doors (Up, Right, Down, Left)")]
    public bool[] hasDoor = new bool[4] { false, false, false, false };

    [Header("Door GameObjects (child door objects, index matches Side)")]
    public GameObject[] doorObjects = new GameObject[4];

    [Header("Room contents")]
    public List<Cockroach> roachesInside = new List<Cockroach>();
    public bool hasInsecticidePickup = false;

    [Header("Visual / gameplay")]
    public Transform[] spawnPoints;

    private BoxCollider boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();

        // 确保 hasDoor 长度为 4
        if (hasDoor == null || hasDoor.Length != 4)
            hasDoor = new bool[4];

        // 确保 doorObjects 长度为 4
        if (doorObjects == null || doorObjects.Length != 4)
            doorObjects = new GameObject[4];
    }

    public bool HasDoor(Side side)
    {
        int idx = (int)side;
        if (hasDoor == null || idx < 0 || idx >= hasDoor.Length) return false;
        return hasDoor[idx];
    }

    public void AddRoach(Cockroach r)
    {
        if (!roachesInside.Contains(r)) roachesInside.Add(r);
        r.currentRoom = this;
        r.transform.position = GetRandomPointInside();
    }

    public void RemoveRoach(Cockroach r)
    {
        if (roachesInside.Contains(r)) roachesInside.Remove(r);
    }

    public Vector3 GetRandomPointInside()
    {
        Vector3 center = transform.position;
        Vector3 ext = boxCollider.size * 0.5f;
        Vector3 localRand = new Vector3(Random.Range(-ext.x * 0.4f, ext.x * 0.4f),
                                        0,
                                        Random.Range(-ext.z * 0.4f, ext.z * 0.4f));
        return center + localRand;
    }

    public void SetDoorActive(Side side, bool active)
    {
        int idx = (int)side;
        if (doorObjects != null && idx >= 0 && idx < doorObjects.Length && doorObjects[idx] != null)
            doorObjects[idx].SetActive(active);
    }

    void OnMouseDown()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.OnRoomClicked(this);
    }
}
