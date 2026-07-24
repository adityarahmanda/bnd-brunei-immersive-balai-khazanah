using UnityEngine;
using UnityEngine.UI;

namespace LZY.Flipbook
{
    public class ImageFlipbookAnimator : FlipbookAnimator
    {
        [SerializeField] private Image image;

        public Image Image => image;

        private void Update()
        {
            var frame = GetUpdateFrame();
            if (frame != null)
                image.sprite = frame;
        }
    }
}