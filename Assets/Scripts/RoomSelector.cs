using UnityEngine;

public class RoomSelector : MonoBehaviour
{
    private Room firstSelected;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ������
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
                        Debug.Log("ѡ�е�һ������: " + room.name);
                    }
                    else
                    {
                        Debug.Log("��������: " + firstSelected.name + " <-> " + room.name);
                        GridManager.Instance.SwapRooms(firstSelected, room);
                        firstSelected = null; // ����
                    }
                }
            }
        }
    }
}
