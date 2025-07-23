using System;
using UnityEngine;

namespace _APA.Scripts
{
    public class APAManager
    {
        private static APAManager _instance;
        public static APAManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new APAManager();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        public APAEventManager EventManager { get; private set; }
        public APAPlayerRegistry PlayerRegistry { get; private set; }

        private APAManager() { }

        private void Initialize()
        {
            var monoManager = new GameObject("MonoManager");
            monoManager.AddComponent<APAMonoManagerObject>();
            UnityEngine.Object.DontDestroyOnLoad(monoManager);

            EventManager = new APAEventManager();
            PlayerRegistry = new();


        }
    }
}