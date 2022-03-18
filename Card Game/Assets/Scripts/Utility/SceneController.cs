using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
	static public void QuitApp() {
		Application.Quit();
	}

	static public void ChangeScene(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}
}
