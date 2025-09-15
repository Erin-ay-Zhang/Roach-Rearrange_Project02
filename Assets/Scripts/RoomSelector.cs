using UnityEngine;

public class RoomSelector : MonoBehaviour
{
    private Room firstSelected;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 鼠标左键
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Room room = hit.collider.GetComponent<Room>();
                if (room != null)
                {
                    if (firstSelected == null)
                    {
                        firstSelected = room;
                        Debug.Log("选中第一个房间: " + room.name);
                    }
                    else
                    {
                        Debug.Log("交换房间: " + firstSelected.name + " <-> " + room.name);
                        GridManager.Instance.SwapRooms(firstSelected, room);
                        firstSelected = null; // 重置
                    }
                }
            }
        }
    }
}
