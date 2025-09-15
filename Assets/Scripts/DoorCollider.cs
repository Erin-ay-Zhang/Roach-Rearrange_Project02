// Assets/Scripts/DoorCollider.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorCollider : MonoBehaviour
{
    public Side side; // which side of the room this door belongs to. (assign in inspector)
    [Tooltip("Leave empty to auto-find parent Room")]
    public Room ownerRoom;

    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
        if (ownerRoom == null) ownerRoom = GetComponentInParent<Room>();
    }

    public Room GetLinkedRoom()
    {
        if (ownerRoom == null) return null;
        return GridManager.Instance.GetNeighbor(ownerRoom, side);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // When player enters the door trigger, attempt to move player to linked room
            Room linked = GetLinkedRoom();
            if (linked != null)
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.EnterRoom(linked);
                }
            }
        }
    }
}
