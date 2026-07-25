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

        private void OnEnable()
        {
            animator.SpriteRenderer.sprite = null;
            var random = Random.Range(0, animator.animations.Length);
            animator.Play(random);
        }

        private void OnDisable()
        {
            animator.SpriteRenderer.sprite = null;
        }

        private void Update()
        {
            if (!animator.isPlaying) 
                LeanPool.Despawn(gameObject);
        }

        public void SetFlippedX(bool value)
        {
            animator.SpriteRenderer.flipX = value;
        }
    }
}