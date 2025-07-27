using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPreviewUI : MonoBehaviour
{
	public static CardPreviewUI Instance;
	[Header("referencias del Panel")]
	public GameObject panel;
	public Image cardImage;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI costText;
	public TextMeshProUGUI powerText;
	public TextMeshProUGUI descriptionText;
	public Button activateButton;
	
	// reference to the clicked card
	private CardInstance currentCard;
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		Instance = this;
		panel.SetActive(false);
	}
	
	public void Show(CardInstance card)
	{
		currentCard = card;
		
		nameText.text = card.data.cardName;
		costText.text = card.data.energyCost.ToString();
		powerText.text = card.currentPower.ToString();
		descriptionText.text = card.data.description;
		cardImage.sprite = card.data.artwork;
		
		// mostrar boton si tiene efecto activable
		activateButton.gameObject.SetActive(card.data.hasActivateEffect);
		
		panel.SetActive(true);
	}
	
	public void Hide()
	{
		panel.SetActive(false);
	}
	
	public void ActivateCardEffect()
	{
		if (currentCard != null)
		{
			currentCard.Activate(); // agregar funcion en card instance
			Hide();
		}
	}
}
