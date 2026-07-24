using UnityEngine;

namespace LZY.Flipbook
{
    public class SpriteRendererFlipbookAnimator : FlipbookAnimator
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public SpriteRenderer SpriteRenderer => spriteRenderer;

        private void Update()
        {
            var frame = GetUpdateFrame();
            if (frame != null)
                spriteRenderer.sprite = frame;
        }
    }
}