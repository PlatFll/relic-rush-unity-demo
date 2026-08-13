using UnityEngine;

namespace RelicRush
{
    public static class RelicRushBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindFirstObjectByType<RelicRushGame>() != null)
                return;

            var root = new GameObject("Relic Rush Runtime");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<RelicRushGame>();
        }
    }
}
