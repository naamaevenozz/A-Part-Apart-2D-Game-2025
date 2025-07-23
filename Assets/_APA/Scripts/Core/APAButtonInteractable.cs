namespace _APA.Scripts
{
    using UnityEngine;
    using UnityEngine.Events;

    [RequireComponent(typeof(Collider2D))]
    public class APAButtonInteractable : APAMonoBehaviour, IInteractable
    {
        [Header("Event Manager Settings")]
        [Tooltip("The unique ID of the object(s) this button should activate/deactivate via EventManager.")]
        [SerializeField]
        private string targetObjectID;

        [Header("Button State")]
        [Tooltip("Does the button start in the 'On' state? (Affects first signal sent)")]
        [SerializeField]
        private bool startsOn = false;

        [Header("Optional Feedback")] [Tooltip("Feedback when the button transitions to the ON state.")]
        public UnityEvent OnTurnedOnFeedback;

        [Tooltip("Feedback when the button transitions to the OFF state.")]
        public UnityEvent OnTurnedOffFeedback;

        private bool isOn; 

        void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogError($"ButtonInteractable '{gameObject.name}' missing Collider2D.", this);
            }

            if (string.IsNullOrEmpty(targetObjectID))
            {
                Debug.LogWarning($"ButtonInteractable '{gameObject.name}' has no Target Object ID.", this);
            }
        }

        void Start()
        {
            isOn = startsOn;
            UpdateVisualState(false); 
        }

        public InteractionType InteractionType => InteractionType.Button;

        public void Interact(GameObject initiatorGameObject)
        {
            if (string.IsNullOrEmpty(targetObjectID))
            {
                Debug.LogWarning($"Button '{gameObject.name}' pressed, but no Target Object ID!", this);
                return;
            }

            isOn = !isOn;

            UpdateVisualState(true);
        }

        private void UpdateVisualState(bool triggerEventsAndFeedback)
        {

            if (triggerEventsAndFeedback)
            {
                if (isOn)
                {
                    Debug.Log($"Button '{gameObject.name}' toggled ON. Sending ACTIVATE for ID: '{targetObjectID}'");
                    // EventManager.TriggerObjectActivate(targetObjectID, this.gameObject);
                    Manager.EventManager.InvokeEvent(APAEventName.TriggerObjectActivate, targetObjectID);
                    OnTurnedOnFeedback?.Invoke();
                }
                else
                {
                    Debug.Log($"Button '{gameObject.name}' toggled OFF. Sending DEACTIVATE for ID: '{targetObjectID}'");
                    // EventManager.TriggerObjectDeactivate(targetObjectID, this.gameObject);
                    Manager.EventManager.InvokeEvent(APAEventName.TriggerObjectDeactivate, targetObjectID);
                    OnTurnedOffFeedback?.Invoke();
                }
            }
        }
    }
}
