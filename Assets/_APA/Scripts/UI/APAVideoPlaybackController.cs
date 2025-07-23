using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;

namespace _APA.Scripts
{
    [RequireComponent(typeof(VideoPlayer))]
    public class APAVideoPlaybackController : APAMonoBehaviour
    {
        [Header("Components")] [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Canvas videoCanvas;
        [SerializeField] private RawImage videoDisplayImage;

        [Header("Skip Settings")] [SerializeField]
        private bool allowSkip = true;

        [SerializeField] private KeyCode skipKey1 = KeyCode.Escape;
        [SerializeField] private KeyCode skipKey2 = KeyCode.KeypadEnter;

        private Action onVideoCompleteCallback;
        private Action onReachedLoopPointCallback;
        private Action onCompleteIfNoLoopCallback;

        private bool isPlaying = false;
        private bool isStopping = false;
        private bool isLoopingAtEnd = false;
        private double endLoopStartTime = 0;

        private RawImage uiTargetRawImage;
        private RenderTexture externalRenderTexture;

        private void Awake()
        {
            if (!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();
            if (!audioSource) audioSource = GetComponent<AudioSource>();

            videoPlayer.playOnAwake = false;
            if (audioSource) audioSource.playOnAwake = false;

            SetupEventHandlers();

            HideVideoUI();
        }

        private void SetupEventHandlers()
        {
            videoPlayer.prepareCompleted += VideoPrepared;
            videoPlayer.loopPointReached += VideoLoopPointReached;
            videoPlayer.errorReceived += VideoError;
        }

        private void OnDestroy()
        {
            videoPlayer.prepareCompleted -= VideoPrepared;
            videoPlayer.loopPointReached -= VideoLoopPointReached;
            videoPlayer.errorReceived -= VideoError;
        }

        private void Update()
        {
            if (allowSkip && isPlaying && onVideoCompleteCallback != null &&
                (Input.GetKeyDown(skipKey1) || Input.GetKeyDown(skipKey2)))
            {
                APADebug.Log($"Video skipped: {videoPlayer.clip?.name}");
                StopPlaybackAndCallback();
            }
        }

        public void Play(VideoClip clip, Action onComplete, bool loop = false,
            RenderTexture renderTexture = null, RawImage rawImage = null,
            VideoAudioOutputMode audioOutput = VideoAudioOutputMode.None,
            AudioSource customAudioSource = null)
        {
            if (isStopping || clip == null)
            {
                APADebug.LogWarning("Play() ignored. Either stopping or clip is null.");
                onComplete?.Invoke();
                return;
            }

            StopIfBusy();
            ResetPlaybackState();

            SetupPlayback(clip, loop, renderTexture, rawImage, audioOutput, customAudioSource);
            onVideoCompleteCallback = loop ? null : onComplete;

            videoPlayer.Prepare();
        }

        private void SetupPlayback(VideoClip clip, bool loop, RenderTexture renderTexture, RawImage rawImage,
            VideoAudioOutputMode audioOutput, AudioSource customAudioSource)
        {
            videoPlayer.clip = clip;
            videoPlayer.isLooping = loop;
            uiTargetRawImage = rawImage;
            externalRenderTexture = renderTexture;

            SetupAudio(audioOutput, customAudioSource);
            SetupVideoRenderMode();
        }

        private void SetupAudio(VideoAudioOutputMode mode, AudioSource customSource)
        {
            videoPlayer.audioOutputMode = mode;
            if (mode == VideoAudioOutputMode.AudioSource)
            {
                AudioSource source = customSource ?? audioSource;
                if (source)
                {
                    videoPlayer.SetTargetAudioSource(0, source);
                    source.Stop();
                }
                else
                {
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                }
            }
        }

        private void SetupVideoRenderMode()
        {
            if (externalRenderTexture)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = externalRenderTexture;
                HideVideoUI();
            }
            else if (uiTargetRawImage)
            {
                videoPlayer.renderMode = VideoRenderMode.APIOnly;
                HideVideoUI();
            }
            else
            {
                videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                if (!videoPlayer.targetCamera) videoPlayer.targetCamera = Camera.main;
                ShowVideoUI();
            }
        }

        private void VideoPrepared(VideoPlayer vp)
        {
            if (isStopping || vp.clip == null) return;
            DisplayVideo();
            vp.Play();
            isPlaying = true;
        }

        private void VideoLoopPointReached(VideoPlayer vp)
        {
            if (isLoopingAtEnd)
            {
                if (vp.time >= endLoopStartTime)
                {
                    onReachedLoopPointCallback?.Invoke();
                }
                else
                {
                    onCompleteIfNoLoopCallback?.Invoke();
                    isPlaying = false;
                    isLoopingAtEnd = false;
                }
            }
            else
            {
                StopPlaybackAndCallback();
            }
        }

        private void VideoError(VideoPlayer vp, string message)
        {
            APADebug.LogError($"Video Error: {message} (Clip: {vp.clip?.name})");
            StopPlaybackAndCallback();
        }

        private void StopIfBusy()
        {
            if (videoPlayer.isPlaying || videoPlayer.isPrepared)
            {
                videoPlayer.Stop();
            }
        }

        private void ResetPlaybackState()
        {
            isPlaying = false;
            isStopping = false;
            isLoopingAtEnd = false;
            onVideoCompleteCallback = null;
            onReachedLoopPointCallback = null;
            onCompleteIfNoLoopCallback = null;
        }

        private void StopPlaybackAndCallback()
        {
            if (isStopping) return;

            isStopping = true;
            isPlaying = false;

            if (videoPlayer.isPlaying) videoPlayer.Stop();

            HideVideoUI();

            Action callback = onVideoCompleteCallback;
            onVideoCompleteCallback = null;
            callback?.Invoke();
        }

        private void ShowVideoUI()
        {
            if (videoCanvas) videoCanvas.gameObject.SetActive(true);
            if (videoDisplayImage)
            {
                videoDisplayImage.texture = null;
                videoDisplayImage.color = Color.black;
                videoDisplayImage.enabled = true;
            }
        }

        private void DisplayVideo()
        {
            if (externalRenderTexture == null && uiTargetRawImage == null && videoDisplayImage)
            {
                videoDisplayImage.texture = videoPlayer.texture;
                videoDisplayImage.color = Color.white;
                videoDisplayImage.enabled = true;
            }

            if (videoCanvas && externalRenderTexture == null && uiTargetRawImage == null)
            {
                videoCanvas.gameObject.SetActive(true);
            }
        }

        private void HideVideoUI()
        {
            if (videoCanvas) videoCanvas.gameObject.SetActive(false);
            if (videoDisplayImage) videoDisplayImage.enabled = false;
            if (uiTargetRawImage) uiTargetRawImage.enabled = false;
        }

        public void ForceStop()
        {
            StopPlaybackAndCallback();
        }

        public bool IsPlaying() => videoPlayer && videoPlayer.isPlaying;
        public bool IsCurrentlyPlaying() => isPlaying || (videoPlayer && videoPlayer.isPlaying);
    }
}