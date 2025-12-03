using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
	public TextMeshProUGUI energyText;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI powerText;
	public TextMeshProUGUI descriptionText;
	public Image backgroundImage;
	
	public void SetUp(CardData data)
	{
		energyText.text = data.energyCost.ToString();
		nameText.text = data.cardName;
		powerText.text = data.power.ToString();
		descriptionText.text = data.description;
		
		// Asignar artwork si existe
		if (data.artwork != null && backgroundImage != null)
		{
			backgroundImage.sprite = data.artwork;
			backgroundImage.preserveAspect = true;
		}
	}
}
