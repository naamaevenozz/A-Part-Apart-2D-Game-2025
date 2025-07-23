using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _APA.Scripts
{
    public class APAGameLoaderComponent : APAMonoBehaviour
    {
        private void Start()
        {
            var manager = APAManager.Instance;
            if (manager.EventManager != null)
            {
                int mainMenuIndex = 1;
                SceneManager.LoadScene(mainMenuIndex);
            }

        }
    }
}
