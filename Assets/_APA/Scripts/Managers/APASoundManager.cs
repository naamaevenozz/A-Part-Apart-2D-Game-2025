using UnityEngine;
using System.Collections;

namespace _APA.Scripts
{
    public class APASoundManager : APAMonoBehaviour
    {
        public static APASoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField]
        private AudioSource musicSource;

        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("Gameplay SFX Clips")] public AudioClip objectPushClip;
        public AudioClip doorMoveClip;
        public AudioClip platformMoveClip;
        public AudioClip pressurePlateClickClip;
        public AudioClip backGroundClip1;

        [Header("Settings")][SerializeField] private bool interruptVoiceLines = true;

        private Coroutine currentVoiceCoroutine = null;
        private GameObject _lastPlayVoice;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (voiceSource == null) voiceSource = gameObject.AddComponent<AudioSource>();

            if (backGroundClip1 != null)
            {
                PlayMusic(backGroundClip1);
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic() => musicSource?.Stop();

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void PlayPushSound() => PlaySFX(objectPushClip);
        public void PlayDoorMoveSound() => PlaySFX(doorMoveClip);
        public void PlayPlatformMoveSound() => PlaySFX(platformMoveClip);
        public void PlayPressurePlateSound() => PlaySFX(pressurePlateClickClip);
        public void PlayClickSound() => PlayPressurePlateSound();

        public void PlayVoiceLine(AudioClip clip, float delay = 0f)
        {
            if (voiceSource == null || clip == null) return;

            if (interruptVoiceLines && voiceSource.isPlaying)
            {
                voiceSource.Stop();
                if (currentVoiceCoroutine != null)
                {
                    StopCoroutine(currentVoiceCoroutine);
                    currentVoiceCoroutine = null;
                }
            }

            if (!interruptVoiceLines && voiceSource.isPlaying) return;

            currentVoiceCoroutine = StartCoroutine(PlayVoiceWithDelay(clip, delay));
        }
        private IEnumerator PlayVoiceWithDelay(AudioClip clip, float delay)
        {
            if (clip == null)
            {
                yield break;
            }
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            if (_lastPlayVoice != null)
            {
                Destroy(_lastPlayVoice);
            }

            _lastPlayVoice = new GameObject($"Voice {clip.name}");

            var audioSource = _lastPlayVoice.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = 0.7f;
            audioSource.Play();

            Destroy(_lastPlayVoice, clip.length + 0.1f);

            currentVoiceCoroutine = null;
        }
    }
}