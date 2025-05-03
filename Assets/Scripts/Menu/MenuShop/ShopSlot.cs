using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] public WeaponStats weaponSO;
    [SerializeField] public PlayerManager playerManager;
    [SerializeField] private VendingMachine vendingMachine;
    [SerializeField] private TextMeshProUGUI costText;

    private Image image;

    public Item itemSO;
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = itemSO.sprite;
        
        if (itemSO.itemType == ItemType.Weapon)
            if(costText && weaponSO) costText.text = weaponSO.cost.ToString();
        
    }
    private void Update()
    {
       if (itemSO.itemType != ItemType.Weapon && playerManager != null)
        {
            CostText();
        }         
    }
    public void SelectItem()
    {
        vendingMachine.SelectItem(this);
    }
   public void CostText()
    {
        costText.text = (playerManager.gameManager.currentRound * 250 + 500).ToString();   
    }
}
