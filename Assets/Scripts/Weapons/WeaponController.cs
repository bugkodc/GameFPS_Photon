using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public enum WeaponType { pistol, rifle, machinegun, shotgun, sniper }
public interface IWeapon
{
    public float CalculateDamage(WeaponStats weaponSO, ZombieManager enemyManager, RaycastHit hit);
};
public class WeaponController : MonoBehaviour
{

    //Components
    public WeaponStats weaponSO;
    [SerializeField] GameObject cameraGO;
    [SerializeField] Camera mainCamera;
    float mainCameraFOV;
    [SerializeField] GameManager gameManager;
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem flashShot;
    MeshRenderer weaponMeshRenderer;
    AudioSource audioSource;

    //Canvas
    [SerializeField] Image aimCross;
    [SerializeField] Sprite normalCross, redCross;
    [SerializeField] CameraShake cameraShake;
    [SerializeField] TrailRenderer trail;
    [SerializeField] GameObject trailGO;
    [SerializeField] GameObject hitCross;
    [SerializeField] GameObject scopeOverlay;
    HitCross hitCrossScript;




    //Animations
    string animationAim = "isAiming";
    string animationReload = "isReloading";
    string animationShoot = "shoot";




    //Buttons
    string reloadButton = "Reload";
    string fire1 = "Fire1";
    string aimButton = "Aim";

    //Shooting
    float range = 1000;
    bool hittingSomething;
    bool isAiming;
    RaycastHit hit;
    Vector3 targetDirection;
    string enemyTag = "Enemy";
    public bool isScoping;

    //Reload system
    int currentAmmo, currentReserveAmmo;

    public bool isReloading;
    [SerializeField] TextMeshProUGUI currentAmmoText, reserveAmmoText;
    //ShootRatio
    float nextShootTime;

    public bool isAvailable;

    public int indexPosition;


    [SerializeField] PhotonView photonView;


    public LayerMask layerToIgnore;
    IWeapon IweaponInterface;



