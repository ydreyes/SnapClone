using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
	public Button playButton;
	public Button optionsButton;
	public Button shopButton;
	
	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	protected void Start()
	{
		playButton.onClick.AddListener(() =>
			SceneManager.LoadScene("CharacterSelect"));

		//optionsButton.onClick.AddListener(() =>
		//	SceneManager.LoadScene("OptionsScene"));

		shopButton.onClick.AddListener(() =>
			SceneManager.LoadScene("ShopScene"));
	}
	
	//public void OnPlay()  => SceneManager.LoadScene("CharacterSelect");
	//public void OnQuit()  { UnityEditor.EditorApplication.isPlaying = false; Application.Quit(); }
	
}
