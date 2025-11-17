using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckCharacterSelectController : MonoBehaviour
{
	public Transform charactersContainer;
	public GameObject characterButtonPrefab;
	public Button confirmButton;
	public Button backButton;

	private CharacterData selected;

	void Start()
	{
		confirmButton.interactable = false;

		LoadCharacters();

		backButton.onClick.AddListener(() =>
			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu")
		);
	}

	void LoadCharacters()
	{
		var chars = Resources.LoadAll<CharacterData>("Characters");

		foreach (var c in chars)
		{
			var go = Instantiate(characterButtonPrefab, charactersContainer);
			var btn = go.GetComponent<CharacterButton>();
			btn.Setup(c, () => OnSelectCharacter(c, btn));
		}
	}

	void OnSelectCharacter(CharacterData data, CharacterButton btn)
	{
		selected = data;
		GameSession.Instance.SelectCharacter(data);

		confirmButton.interactable = true;
	}

	public void OnConfirm()
	{
		// Guardamos el deck que vamos a editar
		GameSession.Instance.deckBeingEdited = selected.starterDeck;
		UnityEngine.SceneManagement.SceneManager.LoadScene("CollectionBuilderScene");
	}
}
