using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZY
{
    [DefaultExecutionOrder(-50)]
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        private static List<Coroutine> _coroutines;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(this);
        }

        public static Coroutine StartNewCoroutine(IEnumerator routine)
        {
            return _instance.StartCoroutine(routine);
        }

        public static bool HasCoroutine(Coroutine coroutine)
        {
            foreach (var coroutineItem in _coroutines)
            {
                if (coroutine == coroutineItem)
                    return true;
            }

            return false;
        }

        public static void StopRunningCoroutine(Coroutine coroutine)
        {
            _instance.StopCoroutine(coroutine);
        }

        public static void StopAllRunningCoroutines()
        {
            _instance.StopAllCoroutines();
        }
    }
}
