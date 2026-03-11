#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Maui.Animations
{
    /// <summary>
    /// Represents an animation that can be composed of child animations,
    /// supports easing functions, and can repeat or auto-reverse.
    /// </summary>
    public class Animation : IDisposable, IEnumerable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Animation"/>.
        /// </summary>
        public Animation()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Animation"/> with the given parameters.
        /// </summary>
        /// <param name="callback">Action invoked on each tick of the animation.</param>
        /// <param name="start">Delay (seconds) before the animation starts.</param>
        /// <param name="duration">Duration (seconds) of the animation.</param>
        /// <param name="easing">Easing function applied to this animation.</param>
        /// <param name="finished">Action invoked when the animation finishes.</param>
        public Animation(Action<double> callback, double start = 0.0, double duration = 1.0, Easing? easing = null, Action? finished = null)
        {
            StartDelay = start;
            Duration = duration;
            Finished = finished;
            Easing = easing ?? Easing.Default;
            Step = callback;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Animation"/> with child animations.
        /// </summary>
        /// <param name="animations">List of child animations.</param>
        public Animation(List<Animation> animations)
        {
            childrenAnimations = animations;
        }

        internal WeakReference<IAnimator>? Parent { get; set; }

        /// <summary>
        /// Action invoked when the animation finishes.
        /// </summary>
        public Action? Finished { get; set; }

        /// <summary>
        /// Action invoked on each tick with current progress.
        /// </summary>
        public Action<double>? Step { get; set; }

        private bool _paused;
        private int _usingResource;
        private double _skippedSeconds;

        /// <summary>
        /// Indicates whether the animation is paused.
        /// </summary>
        public bool IsPaused => _paused;

        /// <summary>
        /// Child animations.
        /// </summary>
        protected List<Animation> childrenAnimations = new();

        /// <summary>
        /// Name of the animation.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Delay (seconds) before the animation starts.
        /// </summary>
        public double StartDelay { get; set; }

        /// <summary>
        /// Duration (seconds) of the animation.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Current timestamp (seconds) of the animation.
        /// </summary>
        public double CurrentTime { get; protected set; }

        /// <summary>
        /// Current progress of the animation (0–1).
        /// </summary>
        public double Progress { get; protected set; }

        /// <summary>
        /// Easing function applied to the animation.
        /// </summary>
        public Easing Easing { get; set; } = Easing.Default;

        /// <summary>
        /// Indicates whether the animation has finished.
        /// </summary>
        public bool HasFinished { get; protected set; }

        /// <summary>
        /// Indicates whether the animation should repeat.
        /// </summary>
        public bool Repeats { get; set; }

        /// <summary>
        /// Provides an enumerator for child animations.
        /// </summary>
        public IEnumerator GetEnumerator() => childrenAnimations.GetEnumerator();

        /// <summary>
        /// Adds a child animation with specified timing.
        /// </summary>
        /// <param name="beginAt">Start time (0–1) relative to parent animation.</param>
        /// <param name="duration">Duration (0–1) relative to parent animation.</param>
        /// <param name="animation">Animation to add.</param>
        public void Add(double beginAt, double duration, Animation animation)
        {
            if (beginAt < 0 || beginAt > 1) throw new ArgumentOutOfRangeException(nameof(beginAt));
            if (duration < 0 || duration > 1) throw new ArgumentOutOfRangeException(nameof(duration));
            if (duration <= beginAt) throw new ArgumentException($"{nameof(duration)} must be greater than {nameof(beginAt)}");

            animation.StartDelay = beginAt;
            animation.Duration = duration;
            childrenAnimations.Add(animation);
        }

        /// <summary>
        /// Advances the animation by the specified milliseconds.
        /// </summary>
        public void Tick(double milliseconds)
        {
            if (_paused)
                return;

            if (0 == Interlocked.Exchange(ref _usingResource, 1))
            {
                try
                {
                    OnTick(_skippedSeconds + milliseconds);
                    _skippedSeconds = 0;
                }
                finally
                {
                    Interlocked.Exchange(ref _usingResource, 0);
                }
            }
            else
            {
                _skippedSeconds += milliseconds;
            }
        }

        /// <summary>
        /// Animation manager reference.
        /// </summary>
        public IAnimationManager? AnimationManager => animationManger;

        /// <summary>
        /// Protected animation manager reference for subclasses.
        /// </summary>
        protected IAnimationManager? animationManger;

        /// <summary>
        /// Updates this animation and its children.
        /// </summary>
        protected virtual void OnTick(double millisecondsSinceLastUpdate)
        {
            if (HasFinished) return;

            CurrentTime += millisecondsSinceLastUpdate / 1000.0;

            if (childrenAnimations.Count > 0)
            {
                bool allFinished = true;
                foreach (var child in childrenAnimations)
                {
                    child.OnTick(millisecondsSinceLastUpdate);
                    if (!child.HasFinished)
                        allFinished = false;
                }
                HasFinished = allFinished;
            }
            else
            {
                if (CurrentTime < StartDelay)
                    return;

                double percent = Math.Min((CurrentTime - StartDelay) / Duration, 1);
                Update(percent);
            }

            if (HasFinished)
            {
                Finished?.Invoke();
                if (Repeats) Reset();
            }
        }

        /// <summary>
        /// Updates the animation progress.
        /// </summary>
        public virtual void Update(double percent)
        {
            try
            {
                Progress = Easing.Ease(percent);
                Step?.Invoke(Progress);
                HasFinished = percent == 1;
            }
            catch
            {
                HasFinished = true;
            }
        }

        /// <summary>
        /// Sets the animation manager and registers this animation.
        /// </summary>
        public void Commit(IAnimationManager animationManger)
        {
            this.animationManger = animationManger;
            animationManger.Add(this);
        }

        /// <summary>
        /// Creates a parent animation with auto-reversing behavior.
        /// </summary>
        public Animation CreateAutoReversing()
        {
            var reversed = CreateReverse();
            return new Animation
            {
                Duration = reversed.StartDelay + reversed.Duration,
                Repeats = Repeats,
                childrenAnimations = { this, reversed }
            };
        }

        /// <summary>
        /// Creates a reversed version of this animation.
        /// </summary>
        protected virtual Animation CreateReverse()
        {
            var reversedChildren = childrenAnimations.ToList();
            reversedChildren.Reverse();

            return new Animation
            {
                Easing = Easing,
                Duration = Duration,
                StartDelay = StartDelay + Duration,
                childrenAnimations = reversedChildren
            };
        }

        /// <summary>
        /// Resets this animation and all child animations.
        /// </summary>
        public virtual void Reset()
        {
            CurrentTime = 0;
            HasFinished = false;
            foreach (var child in childrenAnimations)
                child.Reset();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            _paused = true;
            animationManger?.Remove(this);
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        public void Resume()
        {
            _paused = false;
            animationManger?.Add(this);
        }

        /// <summary>
        /// Removes this animation from its parent.
        /// </summary>
        public void RemoveFromParent()
        {
            if (Parent?.TryGetTarget(out var view) ?? false)
                view.RemoveAnimation(this);
        }

        private bool _disposedValue;

        /// <summary>
        /// Indicates whether this animation has been disposed.
        /// </summary>
        public bool IsDisposed => _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var child in childrenAnimations)
                        child.Dispose();
                    childrenAnimations.Clear();
                }

                _disposedValue = true;
                animationManger?.Remove(this);
                Finished = null;
                Step = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Forces the animation to complete immediately.
        /// </summary>
        internal virtual void ForceFinish()
        {
            if (Progress < 1.0)
                Update(1.0);
        }
    }
}
