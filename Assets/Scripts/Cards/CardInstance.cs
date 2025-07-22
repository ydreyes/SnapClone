using UnityEngine;
using UnityEngine.EventSystems;

public class CardInstance : MonoBehaviour, IPointerClickHandler
{
	public CardData data;
	public bool isPlayerCard;
	public int currentPower;
	// bandera para efecto ongoing
	public bool effectApplied = false;
	// bandera para efecto onreveal
	public bool pendingBoostNextTurn = false;
	// Efecto condicional
	public bool canMoveOnce;

	protected void Start()
	{
		currentPower = data.power;
		GetComponent<CardView>().SetUp(data);
	}
	
	public void OnPointerClick(PointerEventData eventData)
	{
		if (!isPlayerCard) return;
		if (!GameManager.Instance.PlayerCanPlay(data)) return;

		GameManager.Instance.SelectCardToPlay(this);
	}

	public void PlayCard(Zone zone)
	{
		zone.AddCard(this);
		
		if (data.onRevealEffect) {
			data.onRevealEffect.ApplyEffect(this, zone);
		}
		
		if (data.ongoingEffect) {
			data.ongoingEffect.ApplyEffect(this, zone);
		}
		
		if (data.conditionalEffect) {
			data.conditionalEffect.ApplyEffect(this, zone);
		}
		
		zone.UpdatePowerDisplay();
	}
}
