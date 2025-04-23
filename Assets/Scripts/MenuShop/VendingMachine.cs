using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// VendingMachine: Quản lý giao diện cửa hàng, mua bán và tương tác với PlayerManager.
/// VendingMachine: Manages shop UI, item purchasing, and interaction with PlayerManager.
/// </summary>
public class VendingMachine : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopCanvas;         
    [SerializeField] private GameObject doText;             
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Game Management")]
    [SerializeField] private GameManager gameManager;       

    [Header("Shop Configuration")]
    public ShopSlot[] arrayItem;                            

    [Header("State & Runtime")]
    public bool isShopOpen = false;                         
    public PlayerManager _playerManager;                    
    public ShopSlot selectedShopSlot;                    
    private EventSystem eventSystem;                        

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
        _playerManager = playerManager;
        gameManager = playerManager.gameManager;

     
        foreach (var slot in arrayItem)
            slot.playerManager = _playerManager;

        shopCanvas.SetActive(true);
        eventSystem.SetSelectedGameObject(firstSelectedButton);
        gameManager.Shop();
        isShopOpen = true;
    }
    public void ExitShop()
    {
        shopCanvas.SetActive(false);
        gameManager.Resume();
        isShopOpen = false;
    }
    public void SelectItem(ShopSlot shopSlot)
    {
        selectedShopSlot = shopSlot;
    }

    public void BuyItem()
    {
        if (selectedShopSlot == null) return;

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
    private void BuyWeapon()
    {
        var ws = (WeaponStats)selectedShopSlot.itemSO;
        if (_playerManager.currentPoints >= ws.cost)
        {
            _playerManager.SetWeaponAvailable(ws.weaponType);
            _playerManager.UpdatePoints(-ws.cost);
            ExitShop();
        }
    }

    private void BuyHeal()
    {
        int cost = gameManager.currentRound * 250 + 500;
        if (_playerManager.currentPoints >= cost)
        {
            _playerManager.Heal(true);
            _playerManager.UpdatePoints(-cost);
            ExitShop();
        }
    }

    private void BuyAmmo()
    {
        int cost = gameManager.currentRound * 250 + 500;
        if (_playerManager.currentPoints >= cost)
        {
            _playerManager.BuyAmmo();
            _playerManager.UpdatePoints(-cost);
            ExitShop();
        }
    }
}
