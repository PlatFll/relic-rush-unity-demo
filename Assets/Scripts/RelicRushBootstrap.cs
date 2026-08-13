using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace RelicRush
{
    public static class RelicRushBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            EnsureCamera();
            EnsureEventSystem();

            if (Object.FindFirstObjectByType<RelicRushGameCompact>() != null)
                return;

            var runtime = new GameObject("Relic Rush Runtime");
            Object.DontDestroyOnLoad(runtime);
            runtime.AddComponent<RelicRushGameCompact>();
        }

        static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var eventSystemObject = new GameObject("EventSystem");
            Object.DontDestroyOnLoad(eventSystemObject);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        static void EnsureCamera()
        {
            if (Object.FindFirstObjectByType<Camera>() != null)
                return;

            var cameraObject = new GameObject("Relic Rush Camera");
            Object.DontDestroyOnLoad(cameraObject);
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 0;
            camera.depth = -100f;
        }
    }
}
