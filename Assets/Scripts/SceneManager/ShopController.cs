using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class ShopController : MonoBehaviour
{
	[Header("Config")]
	public List<CardData> shopPool;
	public int offersPerRefresh = 6;
	public int cardCost = 500;

	[Header("UI")]
	public Transform offersContainer;
	public GameObject shopItemPrefab;
	public TextMeshProUGUI heroPointsText;
	public Button backButton;
	
	void Start()
	{
		UpdateHeroPoints();

		GenerateOffers();

		if (backButton)
			backButton.onClick.AddListener(() =>
				SceneManager.LoadScene("MainMenu")
			);
	}

	void UpdateHeroPoints()
	{
		heroPointsText.text = 
			$"Puntos de Héroe: {PlayerProgress.Instance.heroPoints}";
	}

	public void GenerateOffers()
	{
		foreach (Transform child in offersContainer)
			Destroy(child.gameObject);

		var pp = PlayerProgress.Instance;

		List<CardData> available = shopPool.Where(c => !pp.ownedCards.Contains(c)).ToList();

		if (available.Count == 0)
		{
			Debug.Log("No hay cartas para ofrecer.");
			return;
		}

		int count = Mathf.Min(offersPerRefresh, available.Count);

		for (int i = 0; i < count; i++)
		{
			CardData card = available[Random.Range(0, available.Count)];

			var go = Instantiate(shopItemPrefab, offersContainer);
			var ui = go.GetComponent<ShopItemUI>();
			ui.Setup(card, cardCost, this);

			available.Remove(card);
		}
	}

	public void TryBuy(CardData card, int cost, ShopItemUI itemUI)
	{
		var pp = PlayerProgress.Instance;

		if (pp.heroPoints < cost)
		{
			Debug.Log("No tienes puntos suficientes");
			return;
		}

		pp.heroPoints -= cost;
		pp.AddCardToCollection(card);

		UpdateHeroPoints();

		itemUI.SetBought();

		Debug.Log("Carta comprada: " + card.cardName);
	}
}
