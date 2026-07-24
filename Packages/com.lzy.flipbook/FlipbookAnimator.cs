using Sirenix.OdinInspector;
using UnityEngine;

namespace LZY.Flipbook
{
    public abstract class FlipbookAnimator : MonoBehaviour
    {
        public bool isPlaying = false;
        public FlipbookAnimation[] animations;
        public float playbackSpeed = 1f;

        public int CurrentFrameIndex => _currentFrameIndex;
        protected int _currentFrameIndex = 0;
        protected Sprite _currentFrameSprite;
        protected float _frameTimer = 0f;

        public FlipbookAnimation CurrentAnimation => _currentAnimation;
        protected FlipbookAnimation _currentAnimation;

        private void Awake()
        {
            if (isPlaying)
                Play(0);
            OnAwake();
        }

        protected virtual void OnAwake() { }

        protected Sprite GetUpdateFrame()
        {
            if (!isPlaying || _currentAnimation == null)
                return null;

            _frameTimer += Time.deltaTime * playbackSpeed;
            var frameTime = _currentAnimation.playType == PlayType.ByFrameRate
                ? 1f / _currentAnimation.frameRate
                : _currentAnimation.Duration / _currentAnimation.frames.Length;

            if (_frameTimer >= frameTime)
            {
                _frameTimer = 0f;
                _currentFrameIndex++;

                if (_currentFrameIndex >= _currentAnimation.frames.Length)
                {
                    if (_currentAnimation.loop)
                    {
                        _currentFrameIndex = 0;
                    }
                    else
                    {
                        Stop();
                        return null;
                    }
                }

                _currentFrameSprite = _currentAnimation.frames[_currentFrameIndex];
            }

            return _currentFrameSprite;
        }

        public void Play(int animationIndex)
        {
            if (animationIndex < 0 || animationIndex >= animations.Length)
            {
                Debug.LogWarning("Animation index out of range.");
                return;
            }

            Play(animations[animationIndex]);
        }

        public void Play(string animationName)
        {
            FlipbookAnimation selectedAnimation = null;
            foreach (var animation in animations)
            {
                if (animation.name == animationName)
                {
                    selectedAnimation = animation;
                    break;
                }
            }

            if (selectedAnimation == null)
            {
                Debug.LogWarning("Animation not found.");
                return;
            }

            Play(selectedAnimation);
        }

        public void Play(FlipbookAnimation animation)
        {
            if (animation == null)
            {
                Debug.LogWarning("No animation assigned.");
                return;
            }

            _currentFrameIndex = 0;
            _frameTimer = 0f;
            _currentAnimation = animation;

            if (animation.frames.Length > 0)
            {
                _currentFrameSprite = animation.frames[0];
            }
            
            OnPlay();
            isPlaying = true;
        }

        public void Resume()
        {
            if (isPlaying) return;

            _frameTimer = 0f;
            OnResume();
            isPlaying = true;
        }

        public void Stop()
        {
            if (!isPlaying) return;

            OnStop();
            isPlaying = false;
        }

        protected virtual void OnPlay() { }

        protected virtual void OnResume() { }

        protected virtual void OnStop() { }
    }
}