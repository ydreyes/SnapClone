using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectController : MonoBehaviour
{
	[Header("UI")]
	public Button confirmButton;
	public TextMeshProUGUI characterName;
	public Transform deckListContainer;          // contenedor vertical
	public GameObject deckListItemPrefab;        // TMP Text simple

	private CharacterData current;

	void Start()
	{
		confirmButton.interactable = false;
		ClearDeckPreview();
	}

	public void OnPick(CharacterData data) // llámalo desde cada botón de personaje
	{
		current = data;
		GameSession.Instance.SelectCharacter(current);

		characterName.text = current.characterName;
		confirmButton.interactable = true;

		ShowDeckPreview(current.starterDeck);
	}

	public void OnConfirm()
	{
		GameSession.Instance.StartGame();
	}

	void ShowDeckPreview(DeckData deck)
	{
		ClearDeckPreview();
		if (deck == null) return;
		foreach (var card in deck.cards)
		{
			var go = Instantiate(deckListItemPrefab, deckListContainer);
			go.GetComponent<TextMeshProUGUI>().text = card ? card.cardName : "<Empty>";
		}
	}

	void ClearDeckPreview()
	{
		for (int i = deckListContainer.childCount - 1; i >= 0; i--)
			Destroy(deckListContainer.GetChild(i).gameObject);
		characterName.text = "";
	}
}
