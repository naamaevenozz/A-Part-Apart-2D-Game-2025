using System;
using UnityEngine;
using System.Collections.Generic;

namespace _APA.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class APAMovingPlatform : APAMonoBehaviour
    {
        public enum PlatformActivationMode { SingleTrip, StartContinuousLoop }

        [Header("Activation Settings")]
        [SerializeField] private PlatformActivationMode activationMode = PlatformActivationMode.SingleTrip;
        [SerializeField] private bool startActivated = false;
        [SerializeField] private string platformID;
        [SerializeField, Min(1)] private int requiredActivations = 1;
        [SerializeField] private bool listenForActivate = true;
        [SerializeField] private bool listenForDeactivate = true;
        [SerializeField] private bool ignoreDeactivationAfterActivation = false;

        [Header("Movement Settings")]
        [SerializeField] private Vector2 movementDisplacement = Vector2.right * 5f;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float pauseAtEnds = 0.5f;
        [SerializeField] private Transform startPointMarker;

        [Header("Audio")]
        [SerializeField] private AudioClip movementSound;

        private Rigidbody2D rb;
        private Vector2 startPoint, endPoint, currentTarget;
        private bool isMoving, isLooping, movingToEnd, isPaused;
        private float pauseTimer;
        private HashSet<GameObject> activeSources = new();
        private bool activationRequirementMetOnce;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.freezeRotation = true;

            startPoint = startPointMarker ? (Vector2)startPointMarker.position : rb.position;
            endPoint = startPoint + movementDisplacement;
            rb.position = startPoint;
            currentTarget = startPoint;
        }

        void Start()
        {
            if (startActivated)
            {
                activationRequirementMetOnce = true;
                StartMovement(activationMode == PlatformActivationMode.SingleTrip ? endPoint : ClosestTarget(), activationMode == PlatformActivationMode.StartContinuousLoop);
            }
        }

        void OnEnable()
        {
            // if (listenForActivate) EventManager.OnObjectActivate += HandleActivation;
            if (listenForActivate) Manager.EventManager.AddListener(APAEventName.OnObjectActivate,HandleActivation);
            // if (listenForDeactivate) EventManager.OnObjectDeactivate += HandleDeactivation;
            if (listenForDeactivate) Manager.EventManager.AddListener(APAEventName.OnObjectDeactivate,HandleDeactivation);

        }

        void OnDisable()
        {
            Manager.EventManager.RemoveListener(APAEventName.OnObjectActivate, HandleActivation);
            Manager.EventManager.RemoveListener(APAEventName.OnObjectDeactivate, HandleDeactivation);
            // EventManager.OnObjectActivate -= HandleActivation;
            // EventManager.OnObjectDeactivate -= HandleDeactivation;
            activeSources.Clear();
        }

        void FixedUpdate()
        {
            if (movementDisplacement.sqrMagnitude < 0.001f) return;

            if (isPaused)
            {
                pauseTimer -= Time.fixedDeltaTime;
                if (pauseTimer <= 0f)
                {
                    isPaused = false;
                    isMoving = true;
                    currentTarget = movingToEnd ? endPoint : startPoint;
                }
                return;
            }

            if (!isMoving) return;

            Vector2 newPos = Vector2.MoveTowards(rb.position, currentTarget, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(newPos, currentTarget) < 0.01f)
            {
                rb.position = currentTarget;

                if (isLooping)
                {
                    movingToEnd = !movingToEnd;
                    PauseIfNeeded();
                }
                else
                {
                    isMoving = false;
                    PauseIfNeeded();
                }
            }
        }

        private void HandleActivation(object data)
        {
            if (data is not Tuple<string, GameObject> tuple) return;
            string id = tuple.Item1;
            GameObject source = tuple.Item2;
            if (!enabled || id != platformID || movementDisplacement.sqrMagnitude < 0.001f) return;
            bool alreadyMet = activeSources.Count >= requiredActivations;
            if (id == "0")
            {
                APADebug.Log(alreadyMet);
            }
            activeSources.Add(source);
            if (activeSources.Count >= requiredActivations && !alreadyMet)
            {
                activationRequirementMetOnce = true;
                StartMovement(activationMode == PlatformActivationMode.SingleTrip ? endPoint : ClosestTarget(), activationMode == PlatformActivationMode.StartContinuousLoop);
            }
        }

        private void HandleDeactivation(object data)
        {
            if (data is not Tuple<string, GameObject> tuple) return;
            string id = tuple.Item1;
            GameObject source = tuple.Item2;
            if (!enabled || id != platformID || movementDisplacement.sqrMagnitude < 0.001f) return;
            bool wasMet = activeSources.Count >= requiredActivations;
            if (!activeSources.Remove(source)) return;
            if (wasMet && activeSources.Count < requiredActivations)
            {
                if (ignoreDeactivationAfterActivation && activationRequirementMetOnce) return;
                activationRequirementMetOnce = false;
                if (activationMode == PlatformActivationMode.SingleTrip && Vector2.Distance(rb.position, endPoint) < 0.1f)
                    StartMovement(startPoint, false);
                else if (activationMode == PlatformActivationMode.StartContinuousLoop)
                    StopMovement();
            }
        }

        private void StartMovement(Vector2 target, bool loop)
        {
            currentTarget = target;
            isMoving = true;
            isLooping = loop;
            movingToEnd = target == endPoint;
            isPaused = false;
            APASoundManager.Instance?.PlaySFX(movementSound );
        }

        private void StopMovement()
        {
            isMoving = false;
            isPaused = false;
            isLooping = false;
        }

        private void PauseIfNeeded()
        {
            if (pauseAtEnds > 0f)
            {
                isPaused = true;
                pauseTimer = pauseAtEnds;
            }
        }

        private Vector2 ClosestTarget() =>
            Vector2.Distance(rb.position, startPoint) < Vector2.Distance(rb.position, endPoint) ? endPoint : startPoint;
    }
}