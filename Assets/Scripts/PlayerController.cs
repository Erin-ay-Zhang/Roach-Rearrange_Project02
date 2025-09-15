// Assets/Scripts/PlayerController.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Room currentRoom;
    public Room previousRoom;
    public int insecticideCount = 1;

    public float speed = 4f;

    private CharacterController cc;
    private CockroachManager cockroachManager;
    private GridManager gridManager;
    private GameManager gameManager;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        cockroachManager = FindObjectOfType<CockroachManager>();
        gridManager = GridManager.Instance;
        gameManager = FindObjectOfType<GameManager>();
        if (currentRoom != null)
        {
            transform.position = currentRoom.GetRandomPointInside() + Vector3.up * 0.5f;
        }
    }

    void Update()
    {
        // simple WASD movement
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (input.sqrMagnitude > 0.001f)
        {
            Vector3 move = input.normalized * speed * Time.deltaTime;
            cc.Move(move);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryUseInsecticide();
        }
    }

    // Called by DoorCollider when player goes through a door trigger
    public void EnterRoom(Room target)
    {
        if (target == null || target == currentRoom) return;
        previousRoom = currentRoom;
        currentRoom = target;
        // place player at target center (slightly up)
        transform.position = currentRoom.GetRandomPointInside() + Vector3.up * 0.5f;

        // notify cockroach manager
        if (cockroachManager != null) cockroachManager.OnPlayerEnteredRoom(currentRoom, previousRoom);
        // update UI if needed
        if (gameManager != null) gameManager.UpdateUI();
    }

    // Use insecticide only if current room has roaches AND roaches are trapped (no moves excluding previousRoom)
    public void TryUseInsecticide()
    {
        if (currentRoom == null) return;
        if (currentRoom.roachesInside.Count == 0)
        {
            Debug.Log("当前房间没有蟑螂");
            return;
        }
        if (insecticideCount <= 0)
        {
            Debug.Log("没有杀虫剂了");
            return;
        }

        // check for escape options (connected neighbors excluding previousRoom)
        List<Room> options = new List<Room>();
        for (int i = 0; i < 4; i++)
        {
            Side s = (Side)i;
            if (gridManager.IsConnected(currentRoom, s))
            {
                Room nb = gridManager.GetNeighbor(currentRoom, s);
                if (nb != previousRoom) options.Add(nb);
            }
        }

        bool trapped = options.Count == 0;
        if (trapped)
        {
            int killed = cockroachManager.KillAllInRoom(currentRoom);
            insecticideCount--;
            Debug.Log($"使用杀虫剂，消灭 {killed} 只蟑螂，剩余药剂 {insecticideCount}");
            if (gameManager != null) gameManager.OnKilledRoaches(killed);
        }
        else
        {
            Debug.Log("蟑螂有逃路，无法使用杀虫剂（按规则）");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // pick up insecticide item
        if (other.CompareTag("Insecticide"))
        {
            insecticideCount++;
            Destroy(other.gameObject);
            if (gameManager != null) gameManager.UpdateUI();
        }
    }
}
