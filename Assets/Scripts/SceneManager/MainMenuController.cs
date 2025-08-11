using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
	public void OnPlay()  => SceneManager.LoadScene("CharacterSelect");
	public void OnQuit()  { UnityEditor.EditorApplication.isPlaying = false; Application.Quit(); }
}
