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
	
	// --- Move Once ---
	public bool canMoveOnce;
	public bool hasMovedOnce = false;
	public int turnPlayed = -1;
	private Zone originalZone;
	
	// referencias para on drag de cartas
	private CanvasGroup canvasGroup;
	private RectTransform rectTransform;
	private Transform originalParent;
	private Transform handParent;
	public bool wasPlayedThisDrop = false;
	
	[Header("Inmunidades")]
	public bool cantBeDestroyed = false;
	public bool cantBeMoved = false;
	public bool cantHavePowerReduced = false;
	
	public int permanentPowerBonus = 0;
	public bool isReplayingOnReveal = false;
	
	public bool gainPowerAfterYouPlay = false;
	public int gainPowerAmount = 1;

	// para excluirse a sí misma si fue jugada este turno
	public int playedTurn = -1;
	
	// Cuando una carta se mueve AQUÍ, esta gana poder
	[Header("Gain powers when move Here")]
	public bool gainsPowerWhenCardMovesHere = false;
	public int gainsPowerWhenCardMovesHereAmount = 2;
	
	// When this moves, add a copy of this card to the old location
	public bool spawnCopyOnMove = false;
	
	// flag para el efecto de profesor X
	public bool isRevealed = false;

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
	
	private bool IsOnBoard()
	{
		return GameManager.Instance.GetZoneForCard(this) != null;
	}
	
	public bool CanMoveNow()
	{
		if (!isPlayerCard) return false;
		if (cantBeMoved) return false;
		if (!canMoveOnce) return false;
		if (hasMovedOnce) return false;

		var zone = GameManager.Instance.GetZoneForCard(this);
		if (zone == null) return false;

		// Solo a partir del turno siguiente (turnPlayed + 1)
		return GameManager.Instance.turnManager.currentTurn >= turnPlayed + 1;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!isPlayerCard) 
			return;
			
		bool onBoard = IsOnBoard();
		
		if (!onBoard)
		{
			if (!GameManager.Instance.PlayerCanPlay(data))
				return;
		}
		else
		{
			if (!CanMoveNow())
				return;

			originalZone = GameManager.Instance.GetZoneForCard(this);
		}

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
		
		// si estaba en el tablero, vuelve a su lugar original
		if (originalZone != null)
		{
			transform.SetParent(originalParent, false);
			rectTransform.anchoredPosition = Vector2.zero;
			originalZone = null;
		}
		else
		{
			// Caso contrario: volver a la mano
			ReturnToHand();
		}

		GameManager.Instance.SetDragging(false);
	}

	public void PlayCard(Zone zone)
	{
		playedTurn = GameManager.Instance.turnManager.currentTurn;		

		GameManager.Instance.RegisterPlayThisTurn(isPlayerCard);
		GameManager.Instance.playedOrderThisTurn.Add(this);

		zone.AddCard(this);
		
		if (data.onRevealEffect)
		{
			GameManager.Instance.pendingReveal.Add(this);
		}
		
		if (data.ongoingEffect)
		{
			GameManager.Instance.pendingReveal.Add(this);
		}
		
		if (data.conditionalEffect) 
		{
			GameManager.Instance.pendingReveal.Add(this);
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
	
	public void AddPower(int amount)
	{
		if (amount == 0) return;

		// Bloquear solo reducciones
		if (amount < 0 && cantHavePowerReduced)
		{
			Debug.Log($"[POWER REDUCTION BLOCKED] {data.cardName} no puede perder poder.");
			return;
		}

		currentPower += amount;

		UpdatePowerUI();

		var zone = GameManager.Instance.GetZoneForCard(this);
		if (zone != null) zone.UpdatePowerDisplay();
	}


}
