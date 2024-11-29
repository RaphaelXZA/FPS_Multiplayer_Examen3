using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Player Info")]
    [SerializeField] private string playerName;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private int score;
    [SerializeField] private int pointsPerKill = 100;

    [Header("World UI")]
    [SerializeField] private GameObject worldCanvas;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image healthBarBackground;  
    [SerializeField] private Image healthBarFill;       

    [Header("Player UI")]
    [SerializeField] private TextMeshProUGUI personalNameText;
    [SerializeField] private Image personalHealthBarBackground;  
    [SerializeField] private Image personalHealthBarFill;       
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Visual")]
    [SerializeField] private MeshRenderer playerMeshRenderer;
    private int materialIndex;

    public float GetCurrentHealth() => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (photonView.IsMine)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Player #" + Random.Range(1000, 9999);
            }

            if (UIManager.Instance != null)
            {
                personalNameText = UIManager.Instance.personalNameText;
                personalHealthBarBackground = UIManager.Instance.personalHealthBarBackground;
                personalHealthBarFill = UIManager.Instance.personalHealthBarFill;
                scoreText = UIManager.Instance.scoreText;
            }
            else
            {
                Debug.LogError("¡UIManager not found!");
            }

            if (PlayerMaterialManager.Instance != null && playerMeshRenderer != null)
            {
                materialIndex = PlayerMaterialManager.Instance.GetUnusedMaterialIndex();
                photonView.RPC("SyncMaterial", RpcTarget.AllBuffered, materialIndex);
            }

            SetupPersonalUI();
        }

        UpdateWorldUI();
        photonView.RPC("SyncPlayerName", RpcTarget.AllBuffered, playerName);
    }

    private void Start()
    {
        if (photonView.IsMine && worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(false);
        }
    }

    private void SetupPersonalUI()
    {
        if (personalNameText != null) personalNameText.text = playerName;

        if (personalHealthBarBackground != null)
        {
            personalHealthBarBackground.type = Image.Type.Filled;
            personalHealthBarBackground.fillMethod = Image.FillMethod.Horizontal;
            personalHealthBarBackground.fillAmount = 1f;
        }

        if (personalHealthBarFill != null)
        {
            personalHealthBarFill.type = Image.Type.Filled;
            personalHealthBarFill.fillMethod = Image.FillMethod.Horizontal;
            personalHealthBarFill.fillAmount = 1f;
        }

        UpdateScore();
    }

    private void Update()
    {
        if (worldCanvas != null)
        {
            //Los valores de los otros jugadores miran a tu camara
            worldCanvas.transform.LookAt(Camera.main.transform);
            worldCanvas.transform.Rotate(0, 180, 0);
        }

        if (photonView.IsMine)
        {
            UpdatePersonalUI();
        }
    }

    private void UpdateWorldUI()
    {
        if (nameText != null) nameText.text = playerName;
        if (healthBarFill != null) healthBarFill.fillAmount = currentHealth / maxHealth;
    }

    private void UpdatePersonalUI()
    {
        if (personalHealthBarFill != null) personalHealthBarFill.fillAmount = currentHealth / maxHealth;
        UpdateScore();
    }

    private void UpdateScore()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    public void ChangeHealth(float amount)
    {
        if (!photonView.IsMine)
        {
            Debug.Log("Intento de cambiar vida en jugador que no es mío");
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"Vida modificada a: {currentHealth}");
        photonView.RPC("SyncHealth", RpcTarget.AllBuffered, currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log($"¡{playerName} murió!");
            Die();
        }
    }

    public void AddScore(int points)
    {
        if (!photonView.IsMine) return;

        score += points;
        UpdateScore();
    }

    public Material GetPlayerMaterial()
    {
        return playerMeshRenderer.material;
    }

    public void Die()
    {
        if (!photonView.IsMine) return;

        score = 0;
        UpdateScore();

        if (GameManager.instance != null)
        {
            Transform spawnPoint = GameManager.instance.GetRandomSpawnPoint();
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        currentHealth = maxHealth;
        photonView.RPC("SyncHealth", RpcTarget.AllBuffered, currentHealth);
    }

    [PunRPC]
    private void SyncPlayerName(string name)
    {
        playerName = name;
        UpdateWorldUI();
    }

    [PunRPC]
    private void SyncMaterial(int index)
    {
        materialIndex = index;
        if (playerMeshRenderer != null && PlayerMaterialManager.Instance != null)
        {
            playerMeshRenderer.material = PlayerMaterialManager.Instance.GetMaterialByIndex(index);
        }
    }

    [PunRPC]
    private void TakeDamage(float damage, int shooterViewID)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        Debug.Log($"Vida modificada a: {currentHealth}");

        UpdateWorldUI();
        if (photonView.IsMine)
        {
            UpdatePersonalUI();

            if (currentHealth <= 0)
            {
                PhotonView shooterView = PhotonView.Find(shooterViewID);
                if (shooterView != null)
                {
                    PlayerStats shooterStats = shooterView.GetComponent<PlayerStats>();
                    if (shooterStats != null)
                    {
                        shooterStats.AddScore(pointsPerKill);
                    }
                }

                Die();
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentHealth);
            stream.SendNext(score);
            stream.SendNext(materialIndex);
        }
        else
        {
            currentHealth = (float)stream.ReceiveNext();
            score = (int)stream.ReceiveNext();
            materialIndex = (int)stream.ReceiveNext();

            UpdateWorldUI();
            if (photonView.IsMine)
            {
                UpdatePersonalUI();
            }
        }
    }

    private void OnDestroy()
    {
        if (photonView.IsMine && PlayerMaterialManager.Instance != null)
        {
            PlayerMaterialManager.Instance.ReleaseMaterialIndex(materialIndex);
        }
    }
}
