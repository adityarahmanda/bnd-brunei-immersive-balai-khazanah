using Sirenix.OdinInspector;
using UnityEngine;

namespace LZY.Flipbook
{
    [CreateAssetMenu(menuName = "LZY/Flipbook/Animation", fileName = "Animation")]
    public class FlipbookAnimation : ScriptableObject
    {
        public PlayType playType = PlayType.ByFrameRate;
        
        public float Duration
        {
            get
            {
                if (frames.Length == 0)
                    return 0f;

                return playType switch
                {
                    PlayType.ByDuration => duration,
                    PlayType.ByFrameRate => frames.Length / frameRate,
                    _ => 0f
                };
            }
        }
        
        [ShowIf(nameof(playType), PlayType.ByDuration)]
        [SerializeField] private float duration = 1f;
        
        [ShowIf(nameof(playType), PlayType.ByFrameRate)]
        public float frameRate = 24f;
        public bool loop;
        
        public Sprite[] frames;
    }
    
    public enum PlayType
    {
        ByFrameRate,
        ByDuration
    }
}