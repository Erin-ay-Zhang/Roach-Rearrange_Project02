// Assets/Scripts/GameManager.cs
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public CockroachManager cockroachManager;
    public PlayerController player;
    public Text insecticideText;
    public Text roachCountText;

    void Start()
    {
        if (cockroachManager == null) cockroachManager = FindObjectOfType<CockroachManager>();
        if (player == null) player = FindObjectOfType<PlayerController>();
        UpdateUI();
    }

    void Update()
    {
        // watch win
        if (cockroachManager != null && cockroachManager.allRoaches.Count == 0)
        {
            // win
            Debug.Log("胜利：所有蟑螂已被清除！");
        }
    }

    public void OnKilledRoaches(int killed)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (insecticideText != null && player != null) insecticideText.text = $"杀虫剂: {player.insecticideCount}";
        if (roachCountText != null && cockroachManager != null) roachCountText.text = $"蟑螂: {cockroachManager.allRoaches.Count}";
    }
}
