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

            SetupPersonalUI();
        }

        SetupWorldUI();
        photonView.RPC("SyncPlayerName", RpcTarget.AllBuffered, playerName);
    }

    private void Start()
    {
        if (photonView.IsMine && worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(false);
        }
    }

    private void SetupWorldUI()
    {
        if (worldCanvas != null)
        {
            worldCanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            worldCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        if (healthBarBackground != null)
        {
            healthBarBackground.type = Image.Type.Filled;
            healthBarBackground.fillMethod = Image.FillMethod.Horizontal;
            healthBarBackground.fillAmount = 1f;
        }

        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.fillAmount = 1f;
        }

        UpdateWorldUI();
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
        if (!photonView.IsMine) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        photonView.RPC("SyncHealth", RpcTarget.AllBuffered, currentHealth);
    }

    public void AddScore(int points)
    {
        if (!photonView.IsMine) return;

        score += points;
        UpdateScore();
    }

    [PunRPC]
    private void SyncPlayerName(string name)
    {
        playerName = name;
        UpdateWorldUI();
    }

    [PunRPC]
    private void SyncHealth(float health)
    {
        currentHealth = health;
        UpdateWorldUI();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentHealth);
            stream.SendNext(score);
        }
        else
        {
            currentHealth = (float)stream.ReceiveNext();
            score = (int)stream.ReceiveNext();

            UpdateWorldUI();
            if (photonView.IsMine)
            {
                UpdatePersonalUI();
            }
        }
    }
}
