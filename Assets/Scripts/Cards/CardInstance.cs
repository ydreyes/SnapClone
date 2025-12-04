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
	// bandera para efecto OnActivate
	public bool hasBeenActivated = false;
	// Efecto condicional
	public bool canMoveOnce;
	
	// referencias para on drag de cartas
	private CanvasGroup canvasGroup;
	private RectTransform rectTransform;
	private Transform originalParent;
	private Transform handParent;
	public bool wasPlayedThisDrop = false;
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();
	}
	
	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	protected void Start()
	{
		if (originalParent == null)
			originalParent = transform.parent;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!isPlayerCard) 
			return;
		
		if (!GameManager.Instance.PlayerCanPlay(data))
			return;

		originalParent = transform.parent;
		canvasGroup.blocksRaycasts = false;

		Transform dragLayer = GameObject.Find("DragLayer").transform;
		transform.SetParent(dragLayer, true);

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

		// Si la zona marcó que la carta fue jugada, no regresarla a la mano
		if (wasPlayedThisDrop)
		{
			wasPlayedThisDrop = false; // reset
			GameManager.Instance.SetDragging(false);
			return;
		}

		// Caso contrario: volver a la mano
		ReturnToHand();

		GameManager.Instance.SetDragging(false);
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
			return;

		if (GameManager.Instance.IsDragging())
			return;

		var zone = GameManager.Instance.GetZoneForCard(this);
		if (zone != null)
		{
			// 🔹 La carta está en una zona: la "desjugamos"

			// 1) Devolver energía al jugador sin pasar del máximo del turno
			int maxEnergy = Mathf.Min(
				GameManager.Instance.turnManager.currentTurn,6 // o el máximo que uses
			);

			GameManager.Instance.turnManager.playerEnergy =Mathf.Clamp(GameManager.Instance.turnManager.playerEnergy + data.energyCost, 0, maxEnergy);
			GameManager.Instance.UpdateEnergyDisplay();

			// 2) Volver a meter la carta en la lista de mano si no está
			var player = GameManager.Instance.player;

			if (!player.hand.Contains(data))
			{
				player.hand.Add(data);
			}

			// 3) Sacarla de la zona y devolver el prefab a la mano
			zone.RemoveCard(this);
			ReturnToHand();
			return;
		}

		// Si la carta no está en ninguna zona, solo mostramos el preview
		CardPreviewUI.Instance.Show(this);
	}
	
	// Helper: ¿se puede activar ahora?
	public bool CanActivate()
	{
		if (hasBeenActivated) return false;
		if (data.onActivateEffect == null) return false;
		if (!isPlayerCard) return false; // solo el jugador activa desde UI
		// Debe estar en una zona del tablero
		var zone = GameManager.Instance.GetZoneForCard(this);
		if (zone == null) return false;
		// Si pones costo de activación
		if (data.energyCost > GameManager.Instance.turnManager.playerEnergy) return false;

		return true;
	}
	
	public void Activate()
	{
		if (!CanActivate()) return;

		// Pagar costo opcional
		if (data.energyCost > 0)
		{
			GameManager.Instance.turnManager.playerEnergy -= data.energyCost;
			GameManager.Instance.UpdateEnergyDisplay();
		}

		// Ejecutar el efecto
		var zone = GameManager.Instance.GetZoneForCard(this);
		data.onActivateEffect.ApplyEffect(this, zone);

		hasBeenActivated = true;  // ← clave: solo una vez
		// Si el efecto cambia poder, refresca la UI de la zona
		zone.UpdatePowerDisplay();
	}
	
	public void Init(Transform hand)
	{
		handParent = hand;
	}
	
	public void ReturnToHand()
	{
		transform.SetParent(handParent, false);
		rectTransform.anchoredPosition = Vector2.zero;
	}
	
	public void UpdatePowerUI()
	{
		// Asegúrate que existe referencia al texto de poder
		var view = GetComponent<CardView>();
		if (view == null || view.powerText == null) return;

		// Obtener poder base y actual
		int basePower = data.power;
		int current = currentPower;

		// Cambiar texto
		view.powerText.text = current.ToString();

		// Cambiar color según modificación
		if (current > basePower)
			view.powerText.color = Color.green;
		else if (current < basePower)
			view.powerText.color = Color.red;
		else
			view.powerText.color = Color.white; // sin cambios
	}

}