    public bool isMobile;
    public GameObject weapon;
    bool isAim = false;
    public void SetIndexPosition()
    {
        indexPosition = ((int)weaponSO.weaponType);
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = weaponSO.maxAmmo;
        currentReserveAmmo = weaponSO.maxReserveAmmo;
        hitCrossScript = hitCross.GetComponent<HitCross>();
        weaponMeshRenderer = GetComponent<MeshRenderer>();
        mainCamera = cameraGO.GetComponentInChildren<Camera>();
        mainCameraFOV = mainCamera.fieldOfView;
        IweaponInterface = GetComponent<IWeapon>();
    }
    private void FixedUpdate()
    {

        hittingSomething = Physics.Raycast(cameraGO.transform.position,
         cameraGO.transform.forward, out hit, range, ~layerToIgnore);
    }


    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }

        SetAmmoText();
        if (gameManager.CurrentLocalGameState == GameState.inGame)
        {

            animator.SetBool(animationShoot, false);

            if (hittingSomething)
            {

                ChangeCrossColor();
            }


            if ((currentAmmo <= 0 && !isReloading && currentReserveAmmo > 0) ||
            (currentAmmo < weaponSO.maxAmmo && !isReloading && Input.GetButtonDown(reloadButton) && isMobile == false))
            {
                StartCoroutine(Reload());
                return;
            }


            if (!isReloading && isMobile == false)
            {

                AimSystem();


                if (Input.GetButton(fire1) && Time.time >= nextShootTime && weaponSO.isAutomatic && currentAmmo > 0 && isMobile == false)
                {
                    Shoot();
                }
                if (Input.GetButtonDown(fire1) && Time.time >= nextShootTime && !weaponSO.isAutomatic && currentAmmo > 0 && isMobile == false)
                {
                    Shoot();
                }
            }

        }
    }

    public void ShootMobi()
    {
        if (isMobile && Time.time >= nextShootTime && weaponSO.isAutomatic && currentAmmo > 0)
        {
            Shoot();
        }
        if (isMobile && Time.time >= nextShootTime && !weaponSO.isAutomatic && currentAmmo > 0)
        {
            Shoot();
        }
    }
    public void ReloadMobi()
    {
        if (weapon.activeSelf == true)
        {
            if ((currentAmmo <= 0 && !isReloading && currentReserveAmmo > 0) ||
                (currentAmmo < weaponSO.maxAmmo && !isReloading && isMobile))
            {
                StartCoroutine(Reload());
                return;
            }
        }
    }
    void Shoot()
    {
        ZombieManager enemyManager = null;
        currentAmmo -= 1;


        float shootTime = Time.time;
        nextShootTime = shootTime + weaponSO.fireRate;


        cameraShake.StartCoroutine(cameraShake.Shake(0.1f, 0.2f));


        if (weaponSO.shotClip)
            audioSource.PlayOneShot(weaponSO.shotClip, 0.5f);


        flashShot.Play();


        TrailRenderer trailInstance = Instantiate(trail, trailGO.transform.position, Quaternion.identity);


        if (hittingSomething)
        {
            enemyManager = hit.transform.gameObject.GetComponent<ZombieManager>();
            StartCoroutine(MoveTrial(trailInstance, hit.point));
        }
        else

            StartCoroutine(MoveTrial(trailInstance, cameraGO.transform.forward * 100));

        animator.SetBool(animationShoot, true);


        if (enemyManager != null)
        {
            DamageTheEnemy(hit, enemyManager, false);


            if (hitCross.activeSelf)
                hitCrossScript.RestartDisableCall();
            else
                hitCross.SetActive(true);

            if (weaponSO.collateral)
                Collateral();
        }
    }
    void DamageTheEnemy(RaycastHit enemyHit, ZombieManager enemyManager, bool isCollateral)
    {

        if (enemyManager != null && enemyManager.isAlive)
        {

            float totalDamage = IweaponInterface.CalculateDamage(weaponSO, enemyManager, enemyHit);
            totalDamage *= isCollateral ? 0.5f : 1;


            PlayerManager localPlayerInstance = PlayerManager.LocalPlayerInstance;
            if (localPlayerInstance) localPlayerInstance.UpdatePoints(enemyHit.collider.gameObject.CompareTag("Headshot") ? 50 : 10);




            if (PhotonNetwork.InRoom && photonView.IsMine)
                enemyManager.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, totalDamage);
            else if (!PhotonNetwork.InRoom)
                enemyManager.TakeDamage(totalDamage);
        }
    }

    void Collateral()
    {
        RaycastHit collateralHit;
        Physics.Raycast(hit.point, cameraGO.transform.forward, out collateralHit, range, ~layerToIgnore);
        ZombieManager enemyHitted = collateralHit.transform.gameObject.GetComponent<ZombieManager>();
        if (enemyHitted != null)
            DamageTheEnemy(collateralHit, enemyHitted, true);
    }


    IEnumerator MoveTrial(TrailRenderer trailToMove, Vector3 destiny)
    {

        float remainingDistance = 0;


        float distanceToHit = Vector3.Distance(trailToMove.transform.position, destiny);

        remainingDistance = distanceToHit;


        while (remainingDistance > 0)
        {

            trailToMove.transform.position = Vector3.Lerp(trailToMove.transform.position,
            destiny, 1 - (remainingDistance / distanceToHit));


            remainingDistance -= weaponSO.bulletSpeed * Time.deltaTime;

            yield return null;
        }

        trailToMove.transform.position = destiny;

        Destroy(trailToMove.gameObject, trailToMove.time);

    }


    void ChangeCrossColor()
    {
        if (!isAiming)
        {

            aimCross.sprite = hit.transform.CompareTag(enemyTag) && hit.transform.gameObject.GetComponent<ZombieManager>().isAlive
            ? redCross : normalCross;


            aimCross.color = hit.transform.CompareTag(enemyTag) && hit.transform.gameObject.GetComponent<ZombieManager>().isAlive
            ? Color.white : Color.black;
        }
    }


    void AimSystem()
    {
        if (Input.GetButton(aimButton))
            SetAimMode(true);
        else
            SetAimMode(false);

    }
    public void AimSystemMobi()
    {
        if (weapon.activeSelf == true)
        {
            if (isAim)
            {
                SetAimMode(true);
                isAim = false;
            }
            else
            {
                SetAimMode(false);
                isAim = true;
            }
        }
    }

    public void SetAimMode(bool aimMode)
    {
        if (aimMode == true)
        {
            animator.SetBool(animationAim, true);
            isAiming = true;
            aimCross.enabled = false;
        }
        else
        {
            animator.SetBool(animationAim, false);
            if (weaponSO.weaponType == WeaponType.sniper)
            {
                StopScoping();
            }
            isAiming = false;
            aimCross.enabled = true;
        }
    }
    public void StopScoping()
    {
        isScoping = false;
        scopeOverlay.SetActive(false);
        weaponMeshRenderer.enabled = true;
        mainCamera.fieldOfView = mainCameraFOV;
    }

    IEnumerator Reload()
    {

        aimCross.enabled = false;

        if (weaponSO.weaponType == WeaponType.sniper)
        {
            StopScoping();
        }
        isReloading = true;
        animator.SetBool(animationReload, true);



        PlayRechargeSounds();

        if (weaponSO.weaponType != WeaponType.shotgun)
        {
            yield return new WaitForSeconds(weaponSO.reloadTime);
            EndReload();
        }
    }


    public void EndReload()
    {

        isReloading = false;

        aimCross.enabled = true;

        AimSystem();

        animator.SetBool(animationReload, false);


        int ammountToReload = weaponSO.maxAmmo - currentAmmo;


        if (currentReserveAmmo - ammountToReload >= 0)
        {
            currentReserveAmmo -= weaponSO.maxAmmo - currentAmmo;
            currentAmmo = weaponSO.maxAmmo;
        }

        else
        {
            currentAmmo = currentReserveAmmo;
            currentReserveAmmo = 0;
        }
    }

    public void CancelReload()
    {
        StopCoroutine("Reload");
        isReloading = false;

        aimCross.enabled = true;

        AimSystem();

        animator.SetBool(animationReload, false);

    }

    void SetAmmoText()
    {

        currentAmmoText.text = $"{currentAmmo} /";
        reserveAmmoText.text = currentReserveAmmo.ToString();
    }


    private void OnEnable()
    {
        animator.SetLayerWeight(weaponSO.animationLayerIndex, 1);
    }

    private void OnDisable()
    {
        animator.SetLayerWeight(weaponSO.animationLayerIndex, 0);
    }

    public void SetAmmoToMax()
    {
        currentReserveAmmo = weaponSO.maxReserveAmmo;
    }


    public void PlayRechargeSounds()
    {
        AudioClip[] rechargeClips;
        rechargeClips = weaponSO.rechargeClips;
        if (rechargeClips.Length > 0)
        {
            if (rechargeClips.Length == 1)
            {
                audioSource.PlayOneShot(rechargeClips[0], 0.5f);
            }
            else
            {
                StartCoroutine(Playsounds(rechargeClips, audioSource));
            }
        }
        else
        {
            return;
        }
    }

    public IEnumerator Playsounds(AudioClip[] audioClips, AudioSource audioSource)
    {

        if (weaponSO.weaponType == WeaponType.shotgun)
        {

            audioSource.PlayOneShot(audioClips[0], 0.5f);
            yield return new WaitForSeconds(audioClips[0].length);

            for (int x = 0; x < weaponSO.maxAmmo - currentAmmo; x++)
            {
                audioSource.PlayOneShot(audioClips[1], 0.5f);
                yield return new WaitForSeconds(audioClips[1].length);
            }
            EndReload();
        }

        else
        {

            for (int i = 0; i < audioClips.Length; i++)
            {
                audioSource.PlayOneShot(audioClips[i], 0.5f);
                yield return new WaitForSeconds(audioClips[i].length);
            }
        }
    }

}
