namespace _APA.Scripts
{
    public class APAMonoManagerObject : APAMonoBehaviour
    {
        public static APAMonoManagerObject Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
