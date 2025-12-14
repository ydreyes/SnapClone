using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPileViewerUI : MonoBehaviour
{
	public GameObject panel;
	public Transform content;
	public GameObject cardPreviewPrefab;

	public TextMeshProUGUI titleText;

	void Start()
	{
		panel.SetActive(false);
	}

	public void ShowDiscardPile()
	{
		ShowPile("Pila de Descarte", GameManager.Instance.discardPile);
	}

	public void ShowDestroyedPile()
	{
		ShowPile("Cartas Destruidas", GameManager.Instance.destroyedPile);
	}

	void ShowPile(string title, System.Collections.Generic.List<CardData> pile)
	{
		titleText.text = title;

		foreach (Transform child in content)
			Destroy(child.gameObject);

		foreach (var card in pile)
		{
			GameObject go = Instantiate(cardPreviewPrefab, content);
			go.GetComponent<CardView>().SetUp(card);
		}

		panel.SetActive(true);
	}

	public void Close()
	{
		panel.SetActive(false);
	}
}
