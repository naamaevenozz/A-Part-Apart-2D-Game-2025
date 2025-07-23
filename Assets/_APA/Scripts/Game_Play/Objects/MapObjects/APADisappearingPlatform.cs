namespace _APA.Scripts
{
    using UnityEngine;
    using System.Collections;

    [RequireComponent(typeof(Collider2D))]
    public class APADisappearingPlatform : APAMonoBehaviour
    {
        public enum ActivationMode
        {
            OnContact,
            OnSignal
        }

        [Header("Behavior")] [SerializeField] private ActivationMode activationMode = ActivationMode.OnContact;

        [Header("Event Settings (for OnSignal mode)")] [SerializeField]
        private string platformID;

        [SerializeField] private float delayOnSignal = 0f;

        [Header("Timing (for OnContact mode)")] [SerializeField]
        private float timeUntilOnContact = 1.0f;

        [SerializeField] private float timeUntilReappear = 3.0f;

        [Header("Visuals & Physics")] [SerializeField]
        private Collider2D platformCollider;

        [SerializeField] private SpriteRenderer platformRenderer;
        [SerializeField] private Animator platformAnimator;

        [Header("Audio")]
        private Animator _animator;

        private enum PlatformState
        {
            Idle,
            Triggered,
            Hidden
        }

        private PlatformState currentState = PlatformState.Idle;
        private Coroutine activeCoroutine = null;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        void Awake()
        {
            if (platformCollider == null) platformCollider = GetComponent<Collider2D>();
            if (platformRenderer == null) platformRenderer = GetComponent<SpriteRenderer>();
            if (platformAnimator == null) platformAnimator = GetComponent<Animator>();

            if (platformCollider == null) APADebug.LogError("DisappearingPlatform needs a Collider2D!");
            if (platformRenderer == null) APADebug.LogError("DisappearingPlatform needs a SpriteRenderer!");
            if (platformAnimator == null) APADebug.LogError("DisappearingPlatform needs an Animator!");

            initialPosition = transform.position;
            initialRotation = transform.rotation;

            if (activationMode == ActivationMode.OnSignal && string.IsNullOrEmpty(platformID))
            {
                APADebug.LogWarning(
                    $"DisappearingPlatform '{gameObject.name}' is set to OnSignal mode but has no Platform ID assigned!");
            }
        }

        void OnEnable()
        {
            if (activationMode == ActivationMode.OnSignal)
            {
                Manager.EventManager.AddListener(APAEventName.OnObjectActivate,HandleEventManagerSignal);
                Manager.EventManager.AddListener(APAEventName.OnObjectDeactivate,HandleEventManagerSignal);
            }

            _animator = GetComponent<Animator>();
        }

        void OnDisable()
        {
            if (activationMode == ActivationMode.OnSignal)
            {
                Manager.EventManager.RemoveListener(APAEventName.OnObjectActivate, HandleEventManagerSignal);
                Manager.EventManager.RemoveListener(APAEventName.OnObjectDeactivate, HandleEventManagerSignal);
            }

            StopAndReset();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (activationMode != ActivationMode.OnContact || currentState != PlatformState.Idle)
                return;

            if (collision.gameObject.CompareTag("Player"))
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (Vector2.Dot(contact.normal, Vector2.down) > 0.7f)
                    {
                        StartDisappearSequence(timeUntilOnContact);
                        break;
                    }
                }
            }
        }

        private void HandleEventManagerSignal(object receivedID)
        {
            receivedID = receivedID.ToString();
            if (activationMode != ActivationMode.OnSignal
                || currentState != PlatformState.Idle
                || string.IsNullOrEmpty(platformID)
                || platformID != receivedID)
            {
                return;
            }

            APADebug.Log(
                $"DisappearingPlatform '{gameObject.name}' received signal for ID '{receivedID}'. Starting disappear sequence (OnSignal).");
            StartDisappearSequence(delayOnSignal);
        }

        private void StartDisappearSequence(float initialDelay)
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);

            currentState = PlatformState.Triggered;
            activeCoroutine = StartCoroutine(DisappearAndReappearRoutine(initialDelay));
        }

        private IEnumerator DisappearAndReappearRoutine(float delayBeforeDisappearing)
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Disappear");
            }
            if (delayBeforeDisappearing > 0.01f)
            {
                yield return new WaitForSeconds(delayBeforeDisappearing);
            }

            if (currentState != PlatformState.Triggered) yield break;

            SetPlatformActive(false);
            currentState = PlatformState.Hidden;

            yield return new WaitForSeconds(timeUntilReappear);

            if (currentState != PlatformState.Hidden) yield break;

            SetPlatformActive(true);
            currentState = PlatformState.Idle;
            activeCoroutine = null;
        }

        private void SetPlatformActive(bool isActive)
        {
            if (platformCollider != null) platformCollider.enabled = isActive;
            if (platformRenderer != null) platformRenderer.enabled = isActive;
        }

        private void StopAndReset()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }

            SetPlatformActive(true);
            currentState = PlatformState.Idle;
        }

        public void ResetPlatformState()
        {
            StopAndReset();
        }
    }
}