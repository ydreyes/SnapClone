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
		//activateButton.gameObject.SetActive(card.data.hasActivateEffect);
		activateButton.gameObject.SetActive(card.CanActivate());
		
		panel.SetActive(true);
	}
	
	public void Hide()
	{
		panel.SetActive(false);
	}
	
	public void ActivateCardEffect()
	{
		if (currentCard == null) return;
		currentCard.Activate();
		// tras activar, deshabilita el botón para que no se repita
		activateButton.gameObject.SetActive(currentCard.CanActivate());
		Hide(); // o déjalo abierto si prefieres
	}
}
