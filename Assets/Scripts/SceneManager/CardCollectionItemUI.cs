using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardCollectionItemUI : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
	public Image art;
	public TextMeshProUGUI nameText;

	private float holdTime = 0.6f;
	private bool isHolding = false;
	private float timer = 0f;

	private CardData card;
	private CollectionBuilderController controller;

	public void Setup(CardData c, CollectionBuilderController ctrl)
	{
		card = c;
		controller = ctrl;

		art.sprite = c.artwork;
		nameText.text = c.cardName;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		isHolding = true;
		timer = 0f;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isHolding = false;
	}

	void Update()
	{
		if (!isHolding) return;

		timer += Time.deltaTime;

		if (timer >= holdTime)
		{
			isHolding = false;
			controller.AddCardToDeck(card);
		}
	}
}
