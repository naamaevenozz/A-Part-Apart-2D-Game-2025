using UnityEngine;

namespace _APA.Scripts
{
    public class APAParallaxLayer : APAMonoBehaviour, IPlayerReceiver
    {
        [Header("Player References")]
        [SerializeField] private Transform player1Transform;
        [SerializeField] private Transform player2Transform;

        [Header("Parallax Effect Strength")]
        [SerializeField] private Vector2 parallaxFactor = Vector2.one;

        [Header("Player-Driven Horizontal Oscillation")]
        [SerializeField] private bool enableHorizontalOscillation = false;
        [SerializeField] private float oscillationTravelDistanceX = 5.0f;

        private Vector3 layerInitialAnchorPosition;
        private Vector3 playersMidpointStartPosition;
        private bool setupComplete = false;

        private float currentOscillationOriginX;
        private int currentParallaxDirectionX = 1;

        void Start()
        {
            // TrySetup();
            Manager.PlayerRegistry.RegisterReceiver(this);
            TrySetup();
        }

        public void SetPlayers(Transform p1, Transform p2)
        {
            player1Transform = p1;
            player2Transform = p2;
        }

        private void TrySetup()
        {
            if (player1Transform == null || player2Transform == null)
            {
                return;
            }
            layerInitialAnchorPosition = transform.position;
            playersMidpointStartPosition = CalculateMidpoint(player1Transform.position, player2Transform.position);

            if (enableHorizontalOscillation)
            {
                currentOscillationOriginX = layerInitialAnchorPosition.x;
                if (oscillationTravelDistanceX <= 0)
                {
                    APADebug.LogWarning(
                        $"ParallaxLayer '{gameObject.name}': Oscillation Travel Distance X is zero or negative. Oscillation will be disabled.");
                    enableHorizontalOscillation = false;
                }
            }

            setupComplete = true;
        }

        void LateUpdate()
        {
            if (!setupComplete) return;

            Vector3 currentPlayersMidpoint = CalculateMidpoint(player1Transform.position, player2Transform.position);
            Vector3 playersMidpointDisplacement = currentPlayersMidpoint - playersMidpointStartPosition;

            float rawParallaxDisplacementX = playersMidpointDisplacement.x * parallaxFactor.x;
            float parallaxDisplacementY = playersMidpointDisplacement.y * parallaxFactor.y;

            float effectiveParallaxDisplacementX = rawParallaxDisplacementX;

            if (enableHorizontalOscillation)
            {
                float currentLegTravelX = rawParallaxDisplacementX * currentParallaxDirectionX;
                float potentialLayerX = currentOscillationOriginX + currentLegTravelX;
                float distanceFromOscillationOrigin = Mathf.Abs(potentialLayerX - currentOscillationOriginX);

                if (distanceFromOscillationOrigin >= oscillationTravelDistanceX)
                {
                    float overshoot = distanceFromOscillationOrigin - oscillationTravelDistanceX;
                    effectiveParallaxDisplacementX = currentParallaxDirectionX > 0
                        ? oscillationTravelDistanceX
                        : -oscillationTravelDistanceX;

                    currentParallaxDirectionX *= -1;
                    currentOscillationOriginX = layerInitialAnchorPosition.x + effectiveParallaxDisplacementX;
                }
                else
                {
                    effectiveParallaxDisplacementX = rawParallaxDisplacementX * currentParallaxDirectionX;
                }
            }

            float targetX = layerInitialAnchorPosition.x + effectiveParallaxDisplacementX;
            float targetY = layerInitialAnchorPosition.y + parallaxDisplacementY;

            Vector3 targetPosition = new Vector3(targetX, targetY, layerInitialAnchorPosition.z);
            transform.position = targetPosition;
        }

        private Vector3 CalculateMidpoint(Vector3 pos1, Vector3 pos2)
        {
            return (pos1 + pos2) / 2.0f;
        }
    }
}
