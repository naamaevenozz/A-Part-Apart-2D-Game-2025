using System;
using System.Collections;
using UnityEngine;

namespace _APA.Scripts
{
    public class APAMonoBehaviour : MonoBehaviour
    {
        protected APAManager Manager => APAManager.Instance;
        protected APAMonoManagerObject MonoManager => APAMonoManagerObject.Instance;

        protected void AddListener(APAEventName eventName, Action<object> eventCallback)
        {
            Manager.EventManager.AddListener(eventName, eventCallback);
        }

        protected void RemoveListener(APAEventName eventName, Action<object> eventCallback)
        {
            Manager.EventManager.RemoveListener(eventName, eventCallback);
        }

        protected void InvokeEvent(APAEventName eventName, object obj)
        {
            Manager.EventManager.InvokeEvent(eventName, obj);
        }

        protected Coroutine RunCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        protected T Get<T>() where T : Component
        {
            return GetComponent<T>();
        }

        protected Transform MyTransform => transform;
    }
}