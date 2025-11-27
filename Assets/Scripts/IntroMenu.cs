using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroMenu : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "CombatEncounter"; // set to your scene name

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
