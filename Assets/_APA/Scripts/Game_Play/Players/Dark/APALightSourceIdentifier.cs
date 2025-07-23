namespace _APA.Scripts
{
    using UnityEngine;

    public class APALightSourceIdentifier : APAMonoBehaviour
    {
        public Transform SourceTransform { get; private set; }

        void Awake()
        {
            SourceTransform = transform;
        }
    }
}