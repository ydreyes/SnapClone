using UnityEngine;
using UnityEngine.EventSystems;

public class CardInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
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
	
	// referencias para on drag de cartas
	//private Canvas canvas;
	private CanvasGroup canvasGroup;
	private RectTransform rectTransform;
	private Transform originalParent;
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();
	}

	protected void Start()
	{
		currentPower = data.power;
		GetComponent<CardView>().SetUp(data);
	}
	
	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!isPlayerCard) 
			return;
		
		if (!GameManager.Instance.PlayerCanPlay(data))
			return;
			
		originalParent = transform.parent;
		canvasGroup.blocksRaycasts = false;
		transform.SetParent(originalParent.root); // para que se arrastre por encima
		
		GameManager.Instance.SetDragging(true); // en OnBeginDrag
	}
	
	public void OnDrag(PointerEventData eventData)
	{
		if (!isPlayerCard)
			return;
			
		Canvas canvas = GetComponentInParent<Canvas>();
		rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
	}
	
	public void OnEndDrag(PointerEventData eventData)
	{
		canvasGroup.blocksRaycasts = true;
		
		// regresa a la mano si no se suelta en zona
		if (transform.parent == originalParent.root)
		{
			transform.SetParent(originalParent);
			rectTransform.anchoredPosition = Vector2.zero;
		}
		
		GameManager.Instance.SetDragging(false); // en OnEndDrag
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
	
	public void OnPointerClick(PointerEventData eventData)
	{
		if (!isPlayerCard)
		{
			return;
		}
		
		if (!GameManager.Instance.IsDragging());
		{
			CardPreviewUI.Instance.Show(this);
		}
		
	}
	
	public void Activate()
	{
		if (data.onActivateEffect != null)
		{
			data.onActivateEffect.ApplyEffect(this, GameManager.Instance.GetZoneForCard(this));
		}
	}

}
