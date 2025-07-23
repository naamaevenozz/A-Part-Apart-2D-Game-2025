using System;

namespace _APA.Scripts
{
    using UnityEngine;
    using UnityEngine.Events;
    using System.Collections.Generic;

    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class APAPressurePlate : APAMonoBehaviour
    {
        [Header("Event Manager Settings")]
        [Tooltip("The unique ID of the object(s) this plate activates/deactivates via EventManager.")]
        [SerializeField]
        private string targetObjectID;

        [SerializeField] private List<string> validTags = new List<string> { "Player", "Pushable" };

        [Header("Optional Feedback")] public UnityEvent OnPressedFeedback;
        public UnityEvent OnReleasedFeedback;

        [Header("Audio")] [SerializeField] private AudioClip clickSound;

        private HashSet<Collider2D> activatingColliders = new HashSet<Collider2D>();

        void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            }

            if (string.IsNullOrEmpty(targetObjectID))
            {
                APADebug.LogWarning($"PressurePlate '{gameObject.name}' has no Target Object ID.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsValidTag(other.tag))
            {
                bool wasEmpty = activatingColliders.Count == 0;
                activatingColliders.Add(other);
                if (wasEmpty && activatingColliders.Count > 0)
                {
                    ActivatePlate();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsValidTag(other.tag))
            {
                bool removed = activatingColliders.Remove(other);
                if (removed && activatingColliders.Count == 0)
                {
                    DeactivatePlate();
                }
            }
        }

        private void ActivatePlate()
        {
            if (string.IsNullOrEmpty(targetObjectID)) return;
            Manager.EventManager.InvokeEvent(APAEventName.OnObjectActivate,new Tuple<string,GameObject>(targetObjectID,this.gameObject));
            OnPressedFeedback?.Invoke();

            if (clickSound != null)
                APASoundManager.Instance?.PlaySFX(clickSound);
            else
                APASoundManager.Instance?.PlayClickSound();
        }

        private void DeactivatePlate()
        {
            if (string.IsNullOrEmpty(targetObjectID)) return;
            Manager.EventManager.InvokeEvent(APAEventName.OnObjectDeactivate,new Tuple<string,GameObject>(targetObjectID,this.gameObject));
            OnReleasedFeedback?.Invoke();
        }

        private bool IsValidTag(string tag)
        {
            return validTags.Contains(tag);
        }

        void OnDisable()
        {
            if (activatingColliders.Count > 0)
            {
                DeactivatePlate();
            }

            activatingColliders.Clear();
        }
    }
}