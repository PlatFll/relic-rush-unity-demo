using UnityEngine;
namespace RelicRush
{
    public static class RelicRushBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(Object.FindFirstObjectByType<RelicRushGameCompact>()!=null)return;var g=new GameObject("Relic Rush Runtime");Object.DontDestroyOnLoad(g);g.AddComponent<RelicRushGameCompact>();}
    }
}
