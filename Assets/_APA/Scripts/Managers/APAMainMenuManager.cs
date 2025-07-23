namespace _APA.Scripts
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    public class APAMainMenuManager : APAMonoBehaviour
    {
        [SerializeField] private string gameWorldSceneName = "GameWorld";
        [SerializeField] private RawImage backgroundVideoDisplay;

        private bool isLoading = false;

        void Start()
        {
            Time.timeScale = 1f;

            if (APAGameManager.Instance != null)
            {
                if (backgroundVideoDisplay != null && APAGameManager.Instance.mainMenuRenderTexture != null)
                {
                    backgroundVideoDisplay.texture = APAGameManager.Instance.mainMenuRenderTexture;
                    backgroundVideoDisplay.enabled = true;
                    APAGameManager.Instance.PlayMainMenuBackgroundVideo();
                }
                else
                {
                    APADebug.LogWarning("Missing RawImage or RenderTexture. Can't play background video.");
                }
            }
        }

        void Update()
        {
            if (isLoading) return;

            if (Input.anyKeyDown &&
                !Input.GetKeyDown(KeyCode.Escape) &&
                !Input.GetMouseButtonDown(0))
            {
                if (UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject == null)
                    StartGame();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                QuitGame();
        }

        public void StartGame()
        {
            if (isLoading || string.IsNullOrEmpty(gameWorldSceneName)) return;

            isLoading = true;
            backgroundVideoDisplay.enabled = false;
            APAGameManager.Instance?.StopMainMenuBackgroundVideo();
            APAGameManager.Instance?.TriggerIntroVideo();
        }

        public void QuitGame()
        {
            APAGameManager.Instance?.StopMainMenuBackgroundVideo();
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
