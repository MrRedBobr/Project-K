using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public delegate void startDragingItem(int index);
public delegate void endDraging(int index, AssetItem item);
public class InventoryCell : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
	[Header("Actions and links on data with frefab")]
	[SerializeField] private AssetItem _item;
	public event Action Ejecting;
	public startDragingItem StartChangePosition;
	public endDraging PasteChangePosition;

	[Header("Parants and draging spaces")]
	[SerializeField] private Transform _emptycellPrefub;
	private Transform _draggingParent;
	[SerializeField]private Transform _originalParent;
	[SerializeField]private RectTransform _rect;
	[SerializeField] private Canvas _canvas;
	private Transform _oldPlase;

	[Header("UI property")]
	[SerializeField] private TextMeshProUGUI _nameField;
	[SerializeField] private Image _iconField;

	private void OnEnable()
	{
		_rect = GetComponent<RectTransform>();
	}

	public void Init(Transform draggingParent, Canvas canvas, AssetItem item)
	{
		_draggingParent = draggingParent;
		_originalParent = transform.parent;
		_canvas = canvas;
		_item = item;
	}

	public void Render(IItem item)
	{
		_nameField.text = item.Name;
		_iconField.sprite = item.UIIcon;
	}
	public void OnBeginDrag(PointerEventData eventData)
	{
		int index = transform.GetSiblingIndex();
		transform.SetParent(_draggingParent);
		_oldPlase = Instantiate(_emptycellPrefub, _originalParent);
		_oldPlase.SetSiblingIndex(index);
		Debug.Log("start pos " + index);

		StartChangePosition(index);
	}
	public void OnDrag(PointerEventData eventData)
	{
		_rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
	}
	public void OnEndDrag(PointerEventData eventData)
	{
		if (((RectTransform)_originalParent.transform.parent).rect.Contains(_rect.anchoredPosition))
		{
			Injecting();
		}
		else 
		{
			Ejecting?.Invoke();
			Destroy(_oldPlase.gameObject);
		}
		
	}

	private void Injecting()
	{
		int closestIndex = 0;
		int oldIndex = _oldPlase.GetSiblingIndex();
		for (int i = 0; i < _originalParent.transform.childCount; i++)
		{
			if (Vector3.Distance(transform.position, _originalParent.GetChild(i).position) <
				Vector3.Distance(transform.position, _originalParent.GetChild(closestIndex).position))
			{
				closestIndex = i;
			}
		}
		Debug.Log("end pos " + closestIndex);
		//вызвать событие вставки на нужное место
		PasteChangePosition(closestIndex, _item);
		//костыль для unity ui, который не хочет адекватно работать
		closestIndex = oldIndex > closestIndex ? closestIndex : closestIndex + 1;
		transform.SetParent(_originalParent);
		Destroy(_oldPlase.gameObject);
		transform.SetSiblingIndex(closestIndex);
	}
}
