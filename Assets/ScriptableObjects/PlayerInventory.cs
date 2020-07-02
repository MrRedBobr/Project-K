using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Player Inventory")]
public class PlayerInventory : ScriptableObject
{
    public List<AssetItem> InventoryItems;
}
