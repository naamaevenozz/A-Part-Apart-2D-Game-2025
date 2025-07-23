
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using APA.Core;

namespace _APA.Scripts
{
    public class APAGameManager : APAMonoBehaviour
    {
        public static APAGameManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameWorldSceneName = "GameWorld";

        [Header("Video Clips")]
        [SerializeField] private VideoClip mainMenuBackgroundVideo;
        [SerializeField] private VideoClip introVideo;
        [SerializeField] private VideoClip middleVideo;
        [SerializeField] private VideoClip gameEndingVideo;
        [SerializeField] private float gameEndingVideoLoopStartTime = 5f;
        [SerializeField] private VideoClip finalCreditsVideo;

        [Header("UI")]
        [SerializeField] public RenderTexture mainMenuRenderTexture;
        [SerializeField] private Image blackScreenOverlay;

        [Header("Video Prefab")]
        [SerializeField] private GameObject videoPlayerPrefab;

        [Header("Prefabs")]
        [SerializeField] private GameObject player1Prefab;
        [SerializeField] private Vector3 spawnPosition1 = new Vector3(-172.1f, 17.7f, 0);
        [SerializeField] private GameObject player2Prefab;
        [SerializeField] private Vector3 spawnPosition2 = new Vector3(-179.7f, 15.3f, 0);

        private GameObject currentEventVideoInstance;
        private GameObject currentMenuBackgroundVideoInstance;
        private GameObject player1Instance;
        private GameObject player2Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void TriggerIntroVideo()
        {
            EnableBlackScreen();
            PlayEventVideo(introVideo, LoadGameWorldScene);
        }

        public void PlayMainMenuBackgroundVideo()
        {
            if (!IsValid(mainMenuBackgroundVideo, mainMenuRenderTexture, videoPlayerPrefab)) return;

            StopCurrentVideos();

            currentMenuBackgroundVideoInstance = Instantiate(videoPlayerPrefab);
            var controller = currentMenuBackgroundVideoInstance.GetComponent<APAVideoPlaybackController>();
            var audio = currentMenuBackgroundVideoInstance.GetComponent<AudioSource>();

            controller.Play(
                mainMenuBackgroundVideo,
                onComplete: null,
                loop: true,
                renderTexture: mainMenuRenderTexture,
                rawImage: null,
                audioOutput: GetAudioMode(mainMenuBackgroundVideo, audio),
                customAudioSource: audio
            );
        }

        public void StopMainMenuBackgroundVideo()
        {
            StopAndDestroy(ref currentMenuBackgroundVideoInstance);
        }

        public void PlayEndingVideo()
        {
            PlayEventVideo(finalCreditsVideo, LoadGameWorldScene);
        }

        private void PlayEventVideo(VideoClip clip, Action onComplete)
        {
            if (!IsValid(clip, null, videoPlayerPrefab)) { onComplete?.Invoke(); return; }

            StopCurrentVideos();
            currentEventVideoInstance = Instantiate(videoPlayerPrefab);
            var controller = currentEventVideoInstance.GetComponent<APAVideoPlaybackController>();
            var audio = currentEventVideoInstance.GetComponent<AudioSource>();

            controller.Play(
                clip,
                onComplete: () =>
                {
                    StopAndDestroy(ref currentEventVideoInstance);
                    onComplete?.Invoke();
                },
                loop: false,
                renderTexture: null,
                rawImage: null,
                audioOutput: GetAudioMode(clip, audio),
                customAudioSource: audio
            );
        }

        private void StopCurrentVideos()
        {
            StopAndDestroy(ref currentEventVideoInstance);
            StopAndDestroy(ref currentMenuBackgroundVideoInstance);
        }

        private void StopAndDestroy(ref GameObject instance)
        {
            if (instance == null) return;
            var controller = instance.GetComponent<APAVideoPlaybackController>();
            controller?.ForceStop();
            Destroy(instance);
            instance = null;
        }

        private void EnableBlackScreen()
        {
            if (blackScreenOverlay)
            {
                blackScreenOverlay.gameObject.SetActive(true);
                blackScreenOverlay.CrossFadeAlpha(1f, 0f, true);
            }
        }

        public void LoadGameWorldScene()
        {
            SceneManager.LoadScene(gameWorldSceneName);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != gameWorldSceneName) return;

            APADebug.Log("[GameManager] GameWorld loaded. Attempting to spawn players...");

            player1Instance = Instantiate(player1Prefab, spawnPosition1, Quaternion.identity);
            player2Instance = Instantiate(player2Prefab, spawnPosition2, Quaternion.identity);

            DontDestroyOnLoad(player1Instance);
            DontDestroyOnLoad(player2Instance);

            APADebug.Log("[GameManager] Players spawned successfully.");
            var cameraPrefab = Resources.Load<APAMainCameraHolder>("Camera");
            if (cameraPrefab == null)
            {
                APADebug.LogWarning("cameraPrefab not loaded.");
                return;
            }

            var cameraInstantiated = Instantiate(cameraPrefab);
            cameraInstantiated.apaMainCameraController.SetPlayers(player1Instance.transform, player2Instance.transform);

            APADebug.Log("Players spawned successfully.");
            APADebug.Log("Players spawned successfully and assigned to barriers and camera.");
            Manager.PlayerRegistry.SetPlayers(player1Instance.transform, player2Instance.transform);

        }

        private bool IsValid(VideoClip clip, RenderTexture texture, GameObject prefab)
        {
            if (clip == null)
            {
                APADebug.LogWarning("Missing VideoClip.");
                return false;
            }
            if (prefab == null)
            {
                APADebug.LogError("Missing VideoPlayerPrefab.");
                return false;
            }
            if (texture == null && clip == mainMenuBackgroundVideo)
            {
                APADebug.LogWarning("Main menu render texture is missing.");
                return false;
            }
            return true;
        }

        private VideoAudioOutputMode GetAudioMode(VideoClip clip, AudioSource source)
        {
            if (clip.audioTrackCount == 0) return VideoAudioOutputMode.None;
            return source ? VideoAudioOutputMode.AudioSource : VideoAudioOutputMode.Direct;
        }
    }
}
