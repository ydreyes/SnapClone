using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
	public static MapController Instance;

	[Header("Refs")]
	public RectTransform mapPanel;     // el panel grande (MapPanel)
	public RectTransform nodesRoot;    // contenedor de nodos
	public RectTransform connectionsRoot; // contenedor de líneas
	public MapNode nodePrefab;

	[Header("Sprites (opcional)")]
	public Sprite enemyIcon;
	public Sprite shopIcon;
	public Sprite bossIcon;

	[Header("Layout")]
	public float verticalPadding = 140f; // distancia entre pasos
	public float horizontalSpread = 220f; // separación lateral para “curvear”

	// runtime
	private List<List<MapNode>> steps = new(); // columnas/filas de pasos

	void Awake() { Instance = this; }

	void Start()
	{
		GenerateFixed5Steps();   // Para el demo: 5 pasos fijos
		UnlockFirstStep();       // Habilitar el primer nodo
	}

	void GenerateFixed5Steps()
	{
		ClearMap();

		// Definimos los 5 pasos exactos: Enemy, Enemy, Shop, Enemy, Boss
		var sequence = new MapNodeType[] {
			MapNodeType.Enemy, MapNodeType.Enemy, MapNodeType.Shop, MapNodeType.Enemy, MapNodeType.Boss
		};

		steps.Clear();

		// Creamos 1 nodo por paso (lineal). Si quieres ramificar, añade 2 en los pasos intermedios.
		for (int i = 0; i < sequence.Length; i++)
		{
			var list = new List<MapNode>();
			var pos = GetStepPosition(i, 0, 1); // (step, indexDentroDelStep, totalDelStep=1)
			var node = CreateNode(sequence[i], i, pos);
			list.Add(node);
			steps.Add(list);
		}

		// Conectar linealmente
		for (int i = 0; i < steps.Count - 1; i++)
		{
			var from = steps[i][0];
			var to   = steps[i+1][0];
			Connect(from, to);
		}
	}

	Vector2 GetStepPosition(int stepIndex, int indexInStep, int countInStep)
	{
		// Vertical: de abajo a arriba (0 abajo)
		float height = mapPanel.rect.height;
		float width  = mapPanel.rect.width;

		// Y: distribuido por pasos
		float y0 = -height * 0.4f; // margen inferior visible
		float y  = y0 + stepIndex * verticalPadding;

		// X: centrado (si countInStep > 1, esparce)
		float x = 0f;
		if (countInStep > 1)
		{
			float start = -horizontalSpread * 0.5f;
			float step  = (countInStep == 1) ? 0 : horizontalSpread / (countInStep - 1);
			x = start + indexInStep * step;
		}

		return new Vector2(x, y);
	}

	MapNode CreateNode(MapNodeType type, int step, Vector2 anchoredPos)
	{
		var node = Instantiate(nodePrefab, nodesRoot);
		node.GetComponent<RectTransform>().anchoredPosition = anchoredPos;

		Sprite s = type switch {
			MapNodeType.Enemy => enemyIcon,
			MapNodeType.Shop  => shopIcon,
			MapNodeType.Boss  => bossIcon,
			_ => null
		};
		string label = type.ToString();

		node.Init(type, step, s, label);
		return node;
	}

	void Connect(MapNode a, MapNode b)
	{
		a.connections.Add(b);
		// Dibuja línea (UI Image) entre A y B
		DrawConnection(a.GetComponent<RectTransform>(), b.GetComponent<RectTransform>());
	}

	void DrawConnection(RectTransform a, RectTransform b)
	{
		var go = new GameObject("Conn", typeof(Image));
		go.transform.SetParent(connectionsRoot, false);
		var img = go.GetComponent<Image>();
		img.color = new Color(1,1,1,0.35f); // línea clarita
		var rt = img.rectTransform;
		rt.pivot = new Vector2(0, 0.5f);

		Vector2 posA = a.anchoredPosition;
		Vector2 posB = b.anchoredPosition;
		Vector2 dir  = (posB - posA);
		float  len   = dir.magnitude;
		float  ang   = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

		rt.sizeDelta = new Vector2(len, 6f); // grosor
		rt.anchoredPosition = posA;
		rt.localRotation = Quaternion.Euler(0,0,ang);
		rt.SetAsFirstSibling(); // líneas detrás de nodos
	}

	void ClearMap()
	{
		for (int i = nodesRoot.childCount - 1; i >= 0; i--) Destroy(nodesRoot.GetChild(i).gameObject);
		for (int i = connectionsRoot.childCount - 1; i >= 0; i--) Destroy(connectionsRoot.GetChild(i).gameObject);
	}

	void UnlockFirstStep()
	{
		if (steps.Count == 0 || steps[0].Count == 0) return;
		foreach (var n in steps[0]) n.SetState(MapNodeState.Available);
	}

	// Llamado por MapNode.OnClick()
	public void SelectNode(MapNode node)
	{
		// Marcar como completado
		node.SetState(MapNodeState.Cleared);

		// Bloquear todo lo demás del mismo step
		foreach (var n in steps[node.stepIndex])
			if (n != node && n.state != MapNodeState.Cleared) n.SetState(MapNodeState.Locked);

		// Habilitar conexiones del siguiente paso
		foreach (var next in node.connections)
			if (next.state != MapNodeState.Cleared) next.SetState(MapNodeState.Available);

		// Lanzar escena según tipo (puedes cambiar nombres)
		switch (node.type)
		{
		case MapNodeType.Enemy:
			StartEncounter(false);
			break;
		case MapNodeType.Shop:
			OpenShop();
			break;
		case MapNodeType.Boss:
			StartEncounter(true);
			break;
		}
	}

	void StartEncounter(bool isBoss)
	{
		if (GameSession.Instance)
		{
			// puedes guardar flags si quieres (ej. GameSession.Instance.isBoss = isBoss)
		}
		SceneManager.LoadScene("GameScene_Combat"); // tu escena de combate
	}

	void OpenShop()
	{
		SceneManager.LoadScene("ShopScene"); // tu escena de tienda
	}
}
