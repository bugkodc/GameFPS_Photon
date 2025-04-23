using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// PlayerManager: Quản lý sức khỏe, điểm, vũ khí và các tương tác của người chơi.
/// PlayerManager: Manages player health, points, weapons, and interactions.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [Header("Singleton")]
    public static PlayerManager LocalPlayerInstance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI namePlayer;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private CanvasGroup takeDamageCG;

    [Header("Health Settings")]
    public float currentHealth = 100f;
    public float maximumHealth = 100f;
    public bool isAlive;

    [Header("Points")]
    public int currentPoints;
    [SerializeField] private GameObject pointsPopup;
    [SerializeField] private GameObject pointsPopupStartPoint;

    [Header("References")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] public GameManager gameManager;
    [SerializeField] public GameObject canvasParent;
    [SerializeField] public GameObject bob;
    [SerializeField] public GameObject capsule;
    [SerializeField] public GameObject Root;

    [Header("Weapon System")]
    [SerializeField] private GameObject weaponHolder;
    private WeaponController currentWeapon;
    private List<int> weaponsAvailableIndexes = new List<int>();
    private int currentWeaponIndex;

    [Header("Effect Settings")]
    [SerializeField] private float damagedBlinkTime = 0.5f;

    private VendingMachine vendingMachine;
    private bool isRecorder = false;

    void Start()
    {
        currentWeaponIndex = 0;
        currentWeapon = weaponHolder.transform.GetChild(currentWeaponIndex).GetComponent<WeaponController>();
        SetWeaponAvailable(WeaponType.pistol);
        currentHealth = maximumHealth;
        healthSlider.value = 1f;
        isAlive = true;
        currentPoints = 0;
        pointsText.text = currentPoints.ToString();
    }
    void Update()
    {
        if (takeDamageCG.alpha > 0f)
            takeDamageCG.alpha -= Time.deltaTime / damagedBlinkTime;

        if (gameManager.CurrentLocalGameState == GameState.inGame)
            CheckMouseWheelInput();

        if (vendingMachine != null && Input.GetKeyDown(KeyCode.E) && !vendingMachine.isShopOpen)
            vendingMachine.OpenShop(this);

        if (Input.GetKeyDown(KeyCode.T))
            SetRecorder();
    }

    public void SetRecorder()
    {
        gameManager.recorder.SetActive(isRecorder);
        gameManager.muteRecorder.SetActive(!isRecorder);
        isRecorder = !isRecorder;
    }

    void UpdateHealth()
    {
        healthSlider.value = currentHealth / maximumHealth;
    }

    public void TakeDamage(float damage)
    {
        cameraShake.StartCoroutine(cameraShake.Shake(0.3f, 0.4f));
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maximumHealth);
        takeDamageCG.alpha = 1f;
        UpdateHealth();
        if (currentHealth <= 0f) Die();
    }


    void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        var players = FindObjectsOfType<PlayerManager>();
        bool anyAlive = players.Any(p => p.isAlive);

        if (!anyAlive)
            foreach (var p in players)
                p.GameOverRPC();

        OffComponent();
    }

    void OffComponent()
    {
        bob.SetActive(false);
        capsule.SetActive(false);
        Root.SetActive(false);
        GetComponent<CharacterMovement>().enabled = false;
    }
    public void GameOverRPC()
    {
        gameManager.GameOver();
    }

    public void UpdatePoints(int pointsUpd)
    {
        currentPoints += pointsUpd;
        pointsText.text = currentPoints.ToString();

        var popup = Instantiate(pointsPopup, pointsPopupStartPoint.transform.position, pointsText.transform.rotation, pointsPopupStartPoint.transform);
        var tm = popup.GetComponent<TextMeshPro>();
        if (tm) tm.SetText($"+{pointsUpd}");
        StartCoroutine(MoveAndDestroyPointsPopup(popup));
    }

    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0f, maximumHealth);
        UpdateHealth();
    }

    public void Heal(bool max)
    {
        if (max) currentHealth = maximumHealth;
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
            {
                ChangeWeapon(weaponsAvailableIndexes[currentWeaponIndex - 1]);
            }
            else
            {
                ChangeWeapon(weaponsAvailableIndexes.Last());
            }
    }

    public void ChangeWeapon(int weaponIndex)
    {
        if (currentWeapon.isReloading) currentWeapon.CancelReload();
        if (currentWeapon.isScoping)
        {
            currentWeapon.StopScoping();
            currentWeapon.SetAimMode(false);
        }

        var weapons = weaponHolder.GetComponentsInChildren<WeaponController>(true);
        for (int i = 0; i < weapons.Length; i++)
        {
            var w = weapons[i];
            bool match = w.indexPosition == weaponIndex && w.isAvailable;
            w.gameObject.SetActive(match);
            if (match)
            {
                currentWeapon = w;
                currentWeaponIndex = weaponsAvailableIndexes.IndexOf(weaponIndex);
            }
        }
    }

    void AddWeaponIndexToAvailable(int indexPosition)
    {
        if (!weaponsAvailableIndexes.Contains(indexPosition))
            weaponsAvailableIndexes.Add(indexPosition);
    }

    public void SetWeaponAvailable(WeaponType type)
    {
        foreach (var w in weaponHolder.GetComponentsInChildren<WeaponController>(true))
        {
            if (w.weaponSO.weaponType == type)
            {
                w.isAvailable = true;
                w.SetIndexPosition();
                AddWeaponIndexToAvailable(w.indexPosition);
                ChangeWeapon(w.indexPosition);
            }
        }
    }

    public void BuyAmmo()
    {
        foreach (var w in weaponHolder.GetComponentsInChildren<WeaponController>(true))
            if (w.isAvailable)
                w.SetAmmoToMax();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("VendingMachine"))
        {
            vendingMachine = other.GetComponent<VendingMachine>();
            gameManager.vendingMachine = vendingMachine;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("VendingMachine"))
        {
            vendingMachine = null;
            gameManager.vendingMachine = null;
        }
    }

    IEnumerator MoveAndDestroyPointsPopup(GameObject popup)
    {
        float timer = 0.3f;
        while (timer > 0f)
        {
            popup.transform.position += Vector3.up * Time.deltaTime * 0.005f;
            timer -= Time.deltaTime;
            yield return null;
        }
        Destroy(popup);
    }
}
