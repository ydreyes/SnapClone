using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterButton : MonoBehaviour
{
	public CharacterData data;
	public CharacterSelectController controller;
	public DeckCharacterSelectController deckController; 

	[Header("Optional visuals")]
	public Image portraitImage;
	public TextMeshProUGUI nameText;
	public Image highlightBorder; // si quieres resaltar el seleccionado

	void Start()
	{
		if (portraitImage && data) portraitImage.sprite = data.portrait;
		if (nameText && data) nameText.text = data.characterName;
	}

	// Asigna este método al OnClick del Button (sin parámetros)
	public void OnClick()
	{
		if (controller && data)
			controller.OnPick(data, this);
	}

	public void SetHighlighted(bool on)
	{
		if (highlightBorder) highlightBorder.enabled = on;
	}
	
	// NUEVO MÉTODO SETUP (para DeckCharacterSelectScene)
	public void Setup(CharacterData c, System.Action onSelect)
	{
		data = c;

		if (portraitImage) portraitImage.sprite = c.portrait;
		if (nameText) nameText.text = c.characterName;

		// asigna el botón
		var btn = GetComponent<Button>();
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(() => onSelect());
	}
}
