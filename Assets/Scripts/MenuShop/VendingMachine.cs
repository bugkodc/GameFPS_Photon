using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
/// <summary>
/// Quản lý mở shop, chọn item, mua hàng và đồng bộ trạng thái với các client khác.
/// </summary>
public class VendingMachine : MonoBehaviourPunCallbacks
{
    ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject doText;
    [SerializeField] private GameObject firstSelectedButton;
    private EventSystem eventSystem;
    private string isShopOpenKey = "isShopOpen";

    public bool isShopOpen = false;
    public PlayerManager _playerManager;
    public ShopSlot selectedShopSlot;

    private void Start()
    {
        eventSystem = EventSystem.current;
    }
    private void OnTriggerEnter(Collider other)
    {
        doText.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        doText.SetActive(false);
    }
    public void OpenShop(PlayerManager playerManager)
    {
        if (hash.ContainsKey(isShopOpenKey))
        {
            hash[isShopOpenKey] = true;
        }
        else
        {
            hash.Add(isShopOpenKey, true);
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        gameManager = playerManager.gameObject.GetComponentInChildren<GameManager>();
        _playerManager = playerManager;

        Debug.Log("OpeningShop");
        shopCanvas.SetActive(true);
        eventSystem.SetSelectedGameObject(firstSelectedButton);

        gameManager.Shop();
        if (gameManager.isMobi)
        {
            _playerManager.canvasParrent.SetActive(false);
        }
    }

    public void ExitShop()
    {
        if (hash.ContainsKey(isShopOpenKey))
        {
            hash[isShopOpenKey] = false;
        }
        else
        {
            hash.Add(isShopOpenKey, false);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        Debug.Log("Close shop");

        shopCanvas.SetActive(false);
        gameManager.Resume();

        if (gameManager.isMobi)
        {
            _playerManager.canvasParrent.SetActive(true);
        }
    }

    public void SelectItem(ShopSlot shopSlot)
    {
        selectedShopSlot = shopSlot;
    }
    public void BuyItem()
    {
        if (selectedShopSlot != null)
        {
            switch (selectedShopSlot.itemSO.itemType)
            {
                case ItemType.Weapon:
                    BuyWeapon();
                    break;
                case ItemType.Ammo:
                    BuyAmmo();
                    break;
                case ItemType.Heal:
                    BuyHeal();
                    break;

            }
        }
    }
    public void BuyWeapon()
    {
        WeaponStats selectedWeaponSO = (WeaponStats)selectedShopSlot.itemSO;
        if(_playerManager && _playerManager.currentPoints >= selectedWeaponSO.cost)
        {
            _playerManager.SetWeaponAvailable(selectedWeaponSO.weaponType);
            _playerManager.UpdatePoints(-selectedWeaponSO.cost);
            ExitShop();
        }
    }
    public void BuyHeal()
    {
       
        if (_playerManager && _playerManager.currentPoints >= selectedShopSlot.weaponSO.cost)
        {
            _playerManager.Heal(true);
            _playerManager.UpdatePoints(-gameManager.currentRound * 250 + 500);
            ExitShop();
        }
    }
    public void BuyAmmo()
    {
       
        if (_playerManager && _playerManager.currentPoints >= selectedShopSlot.weaponSO.cost)
        {
            _playerManager.BuyAmmo();
            _playerManager.UpdatePoints(-gameManager.currentRound * 250 + 500);
            ExitShop();
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps[isShopOpenKey] != null)
        {
            isShopOpen = (bool)changedProps[isShopOpenKey];
            doText.GetComponent<Text>().text = isShopOpen ? "Shop in use" : "Press E";

            Debug.Log(" isShopOpen = " + isShopOpen);
        }
    }
}

