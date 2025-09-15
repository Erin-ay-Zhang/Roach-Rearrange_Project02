// Assets/Scripts/InsecticideItem.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InsecticideItem : MonoBehaviour
{
    void Awake()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null) pc.insecticideCount++;
            Destroy(gameObject);
        }
    }
}
