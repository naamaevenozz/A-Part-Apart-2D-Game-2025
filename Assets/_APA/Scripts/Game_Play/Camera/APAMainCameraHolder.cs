using UnityEngine;
using UnityEngine.Serialization;

namespace _APA.Scripts
{
    public class APAMainCameraHolder : APAMonoBehaviour
    {
        [FormerlySerializedAs("APAMainCameraController")] public APAMainCameraController apaMainCameraController;
    }
}
