using System.Collections;
using APA.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace _APA.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Animator))]
    public class APALightInteractionController : APAMonoBehaviour
    {
        [FormerlySerializedAs("apaDarkPlayerMovement")] [FormerlySerializedAs("darkPlayerMovement")] [Header("Light Interaction Settings")] [SerializeField]
        private ApaDarkApaPlayerMovement apaDarkApaPlayerMovement;

        [Header("References")] [SerializeField]
        private Transform[] lightCheckPoints;

        [SerializeField] private LayerMask lightBlockerLayer;
        [Header("Settings")] [SerializeField] private float maxLightDistance = 3.0f;

        private Rigidbody2D rb;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioClip hurtSound;
        bool initialized;
        private float lastHurtSoundTime = -Mathf.Infinity;
        private const float hurtSoundCooldown = 10f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }
        private void Start()
        {
            InitializeLightCheckPointsIfNeeded();
            StartCoroutine(FixedUpdateCoroutine());
        }
        private void InitializeLightCheckPointsIfNeeded()
        {
            if (lightCheckPoints == null || lightCheckPoints.Length == 0)
            {
                var pointsGO = GameObject.FindGameObjectsWithTag("Light");
                if(pointsGO.Length == 0)
                {
                    APADebug.LogWarning("No light points found at Start.");
                }
                lightCheckPoints = new Transform[pointsGO.Length];
                for (int i = 0; i < pointsGO.Length; i++)
                {
                    lightCheckPoints[i] = pointsGO[i].transform;
                }
            }
            initialized = true;
        }


        protected IEnumerator FixedUpdateCoroutine()
        {
            if (!initialized )
            {
                yield break;
            }
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                foreach (Transform checkPoint in lightCheckPoints)
                {
                    if (checkPoint == null) continue;

                    float distanceToLight = Vector2.Distance(transform.position, checkPoint.position);
                    if (distanceToLight <= maxLightDistance)
                    {
                        RaycastHit2D hit =
                            Physics2D.Linecast(transform.position, checkPoint.position, lightBlockerLayer);

#if UNITY_EDITOR
                        Debug.DrawLine(transform.position, checkPoint.position,
                            hit.collider != null && hit.collider.tag == "Light" ? Color.red : Color.green, 0f);
#endif
                        if (hit.collider != null && hit.collider.tag == "Light")
                        {
                            animator.SetTrigger("Hurt");
                            if (Time.time - lastHurtSoundTime >= hurtSoundCooldown)
                            {
                                APASoundManager.Instance?.PlaySFX(hurtSound);
                                lastHurtSoundTime = Time.time;
                            }
                            var pushDirection = Vector2.right * -Mathf.Sign(checkPoint.position.x - rb.position.x);
                            yield return apaDarkApaPlayerMovement.LightPushDirectionCoroutine(pushDirection);

                            break;
                        }
                    }
                }   

                yield return waitForFixedUpdate;
            }
        }
    }
}