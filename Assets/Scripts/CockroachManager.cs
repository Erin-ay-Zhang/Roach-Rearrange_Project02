using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CockroachManager : MonoBehaviour
{
    public GameObject cockroachPrefab; // prefab with Cockroach component
    public float breedInterval = 5f;

    [HideInInspector]
    public List<Cockroach> allRoaches = new List<Cockroach>();

    public GridManager gridManager;

    void Start()
    {
        if (gridManager == null) gridManager = GridManager.Instance;
        StartCoroutine(BreedLoop());
    }

    IEnumerator BreedLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(breedInterval);
            BreedAndMove();
        }
    }

    // add to registry
    public void RegisterRoach(Cockroach r)
    {
        if (r != null && !allRoaches.Contains(r))
            allRoaches.Add(r);
    }

    // Breed: make clones (doubling count), then move ALL roaches randomly (stay or go to connected neighbor)
    void BreedAndMove()
    {
        // 1) clone current snapshot
        List<Cockroach> snapshot = new List<Cockroach>(allRoaches);
        List<Cockroach> clones = new List<Cockroach>();

        foreach (var r in snapshot)
        {
            if (cockroachPrefab == null) break;
            if (r == null || r.currentRoom == null) continue; // ✅ 防御

            GameObject go = Instantiate(cockroachPrefab, r.currentRoom.GetRandomPointInside(), Quaternion.identity);
            Cockroach cr = go.GetComponent<Cockroach>();
            if (cr != null)
            {
                cr.currentRoom = r.currentRoom;
                cr.transform.position = r.currentRoom.GetRandomPointInside();
                r.currentRoom.AddRoach(cr);
                RegisterRoach(cr);
                clones.Add(cr);
            }
        }

        // 2) move ALL roaches (including clones) randomly (stay or to any connected neighbor)
        List<Cockroach> allSnapshot = new List<Cockroach>(allRoaches);
        foreach (var r in allSnapshot)
        {
            if (r == null || r.currentRoom == null) continue; // ✅ 防御
            Room current = r.currentRoom;

            List<Room> options = new List<Room> { current }; // include stay
            for (int i = 0; i < 4; i++)
            {
                Side s = (Side)i;
                if (gridManager != null && gridManager.IsConnected(current, s))
                {
                    Room nb = gridManager.GetNeighbor(current, s);
                    if (nb != null) options.Add(nb);
                }
            }

            Room choose = options[Random.Range(0, options.Count)];
            if (choose != current)
            {
                MoveRoachToRoom(r, choose);
            }
        }
    }

    // Move single roach (update lists + position)
    public void MoveRoachToRoom(Cockroach r, Room to)
    {
        if (r == null || to == null) return;
        if (r.currentRoom != null) r.currentRoom.RemoveRoach(r);
        to.AddRoach(r);
        r.currentRoom = to; // ✅ 更新 currentRoom
        r.transform.position = to.GetRandomPointInside();
    }

    // Called when player enters a room -> roaches inside should flee
    public void OnPlayerEnteredRoom(Room entered, Room playerPrevRoom)
    {
        if (entered == null) return;

        List<Cockroach> snapshot = new List<Cockroach>(entered.roachesInside);
        foreach (var r in snapshot)
        {
            if (r == null) continue;
            List<Room> options = new List<Room>();
            for (int i = 0; i < 4; i++)
            {
                Side s = (Side)i;
                if (gridManager != null && gridManager.IsConnected(entered, s))
                {
                    Room nb = gridManager.GetNeighbor(entered, s);
                    if (nb != null && nb != playerPrevRoom) options.Add(nb);
                }
            }

            if (options.Count == 0)
            {
                // trapped: stay
                continue;
            }
            else
            {
                Room choose = options[Random.Range(0, options.Count)];
                MoveRoachToRoom(r, choose);
            }
        }
    }

    // Kill all roaches in room (used by insecticide). Returns number killed.
    public int KillAllInRoom(Room room)
    {
        if (room == null) return 0;

        int killed = room.roachesInside.Count;
        foreach (var r in new List<Cockroach>(room.roachesInside))
        {
            allRoaches.Remove(r);
            if (r != null) Destroy(r.gameObject);
        }
        room.roachesInside.Clear();
        return killed;
    }
}
