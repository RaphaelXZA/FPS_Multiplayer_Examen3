using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.1f;
    private float nextFireTime;

    [Header("References")]
    [SerializeField] private Transform cameraRoot;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Camera mainCamera;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Follow = cameraRoot;
                vcam.Priority = 10;
            }
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        transform.rotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);

        Vector3 move = new Vector3(moveHorizontal, 0, moveVertical);
        move = transform.TransformDirection(move);

        controller.Move(move.normalized * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (firePoint != null)
        {
            GameObject bulletObj = PhotonNetwork.Instantiate(bulletPrefab.name,
                firePoint.position,
                firePoint.rotation);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                PlayerStats myStats = GetComponent<PlayerStats>();
                int materialIndex = myStats.GetMaterialIndex();
                bullet.Initialize(photonView, materialIndex);
            }
        }
    }

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(velocity);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}