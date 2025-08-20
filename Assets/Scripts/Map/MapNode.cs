using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapNode : MonoBehaviour
{
	[Header("Setup")]
	public MapNodeType type;
	public int stepIndex; // 0..4 (5 pasos)
	public List<MapNode> connections = new(); // a qué nodos del siguiente paso está conectado

	[Header("UI")]
	public Image icon;
	public TextMeshProUGUI label;
	public Button button;

	[Header("State")]
	public MapNodeState state = MapNodeState.Locked;

	private void Awake()
	{
		if (!button) button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);
	}

	public void Init(MapNodeType t, int step, Sprite iconSprite, string labelText)
	{
		type = t;
		stepIndex = step;
		if (icon) icon.sprite = iconSprite;
		if (label) label.text = labelText;
		SetState(MapNodeState.Locked);
	}

	public void SetState(MapNodeState s)
	{
		state = s;
		// Visual rápido: disponible = full color, bloqueado = gris, cleared = semi
		var cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
		switch (state)
		{
		case MapNodeState.Locked:    cg.alpha = 0.5f; button.interactable = false; break;
		case MapNodeState.Available: cg.alpha = 1f;   button.interactable = true;  break;
		case MapNodeState.Cleared:   cg.alpha = 0.4f; button.interactable = false; break;
		}
	}

	void OnClick()
	{
		if (state != MapNodeState.Available) return;
		MapController.Instance.SelectNode(this);
	}
}
