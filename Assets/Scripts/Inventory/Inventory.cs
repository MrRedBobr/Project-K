using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	[SerializeField] private PlayerInventory playerInventory;
	[SerializeField] private List<AssetItem> _Items;

	[Header("GameObjects in scene")]
	[SerializeField] private InventoryCell _inventoryCellTemplate;
	[SerializeField] private Transform _container;
	[SerializeField] private Transform _draggingParent;
	[SerializeField] private Canvas canva;
	public void OnEnable()
	{
		_Items = playerInventory.InventoryItems;
		Render(_Items);
	}

	public void Render(List<AssetItem> items)
	{
		foreach (Transform child in _container) Destroy(child.gameObject);
		items.ForEach(item =>
		{
			var cell = Instantiate(_inventoryCellTemplate, _container);
			cell.Init(_draggingParent, canva, item);
			cell.Render(item);

			cell.Ejecting += () => Destroy(cell.gameObject);
			cell.StartChangePosition = (index) => _Items.RemoveAt(index);
			cell.PasteChangePosition = (index, _item) => _Items.Insert(index, _item);
		});
	}
}
