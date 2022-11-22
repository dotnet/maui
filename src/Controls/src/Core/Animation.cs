using System;
using System.Collections;
using System.Collections.Generic;
using BaseAnimation = Microsoft.Maui.Animations.Animation;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Encapsulates an animation, a collection of functions that modify properties over a user-perceptible time period.
	/// </summary>
	public class Animation : BaseAnimation
	{
		bool _finishedTriggered;

		/// <summary>
		/// Creates a new <see cref="Animation" /> object with default values.
		/// </summary>
		public Animation()
		{
			Easing = Easing.Linear;
		}

		/// <summary>
		/// Creates a new <see cref = "Animation" /> object with the specified parameters.
		/// </summary>
		/// <param name="callback">An action that is called with successive animation values.</param>
		/// <param name="start"> The fraction into the current animation at which to start the animation.</param>
		/// <param name="end"> The fraction into the current animation at which to end the animation.</param>
		/// <param name="easing"> The easing function to use to transition in, out, or in and out of the animation.</param>
		/// <param name="finished"> An action to call when the animation is finished.</param>
		public Animation(Action<double> callback, double start = 0.0f, double end = 1.0f, Easing easing = null, Action finished = null) : base(callback, start, end - start, easing, finished)
		{

			Easing = easing ?? Easing.Linear;
			Func<double, double> transform = AnimationExtensions.Interpolate(start, end);
			Step = f => callback(transform(f));
		}
		/// <summary>
		/// Adds an <see cref="Animation"/> object to this <see cref="Animation"/> that begins at <paramref name="beginAt"/> and finishes at <paramref name="finishAt"/>.
		/// </summary>
		/// <param name="beginAt">The fraction into this animation at which the added child animation will begin animating.</param>
		/// <param name="finishAt">The fraction into this animation at which the added child animation will stop animating.</param>
		/// <param name="animation">The animation to add.</param>
		public void Add(double beginAt, double finishAt, Animation animation)
		{
			if (beginAt < 0 || beginAt > 1)
				throw new ArgumentOutOfRangeException("beginAt");

			if (finishAt < 0 || finishAt > 1)
				throw new ArgumentOutOfRangeException("finishAt");

			if (finishAt <= beginAt)
				throw new ArgumentException("finishAt must be greater than beginAt");

			animation.StartDelay = beginAt;
			animation.Duration = finishAt - beginAt;
			childrenAnimations.Add(animation);
		}

		/// <summary>
		/// Runs the <paramref name="owner" /> animation with the supplied parameters.
		/// </summary>
		/// <param name="owner">The owning animation that will be animated.</param>
		/// <param name="name">The name, or handle, that is used to access and track the animation and its state.</param>
		/// <param name="rate">The time, in milliseconds, between frames.</param>
		/// <param name="length">The number of milliseconds over which to interpolate the animation.</param>
		/// <param name="easing">The easing function to use to transition in, out, or in and out of the animation.</param>
		/// <param name="finished">An action to call when the animation is finished.</param>
		/// <param name="repeat">A function that should return true if the animation should continue.</param>
		public void Commit(IAnimatable owner, string name, uint rate = 16, uint length = 250, Easing easing = null, Action<double, bool> finished = null, Func<bool> repeat = null)
		{
			owner.Animate(name, this, rate, length, easing, finished, repeat);
		}

		/// <summary>
		/// Returns a callback that recursively runs the eased animation step on this <see cref="Animation" /> object and those of its children that have begun and not finished.
		/// </summary>
		/// <returns>A callback that recursively runs the eased animation step on this <see cref="Animation" /> object and those of its children that have begun and not finished.</returns>
		public Action<double> GetCallback()
		{
			Action<double> result = f =>
			{
				Step?.Invoke(Easing.Ease(f));
				foreach (Animation animation in childrenAnimations)
				{
					if (animation._finishedTriggered)
						continue;

					double val = Math.Max(0.0f, Math.Min(1.0f, (f - animation.StartDelay) / (animation.Duration)));

					if (val <= 0.0f) // not ready to process yet
						continue;

					Action<double> callback = animation.GetCallback();
					callback(val);

					if (val >= 1.0f)
					{
						animation._finishedTriggered = true;
						animation.Finished?.Invoke();
					}
				}
			};
			return result;
		}

		internal void ResetChildren() => this.Reset();

		public override void Reset()
		{
			base.Reset();
			_finishedTriggered = false;
		}

		/// <summary>
		/// Adds an <see cref="Animation" /> object to this <see cref="Animation" /> that begins at <paramref name="beginAt" /> and finishes at <paramref name="finishAt" />.
		/// </summary>
		/// <param name="beginAt">The fraction into this animation at which the added child animation will begin animating.</param>
		/// <param name="finishAt">The fraction into this animation at which the added child animation will stop animating.</param>
		/// <param name="animation">The animation to add.</param>
		public Animation Insert(double beginAt, double finishAt, Animation animation)
		{
			Add(beginAt, finishAt, animation);
			return this;
		}

		/// <summary>
		/// Adds <paramref name="animation" /> to the children of this <see cref="Animation" /> object and sets the start and end times of <paramref name="animation" /> to <paramref name="beginAt" /> and <paramref name="finishAt" />, respectively.
		/// </summary>
		/// <param name="animation">The animation to add.</param>
		/// <param name="beginAt">The fraction into this animation at which the added child animation will begin animating.</param>
		/// <param name="finishAt">The fraction into this animation at which the added child animation will stop animating.</param>
		public Animation WithConcurrent(Animation animation, double beginAt = 0.0f, double finishAt = 1.0f)
		{
			animation.StartDelay = beginAt;
			animation.Duration = finishAt - beginAt;
			childrenAnimations.Add(animation);
			return this;
		}

		/// <summary>
		/// Creates a new <see cref="Animation" /> object with the specified <paramref name="callback" />, and adds it to the children of this <see cref="Animation" /> object.
		/// </summary>
		/// <param name="callback">An action that is called with successive animation values.</param>
		/// <param name="start">The fraction into the current animation at which to start the animation.</param>
		/// <param name="end">The fraction into the current animation at which to end the animation.</param>
		/// <param name="easing">The easing function to use to transition in, out, or in and out of the animation.</param>
		/// <param name="beginAt">The fraction into this animation at which the added child animation will begin animating.</param>
		/// <param name="finishAt">The fraction into this animation at which the added child animation will stop animating.</param>
		public Animation WithConcurrent(Action<double> callback, double start = 0.0f, double end = 1.0f, Easing easing = null, double beginAt = 0.0f, double finishAt = 1.0f)
		{
			var child = new Animation(callback, start, end, easing);
			child.StartDelay = beginAt;
			child.Duration = finishAt - beginAt;
			childrenAnimations.Add(child);
			return this;
		}

		/// <summary>
		/// Specifies if this animation is currently enabled.
		/// </summary>
		public bool IsEnabled
		{
			get
			{
				return animationManger?.Ticker?.SystemEnabled ?? false;
			}
		}
	}
}
