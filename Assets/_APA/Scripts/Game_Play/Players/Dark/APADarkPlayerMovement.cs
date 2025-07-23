using System;
using System.Collections;
using APA.Core;
using UnityEngine;

namespace _APA.Scripts
{
    public class ApaDarkApaPlayerMovement : APAPlayerMovement
    {
        [Header("Settings - push")]
        [SerializeField]
        private float lightPushForce = 50f;

        [SerializeField] private float pushDuration = 0.5f;

        public void StartFixedUpdateCoroutine(bool isEnable)
        {
            if (isEnable)
            {
                if (fixedUpdateCoroutine != null)
                {
                    StopCoroutine(fixedUpdateCoroutine);
                }

                fixedUpdateCoroutine = StartCoroutine(FixedUpdateCoroutine());
            }
            else
            {
                if (fixedUpdateCoroutine != null)
                {
                    StopCoroutine(fixedUpdateCoroutine);
                }
            }
        }

        public IEnumerator LightPushDirectionCoroutine(Vector2 lightPushDirection)
        {
            StartFixedUpdateCoroutine(false);
            float elpased = 0;
            while (elpased < pushDuration)
            {
                rb.AddForce(lightPushDirection * lightPushForce, ForceMode2D.Force);

                elpased += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            StartFixedUpdateCoroutine(true);
        }
    }
}