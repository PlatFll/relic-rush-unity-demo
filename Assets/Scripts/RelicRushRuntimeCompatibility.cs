using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace RelicRush
{
    [DefaultExecutionOrder(-32000)]
    public sealed class RelicRushRuntimeCompatibility : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (Object.FindFirstObjectByType<RelicRushRuntimeCompatibility>() != null)
                return;

            var host = new GameObject("Relic Rush Compatibility");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<RelicRushRuntimeCompatibility>();

            EnsureCamera();
        }

        private void Update()
        {
            ReplaceLegacyInputModules();
        }

        private static void EnsureCamera()
        {
            if (Object.FindFirstObjectByType<Camera>() != null)
                return;

            var cameraObject = new GameObject("Relic Rush Camera");
            Object.DontDestroyOnLoad(cameraObject);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 0;
            camera.depth = -100f;
            cameraObject.tag = "MainCamera";
        }

        private static void ReplaceLegacyInputModules()
        {
            var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

            foreach (var eventSystem in eventSystems)
            {
                var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    legacyModule.enabled = false;
                    Object.Destroy(legacyModule);
                }

                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }
    }
}
