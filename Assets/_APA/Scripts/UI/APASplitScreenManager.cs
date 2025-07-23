using UnityEngine;

namespace _APA.Scripts
{
    public class APASplitScreenManager : APAMonoBehaviour
    {
        public static APASplitScreenManager Instance { get; private set; }

        [Header("Audio")][SerializeField] private AudioClip sequenceSound;
        [SerializeField] private AudioClip afterSound;
        [Header("UI")][SerializeField] private GameObject screenPanel;

        private bool isListeningForInput = false;
        private bool pressedArrow = false;
        private bool pressedDW = false;
        private bool afterSoundPlayed = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
        }

        public void ShowSplitScreen()
        {
            screenPanel.SetActive(true);
            PlaySequenceSound();
        }

        private void PlaySequenceSound()
        {
            APASoundManager.Instance.PlayVoiceLine(sequenceSound);
            Invoke(nameof(StartListeningForInput), 25);
        }

        private void StartListeningForInput()
        {
            isListeningForInput = true;
            afterSoundPlayed = true;
        }

        private void Update()
        {
            if (!isListeningForInput || !afterSoundPlayed)
                return;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                pressedArrow = true;

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W))
                pressedDW = true;

            if (pressedArrow && pressedDW)
            {
                afterSoundPlayed = true;
                PlayAfterSoundAndClose();
            }
        }

        private void PlayAfterSoundAndClose()
        {
            isListeningForInput = false;
            APASoundManager.Instance.PlayVoiceLine(afterSound);
            
            Invoke(nameof(CloseScreen), afterSound.length);
        }

        private void CloseScreen()
        {
            bool canPlayerMove = true;
            InvokeEvent(APAEventName.OnPlayerCanMove, canPlayerMove);

            screenPanel.SetActive(false);

            isListeningForInput = false;
            pressedArrow = false;
            pressedDW = false;
            afterSoundPlayed = false;
        }
        private void HandleShowScreenRequest()
        {
            ShowSplitScreen();
        }
        
        private void OnEnable()
        {
            AddListener(APAEventName.OnShowStuckDecisionUI, OnShowStuckDecisionUI);
        }

        private void OnDisable()
        {
            RemoveListener(APAEventName.OnShowStuckDecisionUI, OnShowStuckDecisionUI);
        }

        private void OnShowStuckDecisionUI(object data)
        {
            bool canPlayerMove = false;
            InvokeEvent(APAEventName.OnPlayerCanMove, canPlayerMove);
            HandleShowScreenRequest();
        }
    }
}