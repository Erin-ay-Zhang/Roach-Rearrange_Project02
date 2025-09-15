// Assets/Scripts/RoomSelectable.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomSelectable : MonoBehaviour
{
    private Room room;
    void Awake()
    {
        room = GetComponent<Room>();
        if (room == null) room = GetComponentInParent<Room>();
    }

    void OnMouseDown()
    {
        if (GridManager.Instance != null && room != null)
        {
            GridManager.Instance.OnRoomClicked(room);
        }
    }
}
