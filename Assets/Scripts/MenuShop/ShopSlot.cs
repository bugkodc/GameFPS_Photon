using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ShopSlot: Quản lý hiển thị và chọn mục trong cửa hàng.
/// ShopSlot: Manages displaying and selecting items in the shop.
/// </summary>
public class ShopSlot : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] public Item itemSO;                
    [SerializeField] public WeaponStats weaponSO;      

    [Header("References")]
    [SerializeField] public PlayerManager playerManager;
    [SerializeField] private VendingMachine vendingMachine;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI costText;   
    private Image image;                                 
    void Start()
    {
        image = GetComponent<Image>();
        if (itemSO?.sprite != null)
            image.sprite = itemSO.sprite;

        if (itemSO.itemType == ItemType.Weapon && costText != null && weaponSO != null)
            costText.text = weaponSO.cost.ToString();
    }

    
    private void Update()
    {
        if (itemSO.itemType != ItemType.Weapon && playerManager != null)
            CostText();
    }

    
    public void SelectItem()
    {
        vendingMachine.SelectItem(this);
    }


    public void CostText()
    {
        int cost = playerManager.gameManager.currentRound * 250 + 500;
        costText.text = cost.ToString();
    }
}
