// Assets/Scripts/Cockroach.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Cockroach : MonoBehaviour
{
    [HideInInspector]
    public Room currentRoom;
    public float wanderSpeed = 1f;
    public float wanderRadius = 1f;

    private Vector3 targetPos;
    private CockroachManager manager;

    void Start()
    {
        manager = FindObjectOfType<CockroachManager>();
        if (manager != null) manager.RegisterRoach(this);
        PickNewTarget();
    }

    void Update()
    {
        // simple wandering inside current room (visual)
        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            PickNewTarget();
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPos, wanderSpeed * Time.deltaTime);
    }

    void PickNewTarget()
    {
        if (currentRoom != null)
            targetPos = currentRoom.GetRandomPointInside();
        else
            targetPos = transform.position;
    }
}
