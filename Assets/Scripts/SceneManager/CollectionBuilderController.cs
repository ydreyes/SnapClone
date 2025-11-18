using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionBuilderController : MonoBehaviour
{
	[Header("Collection UI")]
	public Transform collectionContainer;
	public GameObject collectionItemPrefab;

	[Header("Deck UI")]
	public Transform deckContainer;
	public GameObject deckItemPrefab;
	public TextMeshProUGUI deckCountText;

	[Header("Misc UI")]
	public Button backButton;
	public Button confirmButton;
	public TextMeshProUGUI heroPointsText;
	public TextMeshProUGUI characterNameText;

	private List<CardData> playerCollection;
	private List<CardData> editingDeck;

	void Start()
	{
		var pp = PlayerProgress.Instance;

		heroPointsText.text = "Puntos: " + pp.heroPoints;

		characterNameText.text = GameSession.Instance.selectedCharacter.characterName;

		editingDeck = new List<CardData>(GameSession.Instance.deckBeingEdited.cards);

		LoadPlayerCollection();

		RenderCollection();
		RenderDeck();

		backButton.onClick.AddListener(() =>
		{
			if (editingDeck.Count == 12)
				UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		});

		confirmButton.onClick.AddListener(() =>
		{
			if (editingDeck.Count == 12)
			{
				SaveDeckChanges();
				UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
			}
		});
	}

	void LoadPlayerCollection()
	{
		var pp = PlayerProgress.Instance;
		playerCollection = pp.ownedCards
			.OrderBy(c => c.energyCost)
			.ThenBy(c => c.power)
			.ToList();
	}

	void RenderCollection()
	{
		foreach (Transform t in collectionContainer) Destroy(t.gameObject);

		foreach (var c in playerCollection)
		{
			var go = Instantiate(collectionItemPrefab, collectionContainer);
			var ui = go.GetComponent<CardCollectionItemUI>();
			ui.Setup(c, this);
		}
	}

	void RenderDeck()
	{
		foreach (Transform t in deckContainer) Destroy(t.gameObject);

		foreach (var c in editingDeck)
		{
			var go = Instantiate(deckItemPrefab, deckContainer);
			var ui = go.GetComponent<CardDeckItemUI>();
			ui.Setup(c, this);
		}

		deckCountText.text = $"{editingDeck.Count}/12";

		confirmButton.interactable = (editingDeck.Count == 12);
		backButton.interactable = (editingDeck.Count == 12);
	}

	public void AddCardToDeck(CardData c)
	{
		if (editingDeck.Count >= 12) return;
		editingDeck.Add(c);
		RenderDeck();
	}

	public void RemoveCardFromDeck(CardData c)
	{
		editingDeck.Remove(c);
		RenderDeck();
	}

	void SaveDeckChanges()
	{
		GameSession.Instance.deckBeingEdited.cards = new List<CardData>(editingDeck);
	}
}
