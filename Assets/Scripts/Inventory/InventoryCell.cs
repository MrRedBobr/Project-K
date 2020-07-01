using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class InventoryCell : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
	public event Action Injecting;
	[SerializeField] private Transform _emptycellPrefub;

	[SerializeField] private TextMeshProUGUI _nameField;
	[SerializeField] private Image _iconField;

	private Transform _draggingParent;
	[SerializeField]private Transform _originalParent;
	[SerializeField]private RectTransform _rect;
	[SerializeField] private Canvas _canvas;

	private Transform _oldPlase;

	private void OnEnable()
	{
		_rect = GetComponent<RectTransform>();
	}

	public void Init(Transform draggingParent, Canvas canvas)
	{
		_draggingParent = draggingParent;
		_originalParent = transform.parent;
		_canvas = canvas;
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
		Debug.Log(index);
		_oldPlase.SetSiblingIndex(index);
	}
	public void OnDrag(PointerEventData eventData)
	{
		_rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
	}
	public void OnEndDrag(PointerEventData eventData)
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
		Debug.Log(closestIndex);
		closestIndex = oldIndex > closestIndex ? closestIndex : closestIndex+1;
		transform.SetParent(_originalParent);
		Destroy(_oldPlase.gameObject);
		/*Vector3 pos = _rect.position;
		pos.z = 0;
		_rect.position = pos;*/
		transform.SetSiblingIndex(closestIndex);
	}
}
