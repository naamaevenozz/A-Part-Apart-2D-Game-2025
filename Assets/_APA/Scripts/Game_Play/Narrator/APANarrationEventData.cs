namespace _APA.Scripts
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NarrationEvent_", menuName = "Game/Narration Event", order = 1)]
    public class APANarrationEventData : ScriptableObject
    {
        [Tooltip(
            "Unique ID to match triggers in the game (e.g., 'Level1Start', 'FoundKey', 'PressurePlate_A4_Pressed')")]
        public string TriggerID; 

        [Tooltip("The audio clip to play for this event.")]
        public AudioClip VoiceLine;

        [Tooltip("Optional text for subtitles.")] [TextArea]
        public string SubtitleText;

        [Tooltip("Delay in seconds before playing the voice line after the trigger.")]
        public float Delay = 0f;

        [Tooltip("Minimum time in seconds before this specific trigger ID can be activated again.")]
        public float Cooldown = 5.0f;

        [Tooltip("Should this narration only play once per game session?")]
        public bool PlayOnlyOnce = false;

        [HideInInspector] public float lastPlayedTime = -1000f; 
        [HideInInspector] public bool hasBeenPlayed = false; 
    }
}