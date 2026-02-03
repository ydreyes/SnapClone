using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ZoneController : MonoBehaviour
{
	[Header("Nodes UI (7)")]
	public Button[] nodeButtons = new Button[7];

	[Header("Preview UI")]
	public GameObject previewPanel;
	public Image enemyImage;
	public TMPro.TextMeshProUGUI enemyName;
	public TMPro.TextMeshProUGUI enemyBlurb;
	public Button fightButton;
	public Button backButton; // opcional

	private EnemyData[] pool;     // 5 enemigos de la zona
	private EnemyData selectedEnemy;
	private int availableCount = 3; // 3 al inicio
	
	[Header("Exit Decision UI")]
	public GameObject exitDecisionPanel;
	public Button continuePlayingButton;
	public Button exitToMapButton;

	void Start()
	{
		
		ClearPreview();
		
		// Si vienes de GameScene tras ganar, necesitas marcar y desbloquear:
		int last = PlayerPrefs.GetInt("LastNodeIndex", -1);

		if (PlayerPrefs.GetInt("PendingMark", 0) == 1 && last != -1)
		{
			PlayerPrefs.SetInt("PendingMark", 0);
			MarkNodeCompletedAndUnlockMore();
			return;
		}
		else
		{
			PlayerPrefs.SetInt("PendingMark", 0);
			//PlayerPrefs.SetInt("LastNodeIndex", -1);
		}

		LoadEnemyPool();
		SetupNodes();

		if (backButton) backButton.onClick.AddListener(() =>
		{
			SceneManager.LoadScene("WorldMapScene");
		});
	}
	
	void LoadEnemyPool()
	{
		int zoneIndex = PlayerProgress.Instance.currentZoneIndex;
		string path = "Enemies/Zone" + zoneIndex; // Resources/Enemies/ZoneX

		EnemyPool poolAsset = Resources.Load<EnemyPool>($"Enemies/Zone{zoneIndex}/EnemyPool_Zone{zoneIndex}");
		if (poolAsset != null && poolAsset.enemies.Length > 0)
		{
			pool = poolAsset.enemies;
		}
		else
		{
			Debug.LogWarning($"No se encontraron EnemyPool en Resources/{path}");
			pool = new EnemyData[0];
		}
	}

	void SetupNodes()
	{
		var pp = PlayerProgress.Instance;
		// calcula cuántos nodos completados hay para ajustar la disponibilidad si recargas
		int done = 0;
		foreach (bool b in pp.nodesCompleted) if (b) done++;
		availableCount = Mathf.Clamp(3 + (done * 2), 3, 7);

		for (int i = 0; i < nodeButtons.Length; i++)
		{
			int idx = i;
			bool alreadyDone = pp.nodesCompleted[i];

			nodeButtons[i].onClick.RemoveAllListeners();
			nodeButtons[i].onClick.AddListener(() => OnClickNode(idx));

			// interactable solo si no está completado y está dentro del límite actual
			nodeButtons[i].interactable = !alreadyDone && (idx < availableCount);
		}
	}

	void OnClickNode(int nodeIndex)
	{
		// Elige enemigo aleatorio del pool de 5
		if (pool == null || pool.Length == 0) return;
		selectedEnemy = pool[Random.Range(0, pool.Length)];
		
		Debug.Log($"[ZoneController] Nodo {nodeIndex} seleccionado, enemigo: {selectedEnemy.enemyName}");

		// Muestra preview
		if (enemyImage) enemyImage.sprite = selectedEnemy.previewArt ? selectedEnemy.previewArt : selectedEnemy.portrait;
		if (enemyName) enemyName.text = selectedEnemy.enemyName;
		if (enemyBlurb) enemyBlurb.text = selectedEnemy.blurb;
		
		if (previewPanel)
		{
			previewPanel.SetActive(true);
		}

		fightButton.onClick.RemoveAllListeners();
		fightButton.onClick.AddListener(() => StartFight(nodeIndex));
		fightButton.interactable = true;
	}

	void StartFight(int nodeIndex)
	{
		Debug.Log($"[ZoneController] StartFight node={nodeIndex}, enemy={selectedEnemy?.enemyName}");

		// Guarda selección y nodo para marcar al regresar
		GameSession.Instance.SetEnemy(selectedEnemy);
		PlayerPrefs.SetInt("LastNodeIndex", nodeIndex);

		// Ir a GameScene (tu escena de combate actual)
		SceneManager.LoadScene("GameScene");
	}

	public void MarkNodeCompletedAndUnlockMore()
	{
		int last = PlayerPrefs.GetInt("LastNodeIndex", -1);
		if (last < 0) { SetupNodes(); return; }

		var pp = PlayerProgress.Instance;
		pp.nodesCompleted[last] = true;

		// contar completados
		int done = 0;
		foreach (bool b in pp.nodesCompleted) if (b) done++;
		
		//completó los 4?
		if (done ==4)
		{
			ShowExitDecisionPanel();
			return;
		}

		// ¿completó los 7?
		if (done >= 7)
		{
			// desbloquear siguiente zona en el WorldMap
			pp.zonesCompletedOnWorld = Mathf.Max(pp.zonesCompletedOnWorld, pp.currentZoneIndex + 1);
			SceneManager.LoadScene("WorldMapScene");
			return;
		}

		// aún faltan: recalcular nodos disponibles (+2)
		availableCount = Mathf.Clamp(3 + (done * 2), 3, 7);

		// refrescar la escena para actualizar botones
		SceneManager.LoadScene("ZoneScene");
	}
	
	void ShowExitDecisionPanel()
	{
		if (exitDecisionPanel != null)
		{
			exitDecisionPanel.SetActive(true);

			continuePlayingButton.onClick.RemoveAllListeners();
			continuePlayingButton.onClick.AddListener(() =>
			{
				// Seguir jugando → recargar escena con nodos actualizados
				SceneManager.LoadScene("ZoneScene");
			});

			exitToMapButton.onClick.RemoveAllListeners();
			
			exitToMapButton.onClick.AddListener(() =>
			{
			    var pp = PlayerProgress.Instance;
			
			    // 🔥 marcar zona como completada (solo una vez)
			    if (pp.zonesCompletedOnWorld <= pp.currentZoneIndex)
			    {
				    pp.zonesCompletedOnWorld = pp.currentZoneIndex + 1;
			    }
			
			    // evitar que la próxima entrada ejecute lógica de combate anterior
			    PlayerPrefs.SetInt("PendingMark", 0);
			    PlayerPrefs.SetInt("LastNodeIndex", -1);
			
			    SceneManager.LoadScene("WorldMapScene");
			});
		}
	}

	void ClearPreview()
	{
		if (enemyImage) enemyImage.sprite = null;
		if (enemyName) enemyName.text = "";
		if (enemyBlurb) enemyBlurb.text = "";
		if (fightButton) fightButton.interactable = false;
		
		if (previewPanel) previewPanel.SetActive(false);
	}
}
