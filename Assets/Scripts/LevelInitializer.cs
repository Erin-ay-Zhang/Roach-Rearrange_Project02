using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInitializer : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject cockroachPrefab;
    public int initialRoachCount = 5;

    IEnumerator Start()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        // 等一帧，保证 GridManager 初始化完成
        yield return null;

        SpawnInitialRoaches();
    }

    void SpawnInitialRoaches()
    {
        for (int i = 0; i < initialRoachCount; i++)
        {
            int x = Random.Range(0, gridManager.width);
            int y = Random.Range(0, gridManager.height);
            Room room = gridManager.grid[x, y];

            if (room == null) continue;

            Transform spawn = room.transform.Find("RoachSpawnPoint");
            Vector3 pos = spawn != null ? spawn.position : room.transform.position;

            GameObject roachObj = Instantiate(cockroachPrefab, pos, Quaternion.identity);
            Cockroach roach = roachObj.GetComponent<Cockroach>();

            // ✅ 必须绑定房间，不然繁殖时报 null
            roach.currentRoom = room;
            room.AddRoach(roach);

            FindObjectOfType<CockroachManager>().RegisterRoach(roach);
        }
    }
}
