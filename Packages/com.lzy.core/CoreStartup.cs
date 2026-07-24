using UnityEngine;

namespace LZY
{
    public static class CoreStartup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitBeforeScene()
        {
            SpawnResourcesObject<GlobalScreenBlocker>();
            SpawnResourcesObject<GlobalScreenLoader>();
            SpawnResourcesObject<CoroutineRunner>();
        }
    
        private static void SpawnResourcesObject<T>() where T : Object
        {
            var path = typeof(T).Name;
            var prefab = Resources.Load<T>(path);
            Object.Instantiate(prefab);
        }
    }
}
