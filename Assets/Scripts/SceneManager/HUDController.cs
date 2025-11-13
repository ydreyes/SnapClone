using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
	public TextMeshProUGUI lifeText;
	public TextMeshProUGUI heroText;

	void Start()
	{
		RefreshHUD();
	}

	void OnEnable()
	{
		RefreshHUD();
	}

	public void RefreshHUD()
	{
		if (PlayerProgress.Instance == null) return;

		lifeText.text = $"Vidas: {PlayerProgress.Instance.lives}";
		heroText.text = $"Héroe: {PlayerProgress.Instance.heroPoints}";
	}
}