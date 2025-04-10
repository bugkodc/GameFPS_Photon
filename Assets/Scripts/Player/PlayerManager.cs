using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Photon.Pun;
using Unity.Mathematics;
using Photon.Voice.Unity;
using static UnityEditor.Recorder.OutputPath;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager LocalPlayerInstance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI namePlayer;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private CanvasGroup takeDamageCG;

    [Header("Health Settings")]
    public float currentHealth = 100;
    public float maximumHealth = 100;
    public bool isAlive;

    [Header("Points")]
    public int currentPoints;
    [SerializeField] private GameObject pointsPopup;
    [SerializeField] private GameObject pointsPopupStartPoint;

    [Header("References")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] public GameManager gameManager;
    [SerializeField] public PhotonView photonView;
    [SerializeField] private Recorder recorder;
    [SerializeField] public GameObject canvasParrent;
    [SerializeField] public GameObject bob;
    [SerializeField] public GameObject capsule;
    [SerializeField] public GameObject Root;
    [Header("Weapon System")]
    [SerializeField] private GameObject weaponHolder;
    private WeaponController currentWeapon;
    private List<int> weaponsAvailableIndexes = new List<int>();
    private int currentWeaponIndex;

    private VendingMachine vendingMachine;
    private bool isRecorder = false;

    [Header("Effect Settings")]
    [SerializeField] private float damagedBlinkTime = 0.5f;
    private void Awake()
    {
        if (photonView.IsMine && PhotonNetwork.InRoom)
        {
            LocalPlayerInstance = this;
        }
        else if (!PhotonNetwork.InRoom)
        {
            LocalPlayerInstance = this;
        }
    }
    void Start()
    {
        // Setup initial weapon (pistol)
        currentWeaponIndex = 0;
        currentWeapon = weaponHolder.transform.GetChild(currentWeaponIndex).GetComponent<WeaponController>();
        SetWeaponAvailable(WeaponType.pistol);

        // Health & Points
        currentHealth = maximumHealth;
        healthSlider.value = 1f;
        isAlive = true;
        currentPoints = 0;
        pointsText.text = currentPoints.ToString();
        if (PhotonNetwork.InRoom)
        {
            namePlayer.text = photonView.Owner.NickName;
        }
        recorder = GameObject.FindGameObjectWithTag("Recorder").GetComponent<Recorder>();
    }

    // Update is called once per frame
    void Update()
    {
        if (takeDamageCG.alpha > 0)
        {
            takeDamageCG.alpha -= Time.deltaTime / damagedBlinkTime;
        }

        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }

        if (gameManager.CurrentLocalGameState == GameState.inGame)
        {
            CheckMouseWheelInput();
        }

        //If we are in range of a vending machine check input to open or close it
        if (vendingMachine != null)
        {
            if (Input.GetKeyDown(KeyCode.E) && !vendingMachine.isShopOpen)
            {
                Debug.Log(vendingMachine.isShopOpen);
                vendingMachine.OpenShop(this);
            }
            if (gameManager.isMobi)
            {
                gameManager.keyE.SetActive(true);
            }

        }
        if (Input.GetKeyDown(KeyCode.T) && !gameManager.isMobi)
        {
            SetRecorder();
        }
    }
    public void SetRecorder()
    {
        recorder.RecordingEnabled = isRecorder;
        gameManager.recorder.SetActive(isRecorder);
        gameManager.muteRecorder.SetActive(!isRecorder);
        isRecorder = !isRecorder;
    }
    public void OpenShopMobi()
    {
        vendingMachine.OpenShop(this);
    }
    void UpdateHealth()
    {
        healthSlider.value = (float)currentHealth / (float)maximumHealth;
    }

    [PunRPC]
    public void TakeDamage(float damage)
    {

        cameraShake.StartCoroutine(cameraShake.Shake(0.3f, 0.4f));
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maximumHealth);
        takeDamageCG.alpha = 1;
        UpdateHealth();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        PlayerManager[] players = FindObjectsOfType<PlayerManager>();
        bool hasAlivePlayer = false;
        foreach (PlayerManager player in players)
        {
            if (player.isAlive)
            {
                hasAlivePlayer = true;
                break;
            }
        }
        if (!hasAlivePlayer)
        {
            if (PhotonNetwork.InRoom)
            {
                photonView.RPC("GameOverRPC", RpcTarget.All);
            }
            GameOverRPC();
        }
        OffComponent();

    }
    void OffComponent()
    {
        bob.SetActive(false);
        capsule.SetActive(false);
        Root.SetActive(false);
        gameObject.GetComponent<CharacterMovement>().enabled = false;
        ZombieController[] Zombies = FindAnyObjectByType<ZombieController>;
    }
    [PunRPC]
    void GameOverRPC()
    {
        gameManager.GameOver();
    }
    public void UpdatePoints(int pointsUpd)
    {
        currentPoints += pointsUpd;
        pointsText.text = currentPoints.ToString();
        GameObject popupPoints = Instantiate(pointsPopup, pointsPopupStartPoint.transform.position, pointsText.transform.rotation, pointsPopupStartPoint.transform);
        TextMeshPro tmproText = popupPoints.GetComponent<TextMeshPro>();
        if (tmproText) tmproText.SetText($"+ {pointsUpd.ToString()}");
        StartCoroutine(MoveAndDestroyPointsPopup(popupPoints));
    }
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        UpdateHealth();
    }
    public void Heal(bool max)
    {
        if (max)
        {
            currentHealth = maximumHealth;
        }
        UpdateHealth();
    }

    void CheckMouseWheelInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (currentWeaponIndex + 1 < weaponsAvailableIndexes.Count)
            {
                ChangeWeapon(weaponsAvailableIndexes[currentWeaponIndex + 1]);
            }
            else
                ChangeWeapon(weaponsAvailableIndexes.First());
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            if (currentWeaponIndex - 1 >= 0)
                ChangeWeapon(weaponsAvailableIndexes[currentWeaponIndex - 1]);
            else
                ChangeWeapon(weaponsAvailableIndexes.Last());

    }

    public void ChangeWeapon(int weaponsAvailableIndex)
    {
        Debug.Log(currentWeapon + " Current weapon on change weapon");
        if (currentWeapon.isReloading)
            currentWeapon.CancelReload();

        if (currentWeapon.isScoping)
        {
            currentWeapon.StopScoping();
            currentWeapon.SetAimMode(false);
        }
        foreach (WeaponController weapon in weaponHolder.GetComponentsInChildren<WeaponController>(true))
        {
            if (weaponsAvailableIndex == weapon.indexPosition && weapon.isAvailable)
            {
                weapon.gameObject.SetActive(true);
                currentWeapon = weapon;
                currentWeaponIndex = weaponsAvailableIndexes.IndexOf(weaponsAvailableIndex);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
        }

    }

    void AddWeaponIndexToAvailable(int indexPosition)
    {
        Debug.Log("Index position addweaponindextoavailable: " + indexPosition);
        if (!weaponsAvailableIndexes.Contains(indexPosition))
            weaponsAvailableIndexes.Add(indexPosition);
    }
    public void SetWeaponAvailable(WeaponType weaponTypeToSetAvailable)
    {

        foreach (WeaponController weapon in weaponHolder.GetComponentsInChildren<WeaponController>(true))
        {
            Debug.Log(weapon.gameObject.name);
            if (weapon.weaponSO.weaponType == weaponTypeToSetAvailable)
            {
                weapon.isAvailable = true;
                weapon.SetIndexPosition();
                AddWeaponIndexToAvailable(weapon.indexPosition);
                ChangeWeapon(weapon.indexPosition);
            }
        }
    }
    public void BuyAmmo()
    {
        foreach (WeaponController weapon in weaponHolder.GetComponentsInChildren<WeaponController>(true))
        {
            if (weapon.isAvailable)
            {
                weapon.SetAmmoToMax();
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "VendingMachine")
        {
            vendingMachine = other.gameObject.GetComponent<VendingMachine>();
            gameManager.vendingMachine = vendingMachine;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "VendingMachine")
        {
            vendingMachine = null;
            gameManager.vendingMachine = null;
            if (gameManager.isMobi)
            {
                gameManager.keyE.SetActive(false);
            }
        }
    }

    IEnumerator MoveAndDestroyPointsPopup(GameObject popup)
    {
        float timer = .3f;
        while (timer > 0)
        {
            popup.transform.position += Vector3.up * Time.deltaTime * .005f;
            timer -= Time.deltaTime;
            yield return null;
        }

        Destroy(popup);
    }
}
