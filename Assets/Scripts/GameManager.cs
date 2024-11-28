using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string playerPrefabName = "Player";
    [SerializeField] private Transform[] spawnPoints;

    private static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Already in room, spawning player...");
            SpawnPlayer();
        }
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Debug.Log("Attempting to spawn player...");
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log($"Connected and in room. Looking for prefab: {playerPrefabName}");
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            var player = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
            if (player != null)
            {
                Debug.Log("Player spawned successfully");
            }
            else
            {
                Debug.LogError("Failed to spawn player");
            }
        }
        else
        {
            Debug.LogError($"Cannot spawn player - Connected: {PhotonNetwork.IsConnected}, InRoom: {PhotonNetwork.InRoom}");
        }
    }
}