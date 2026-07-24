using System;
using System.Threading;
using System.Threading.Tasks;
using Lean.Pool;
using LZY.Flipbook;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LZY.BND
{
    public class ButterflyParticle : MonoBehaviour
    {
        [SerializeField] private SpriteRendererFlipbookAnimator animator;

        private CancellationTokenSource _cts;
        
        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            _ = PlayAnimAsync(_cts.Token);
        }

        private void OnDisable()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task PlayAnimAsync(CancellationToken cancellationToken = default)
        {
            var random = Random.Range(0, animator.animations.Length);
            var duration = animator.animations[random].Duration;
            animator.Play(random);
            await Task.Delay(TimeSpan.FromSeconds(duration), cancellationToken);
            animator.Stop();
            LeanPool.Despawn(gameObject);
        }

        public void SetFlippedX(bool value)
        {
            animator.SpriteRenderer.flipX = value;
        }
    }
}