using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
	public Button playButton;
	public Button DeckButton;
	public Button shopButton;
	
	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	protected void Start()
	{
		playButton.onClick.AddListener(() =>
			SceneManager.LoadScene("CharacterSelect"));

		DeckButton.onClick.AddListener(() =>
			SceneManager.LoadScene("DeckCharacterSelect"));

		shopButton.onClick.AddListener(() =>
			SceneManager.LoadScene("ShopScene"));
	}

	//public void OnQuit()  { UnityEditor.EditorApplication.isPlaying = false; Application.Quit(); }
	
}
