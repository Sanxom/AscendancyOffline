using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public const string NEW_GAME_START_SCENE = "Game";

    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject loadingIcon;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Start()
    {
        optionsScreen.SetActive(false);
    }

    public IEnumerator LoadStart()
    {
        loadingScreen.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(NEW_GAME_START_SCENE);

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                loadingText.text = $"Press any key to continue...";
                loadingIcon.SetActive(false);

                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    asyncLoad.allowSceneActivation = true;

                    Time.timeScale = 1f;
                }
            }

            yield return null;
        }
    }

    public void StartGame()
    {
        StartCoroutine(LoadStart());
    }

    public void OpenOptions()
    {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsScreen.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
