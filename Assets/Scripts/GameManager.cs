using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string playerPrefabName = "Player";
    [SerializeField] private Transform[] spawnPoints;

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room, spawning player...");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning("Player is not in room to spawn");
        }
    }
}