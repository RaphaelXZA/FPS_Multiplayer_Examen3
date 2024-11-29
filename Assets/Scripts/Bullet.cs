using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Bullet Properties")]
    [SerializeField] private float speed = 30f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int pointsPerKill = 100;

    private PhotonView shooterPhotonView; //Para saber qui�n dispar�
    private Material bulletMaterial;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(PhotonView shooter, Material material)
    {
        shooterPhotonView = shooter;
        bulletMaterial = material;
        GetComponent<MeshRenderer>().material = bulletMaterial;

        Destroy(gameObject, lifetime);
    }

    private void Start()
    {
        rb.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats != null && playerStats.photonView != shooterPhotonView)
        {
            playerStats.ChangeHealth(-damage);

            if (playerStats.GetCurrentHealth() <= 0)
            {
                PlayerStats shooterStats = shooterPhotonView.GetComponent<PlayerStats>();
                if (shooterStats != null)
                {
                    shooterStats.AddScore(pointsPerKill);
                }
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //Solo para que el Photon View lo acepte
    }
}