using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] private WeaponStats weaponSO;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private VendingMachine vendingMachine;
    [SerializeField] private TextMeshProUGUI costText;

    public Item itemSO;
    private Image image;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = itemSO.sprite;
        
        if (itemSO.itemType == ItemType.Weapon)
            if(costText && weaponSO) costText.text = weaponSO.cost.ToString();
        else
        {
            costText.enabled = false;
        }
    }
    public void SelectItem()
    {
        vendingMachine.SelectItem(this);
    }
}
