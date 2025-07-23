
using System;

namespace _APA.Scripts.Managers
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;

    public class APANarratorController : APAMonoBehaviour
    {
        private const string SPLIT_TRIGGER_ID = "Split";
        private const string FINAL_TRIGGER_ID = "FinalVideo";
        [Tooltip("Assign all NarrationEventData Scriptable Objects here.")]
        [SerializeField]
        private List<APANarrationEventData> narrationEvents;

        private Dictionary<string, APANarrationEventData> narrationLookup;

        private void Awake()
        {
            narrationLookup = new Dictionary<string, APANarrationEventData>();
            if (narrationEvents != null)
            {
                foreach (var evtData in narrationEvents)
                {
                    if (evtData == null) continue;
                    if (!narrationLookup.ContainsKey(evtData.TriggerID))
                    {
                        evtData.lastPlayedTime = -1000f;
                        evtData.hasBeenPlayed = false;
                        narrationLookup.Add(evtData.TriggerID, evtData);
                    }
                    else
                    {
                        APADebug.LogWarning($"Duplicate TriggerID '{evtData.TriggerID}' found in Narration Events!"
                           );
                    }
                }
            }
            else
            {
                APADebug.LogError("NarratorController has no NarrationEventData assigned!");
            }
        }


        private void OnEnable()
        {
            Manager.EventManager.AddListener(APAEventName.OnObjectActivate, HandleObjectActivate);
            Manager.EventManager.AddListener(APAEventName.OnObjectDeactivate, HandleObjectDeactivate);
        }

        private void OnDisable()
        {
            Manager.EventManager.RemoveListener(APAEventName.OnObjectActivate, HandleObjectActivate);
            Manager.EventManager.RemoveListener(APAEventName.OnObjectDeactivate, HandleObjectDeactivate);

        }
        
        private void HandleObjectActivate(object data)
        {
            if (data is not Tuple<string, GameObject> tuple) return;

            string narrationTriggerID = tuple.Item1;
            TryPlayNarration(narrationTriggerID);
        }
        private void HandleObjectDeactivate(object data)
        {
            if (data is not Tuple<string, GameObject> tuple) return;

            string narrationTriggerID = tuple.Item1;
            APADebug.Log($"NarratorController: Handling Object Deactivate. Trying narration ID: {narrationTriggerID}");
            TryPlayNarration(narrationTriggerID);
        }

        private void TryPlayNarration(string triggerID)
        {
            if (triggerID == SPLIT_TRIGGER_ID && APASplitScreenManager.Instance != null)
            {
                var lightPlayer = FindObjectOfType<APALightInteractionController>();
                if (lightPlayer != null)
                {
                    InvokeEvent(APAEventName.OnShowStuckDecisionUI, null);
                }
                else
                {
                    Debug.LogWarning("NarratorController: Tried to trigger stuck sequence, but no LightInteractionController was found in scene.");
                }
            }
            if (triggerID == FINAL_TRIGGER_ID && APASplitScreenManager.Instance != null)
            {
                var lightPlayer = FindObjectOfType<APALightInteractionController>();
                if (lightPlayer != null)
                {
                    APAGameManager.Instance.PlayEndingVideo();
                }
            }

            Debug.Log($"[Narrator] Checking narrationLookup for triggerID: {triggerID}");

            if (narrationLookup.TryGetValue(triggerID, out APANarrationEventData eventData))
            {
                if (eventData.PlayOnlyOnce && eventData.hasBeenPlayed)
                {
                    return;
                }

                if (Time.time < eventData.lastPlayedTime + eventData.Cooldown)
                {
                    return;
                }

                if (APASoundManager.Instance != null && eventData.VoiceLine != null)
                {
                    APADebug.Log($"NarratorController: Playing '{triggerID}' (Clip: {eventData.VoiceLine.name})");
                    APASoundManager.Instance.PlayVoiceLine(eventData.VoiceLine, eventData.Delay);

                    eventData.lastPlayedTime = Time.time;
                    eventData.hasBeenPlayed = true;

                    if (!string.IsNullOrEmpty(eventData.SubtitleText))
                    {
                        APASubtitleManager.Instance?.ShowSubtitle(eventData.SubtitleText, eventData.VoiceLine.length);
                    }
                }
                else
                {
                    if (eventData.VoiceLine == null)
                        APADebug.LogError($"NarrationEventData for '{triggerID}' is missing an AudioClip!");
                    if (APASoundManager.Instance == null) APADebug.LogError("SoundManager Instance is missing!");
                }
            }
        }

    }
}