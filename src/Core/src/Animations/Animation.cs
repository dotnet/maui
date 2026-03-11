using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Maui.Animations
{
	/// <summary>
	/// Represents an animation.
	/// </summary>
	public class Animation : IDisposable, IEnumerable
	{
		readonly List<Animation> childrenAnimations = new();

		internal WeakReference<IAnimator>? Parent { get; set; }

		Easing _easing = Easing.Default;
		bool _paused;
		double _skippedSeconds;
		int _usingResource;
		IAnimationManager? animationManager;
		bool _disposedValue;

		/// <summary>
		/// Instantiate a new <see cref="Animation"/> object.
		/// </summary>
		public Animation() { }

		/// <summary>
		/// Instantiate a new <see cref="Animation"/> object with the given parameters.
		/// </summary>
		public Animation(Action<double> step, double start = 0, double duration = 1, Easing? easing = null, Action? finished = null)
		{
			Step = step ?? throw new ArgumentNullException(nameof(step));
			StartDelay = start;
			Duration = duration;
			Easing = easing ?? Easing.Default;
			Finished = finished;
		}

		/// <summary>
		/// Instantiate a new <see cref="Animation"/> object that consists of the given list of child animations.
		/// </summary>
		public Animation(List<Animation> animations)
		{
			childrenAnimations = animations ?? throw new ArgumentNullException(nameof(animations));
		}

		/// <summary>
		/// A callback invoked when this animation finishes.
		/// </summary>
		public Action? Finished { get; set; }

		/// <summary>
		/// A callback invoked after each tick of this animation.
		/// </summary>
		public Action<double>? Step { get; set; }

		/// <summary>
		/// Specifies whether this animation is currently paused.
		/// </summary>
		public bool IsPaused => _paused;

		/// <summary>
		/// The name of this animation.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// The delay in seconds before the animation starts.
		/// </summary>
		public double StartDelay { get; set; }

		/// <summary>
		/// Duration in seconds.
		/// </summary>
		public double Duration { get; set; }

		/// <summary>
		/// Current timestamp in seconds.
		/// </summary>
		public double CurrentTime { get; protected set; }

		/// <summary>
		/// Progress percentage.
		/// </summary>
		public double Progress { get; protected set; }

		/// <summary>
		/// Easing function applied.
		/// </summary>
		public Easing Easing
		{
			get => _easing;
			set => _easing = value ?? Easing.Default;
		}

		/// <summary>
		/// Specifies whether this animation has finished.
		/// </summary>
		public bool HasFinished { get; protected set; }

		/// <summary>
		/// Specifies whether this animation should repeat.
		/// </summary>
		public bool Repeats { get; set; }

		/// <summary>
		/// Enumerator for child animations.
		/// </summary>
		public IEnumerator GetEnumerator() => childrenAnimations.GetEnumerator();

		/// <summary>
		/// Adds a child animation.
		/// </summary>
		public void Add(double beginAt, double duration, Animation animation)
		{
			if (animation is null)
				throw new ArgumentNullException(nameof(animation));

			if (beginAt < 0 || beginAt > 1)
				throw new ArgumentOutOfRangeException(nameof(beginAt));

			if (duration < 0 || duration > 1)
				throw new ArgumentOutOfRangeException(nameof(duration));

			if (duration <= beginAt)
				throw new ArgumentException($"{nameof(duration)} must be greater than {nameof(beginAt)}");

			animation.StartDelay = beginAt;
			animation.Duration = duration;
			childrenAnimations.Add(animation);
		}

		/// <summary>
		/// Updates animation state.
		/// </summary>
		public void Tick(double milliseconds)
		{
			if (IsPaused || _disposedValue)
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
		/// Manager reference.
		/// </summary>
		public IAnimationManager? AnimationManager => animationManager;

		/// <summary>
		/// Performs the tick logic.
		/// </summary>
		protected virtual void OnTick(double millisecondsSinceLastUpdate)
		{
			if (HasFinished)
				return;

			var seconds = millisecondsSinceLastUpdate / 1000.0;
			CurrentTime += seconds;

			if (childrenAnimations.Count > 0)
			{
				var finished = true;
				foreach (var child in childrenAnimations)
				{
					child.OnTick(millisecondsSinceLastUpdate);
					if (!child.HasFinished)
						finished = false;
				}
				HasFinished = finished;
			}
			else if (CurrentTime >= StartDelay)
			{
				var percent = Math.Min((CurrentTime - StartDelay) / Duration, 1);
				Update(percent);
			}

			if (HasFinished)
			{
				Finished?.Invoke();
				if (Repeats)
					Reset();
			}
		}

		/// <summary>
		/// Updates progress and calls Step.
		/// </summary>
		public virtual void Update(double percent)
		{
			try
			{
				Progress = Easing.Ease(percent);
				Step?.Invoke(Progress);
				HasFinished = percent >= 1;
			}
			catch
			{
				HasFinished = true;
			}
		}

		/// <summary>
		/// Assigns animation manager.
		/// </summary>
		public void Commit(IAnimationManager manager)
		{
			animationManager = manager ?? throw new ArgumentNullException(nameof(manager));
			manager.Add(this);
		}

		/// <summary>
		/// Creates an auto-reversing animation.
		/// </summary>
		public Animation CreateAutoReversing()
		{
			var reversed = CreateReverse();
			var parent = new Animation
			{
				Duration = reversed.StartDelay + reversed.Duration,
				Repeats = Repeats,
				childrenAnimations =
				{
					this,
					reversed
				}
			};
			Repeats = false;
			return parent;
		}

		/// <summary>
		/// Creates reversed animation.
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
		/// Resets animation state.
		/// </summary>
		public virtual void Reset()
		{
			CurrentTime = 0;
			HasFinished = false;
			foreach (var child in childrenAnimations)
				child.Reset();
		}

		public void Pause()
		{
			_paused = true;
			animationManager?.Remove(this);
		}

		public void Resume()
		{
			_paused = false;
			animationManager?.Add(this);
		}

		public void RemoveFromParent()
		{
			if (Parent?.TryGetTarget(out var view) ?? false)
				view?.RemoveAnimation(this);
		}

		public bool IsDisposed => _disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (_disposedValue)
				return;

			if (disposing)
			{
				foreach (var child in childrenAnimations)
					child.Dispose();
				childrenAnimations.Clear();
			}

			_disposedValue = true;
			animationManager?.Remove(this);
			Finished = null;
			Step = null;
		}

		public void Dispose() => Dispose(true);

		internal virtual void ForceFinish()
		{
			if (Progress < 1)
				Update(1);
		}
	}
}
