using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectController : MonoBehaviour
{
	[Header("UI refs")]
	public Button confirmButton;
	public TextMeshProUGUI characterName;
	public Image portraitPreview;
	public Transform deckListContainer;      // ScrollView/Viewport/Content
	public GameObject deckListItemPrefab;    // DeckListItem prefab
	public Button backButton;

	private CharacterData current;
	private CharacterButton currentButton;

	void Start()
	{
		confirmButton.interactable = false;
		ClearDeckPreview();
		if (backButton) backButton.onClick.AddListener(OnBack);
	}

	// Llamado por CharacterButton.OnClick()
	public void OnPick(CharacterData data, CharacterButton source)
	{
		current = data;

		// Resaltar botón
		if (currentButton) currentButton.SetHighlighted(false);
		currentButton = source;
		if (currentButton) currentButton.SetHighlighted(true);

		// UI principal
		characterName.text = current.characterName;
		if (portraitPreview) portraitPreview.sprite = current.portrait;

		confirmButton.interactable = true;

		ShowDeckPreview(current.starterDeck);
		// Guarda selección en la sesión si ya la tienes cargada
		if (GameSession.Instance) GameSession.Instance.SelectCharacter(current);
	}

	public void OnConfirm()
	{
		if (GameSession.Instance) GameSession.Instance.StartGame();
		// si no usas GameSession aún, aquí harías SceneManager.LoadScene("GameScene");
	}

	void OnBack()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
	}

	void ShowDeckPreview(DeckData deck)
	{
		ClearDeckPreview();
		if (deck == null) return;

		foreach (var card in deck.cards)
		{
			var go = Instantiate(deckListItemPrefab, deckListContainer);
			var txt = go.GetComponent<TextMeshProUGUI>();
			if (txt) txt.text = card ? $"{card.cardName}  (C:{card.energyCost} P:{card.power})" : "<Empty>";
		}
	}

	void ClearDeckPreview()
	{
		for (int i = deckListContainer.childCount - 1; i >= 0; i--)
			Destroy(deckListContainer.GetChild(i).gameObject);

		characterName.text = "";
		if (portraitPreview) portraitPreview.sprite = null;
	}
}
