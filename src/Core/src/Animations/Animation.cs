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
		/// <summary>
		/// Instantiate a new <see cref="Animation"/> object.
		/// </summary>
		public Animation()
		{

		}

		/// <summary>
		/// Instantiate a new <see cref="Animation"/> object with the given parameters.
		/// </summary>
		/// <param name="callback">The <see cref="Action{T}"/> that is invoked after each tick of this animation.</param>
		/// <param name="start">Specifies a delay (in seconds) taken into account before the animation starts.</param>
		/// <param name="duration">Specifies the duration that this animation should take in seconds.</param>
		/// <param name="easing">The easing function to apply to this animation.</param>
		/// <param name="finished">A callback <see cref="Action{T}"/> that is invoked after the animation has finished.</param>
		public Animation(Action<double> callback, double start = 0.0f, double duration = 1.0f, Easing? easing = null, Action? finished = null)
		{
			StartDelay = start;
			Duration = duration;
			Finished = finished;
			Easing = easing ?? Easing.Default;
			Step = callback;

		}

		/// <summary>
		/// Instantiate a new <see cref="Animation"/> object that consists of the given list of child animations.
		/// </summary>
		/// <param name="animations">A <see cref="List{T}"/> that contains <see cref="Animation"/> objects that will be children of the newly instantiated animation.</param>
		public Animation(List<Animation> animations)
		{
			childrenAnimations = animations;
		}

		internal WeakReference<IAnimator>? Parent { get; set; }

		/// <summary>
		/// A callback that is invoked when this animation finishes.
		/// </summary>
		public Action? Finished { get; set; }

		/// <summary>
		/// A callback that is invoked after each tick of this animation.
		/// </summary>
		public Action<double>? Step { get; set; }

		bool _paused;

		/// <summary>
		/// Specifies whether this animation is currently paused.
		/// </summary>
		public bool IsPaused => _paused;

		/// <summary>
		/// Collection of child animations associated to this animation.
		/// </summary>
		protected List<Animation> childrenAnimations = new();

		/// <summary>
		/// The name of this animation.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// The delay (in seconds) taken into account before the animation starts.
		/// </summary>
		public double StartDelay { get; set; }

		/// <summary>
		/// The duration of this animation in seconds.
		/// </summary>
		public double Duration { get; set; }

		/// <summary>
		/// The current timestamp (in seconds) of the animation.
		/// </summary>
		public double CurrentTime { get; protected set; }

		/// <summary>
		/// Progress of this animation in percentage.
		/// </summary>
		public double Progress { get; protected set; }

		/// <summary>
		/// The <see cref="Easing"/> function that is applied to this animation.
		/// </summary>
		public Easing Easing { get; set; } = Easing.Default;

		/// <summary>
		/// Specifies whether this animation has finished.
		/// </summary>
		public bool HasFinished { get; protected set; }

		/// <summary>dotnet_analyzer_diagnostic.CA1805.severity = none
		/// Specifies whether this animation should repeat.
		/// </summary>
		public bool Repeats { get; set; }

		double _skippedSeconds;
		int _usingResource;

		/// <summary>
		/// Provides an <see cref="IEnumerator"/> of the child animations.
		/// </summary>
		/// <returns><see cref="IEnumerator"/> of <see cref="Animation"/></returns>
		public IEnumerator GetEnumerator() => childrenAnimations.GetEnumerator();

		/// <summary>
		/// Adds a new child animation to this animation with the specified parameters.
		/// </summary>
		/// <param name="beginAt">Specifies a delay (in seconds) taken into account before the added child animation starts.</param>
		/// <param name="duration">Specifies the duration (in seconds) that the added child animation should take.</param>
		/// <param name="animation">The <see cref="Animation"/> object to add to this animation as a child.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="beginAt"/> or <paramref name="duration"/> is less than 0 or more than 1.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="duration"/> is less than or equal to <paramref name="beginAt"/>.</exception>
		public void Add(double beginAt, double duration, Animation animation)
		{
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
		/// Method to trigger an update for this animation.
		/// </summary>
		/// <param name="milliseconds">Number of milliseconds that have passed since the last tick.</param>
		public void Tick(double milliseconds)
		{
			if (IsPaused)
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
					//Release the lock
					Interlocked.Exchange(ref _usingResource, 0);
				}
			}
			//animation is lagging behind!
			else
			{
				_skippedSeconds += milliseconds;
			}
		}

		/// <summary>
		/// A reference to the <see cref="IAnimationManager"/> that manages this animation.
		/// </summary>
		public IAnimationManager? AnimationManager => animationManger;

		/// <summary>
		/// A reference to the <see cref="IAnimationManager"/> that manages this animation.
		/// </summary>
		protected IAnimationManager? animationManger;

		/// <summary>
		/// Executes the logic to update all animations within this animation.
		/// </summary>
		/// <param name="millisecondsSinceLastUpdate">Number of milliseconds that have passed since the last tick.</param>
		protected virtual void OnTick(double millisecondsSinceLastUpdate)
		{
			if (HasFinished)
				return;

			var secondsSinceLastUpdate = millisecondsSinceLastUpdate / 1000.0;
			CurrentTime += secondsSinceLastUpdate;

			if (childrenAnimations.Any())
			{
				var hasFinished = true;
				foreach (var animation in childrenAnimations)
				{

					animation.OnTick(millisecondsSinceLastUpdate);
					if (!animation.HasFinished)
						hasFinished = false;

				}

				HasFinished = hasFinished;
			}
			else
			{
				var start = CurrentTime - StartDelay;

				if (CurrentTime < StartDelay)
					return;

				var percent = Math.Min(start / Duration, 1);
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
		/// Updates this animation by updating <see cref="Progress"/> and invoking <see cref="Step"/>.
		/// </summary>
		/// <param name="percent">Progress of this animation in percentage.</param>
		public virtual void Update(double percent)
		{
			try
			{
				Progress = Easing.Ease(percent);
				Step?.Invoke(Progress);
				HasFinished = percent == 1;
			}
			catch (Exception)
			{
				HasFinished = true;
			}
		}

		/// <summary>
		/// Sets the <see cref="IAnimationManager"/> for this animation.
		/// </summary>
		/// <param name="animationManger">Reference to the <see cref="IAnimationManager"/> that will manage this animation.</param>
		public void Commit(IAnimationManager animationManger)
		{
			this.animationManger = animationManger;
			animationManger.Add(this);
		}

		/// <summary>
		/// Creates an animation that includes both the original animation and a reversed version of the same animation.
		/// </summary>
		/// <returns>An <see cref="Animation"/> object with the original animation and the reversed animation.</returns>
		/// <remarks>You can get just the reversed animation by using <see cref="CreateReverse"/>.</remarks>
		public Animation CreateAutoReversing()
		{
			var reversedChildren = childrenAnimations.ToList();
			reversedChildren.Reverse();
			var reversed = CreateReverse();

			var parentAnimation = new Animation
			{
				Duration = reversed.StartDelay + reversed.Duration,
				Repeats = Repeats,
				childrenAnimations =
			{
				this,
				reversed,
			}
			};

			Repeats = false;
			return parentAnimation;
		}

		/// <summary>
		/// Creates a reversed version of the current animation, including reversing the child animations.
		/// </summary>
		/// <returns>An <see cref="Animation"/> object that is the reversed version of this animation.</returns>
		/// <remarks>You can the forward and reverse animation by using <see cref="CreateAutoReversing"/>.</remarks>
		protected virtual Animation CreateReverse()
		{
			var reversedChildren = childrenAnimations.ToList();
			reversedChildren.Reverse();

			return new Animation
			{
				Easing = Easing,
				Duration = Duration,
				StartDelay = StartDelay + Duration,
				childrenAnimations = reversedChildren,
			};
		}

		/// <summary>
		/// Resets the animation (and all child animations) to its initial state.
		/// </summary>
		public virtual void Reset()
		{
			CurrentTime = 0;
			HasFinished = false;

			foreach (var x in childrenAnimations)
				x.Reset();
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
		/// Removes this animation from it's parent.
		/// If there is no parent, nothing will happen.
		/// </summary>
		public void RemoveFromParent()
		{
			IAnimator? view = null;
			if (Parent?.TryGetTarget(out view) ?? false)
				view?.RemoveAnimation(this);
		}

		/// <summary>
		/// Gets a value that specifies if this animation has been disposed.
		/// </summary>
		public bool IsDisposed => _disposedValue;

		private bool _disposedValue; // To detect redundant calls

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
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}

		internal virtual void ForceFinish()
		{
			if (Progress < 1.0)
			{
				Update(1.0);
			}
		}
	}
}