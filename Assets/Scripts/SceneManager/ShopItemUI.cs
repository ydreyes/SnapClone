using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
	public Image cardArt;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI costText;
	public Button buyButton;

	private CardData card;
	private int cost;
	private ShopController controller;

	public void Setup(CardData card, int cost, ShopController controller)
	{
		this.card = card;
		this.cost = cost;
		this.controller = controller;

		nameText.text = card.cardName;
		costText.text = cost.ToString();

		if (cardArt && card.artwork)
			cardArt.sprite = card.artwork;

		buyButton.onClick.RemoveAllListeners();
		buyButton.onClick.AddListener(() => controller.TryBuy(card, cost, this));
	}

	public void SetBought()
	{
		buyButton.interactable = false;
		buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "COMPRADO";
	}
}
