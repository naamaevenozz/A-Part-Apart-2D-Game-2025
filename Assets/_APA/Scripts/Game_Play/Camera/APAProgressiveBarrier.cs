using System;
using UnityEngine;

namespace _APA.Scripts
{
    public class APAProgressiveBarrier : APAMonoBehaviour, IPlayerReceiver
    {
        public enum BarrierState { BlockingForward, Open, WaitingForPlayersToPass, BlockingBackward }

        [Header("Barrier Setup")]
        [SerializeField] private int boundaryPointIndex = -1;
        [SerializeField] private string unlockSignalID;
        [SerializeField, Min(1)] private int requiredUnlockSignals = 1;
        [SerializeField] private string sceneToLoadOnUnlock = "";
        [SerializeField] private Transform player1Transform;
        [SerializeField] private Transform player2Transform;

        [SerializeField]private Collider2D blockingCollider;
        private BarrierState currentState;
        private int unlockCount = 0;
        private float barrierX;
        private bool setupComplete = false;
        
        private void Start()
        {
            Manager.PlayerRegistry.RegisterReceiver(this);
            TrySetup();
        }
        void OnEnable()
        {
            if (currentState == BarrierState.BlockingForward && !string.IsNullOrEmpty(unlockSignalID))
                Manager.EventManager.AddListener(APAEventName.OnObjectActivate, OnObjectActivated);
        }
        void OnDisable()
        {
            Manager.EventManager.RemoveListener(APAEventName.OnObjectActivate, OnObjectActivated);
        }
        void Update()
        {
            if (!setupComplete || currentState != BarrierState.WaitingForPlayersToPass)
            {
                return;
            }

            if (player1Transform.position.x > barrierX + 1f && player2Transform.position.x > barrierX + 1f)
            {
                SetState(BarrierState.BlockingBackward);
                if (boundaryPointIndex >= 0)
                {
                    Manager.EventManager.InvokeEvent(APAEventName.OnPlayersPassedBarrier, boundaryPointIndex);
                }
            }
        }
        private void HandleUnlockSignal(string id, GameObject _)
        {
            if (id != unlockSignalID || currentState != BarrierState.BlockingForward || ++unlockCount < requiredUnlockSignals) return;

            if (boundaryPointIndex >= 0)
                Manager.EventManager.InvokeEvent(APAEventName.TriggerBarrierOpened, boundaryPointIndex);
            if (!string.IsNullOrEmpty(sceneToLoadOnUnlock))
                Manager.EventManager.InvokeEvent(APAEventName.TriggerLoadScene, sceneToLoadOnUnlock);

            SetState(BarrierState.Open);
            Manager.EventManager.RemoveListener(APAEventName.OnObjectActivate, OnObjectActivated);
        }

        private void OnObjectActivated(object data)
        {
            if (data is not Tuple<string, GameObject> tuple) return;
            HandleUnlockSignal(tuple.Item1, tuple.Item2);
        }

        private void SetState(BarrierState newState)
        {
            currentState = newState;
            switch (newState)
            {
                case BarrierState.BlockingForward:
                case BarrierState.BlockingBackward:
                    blockingCollider.enabled = true;
                    blockingCollider.isTrigger = false;
                    break;
                case BarrierState.Open:
                    blockingCollider.enabled = false;
                    SetState(BarrierState.WaitingForPlayersToPass);
                    break;
                case BarrierState.WaitingForPlayersToPass:
                    break;
            }
        }

        public void ResetBarrier()
        {
            unlockCount = 0;
            SetState(BarrierState.BlockingForward);
            if (isActiveAndEnabled) OnEnable();
        }

        public void SetPlayers(Transform p1, Transform p2)
        {
            player1Transform = p1;
            player2Transform = p2;
            TrySetup();
        }

        private void TrySetup()
        {
            if (!blockingCollider || !player1Transform || !player2Transform)
            {
                setupComplete = false;
                return;
            }

            barrierX = transform.position.x;
            SetState(BarrierState.BlockingForward);
            setupComplete = true;
        }
    }
}
