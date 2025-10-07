#nullable disable
//
// Tweener.cs
//
// Author:
//       Jason Smith <jason.smith@xamarin.com>
//
// Copyright (c) 2012 Microsoft.Maui.Controls Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	/// <summary>Extension methods for <see cref="Microsoft.Maui.Controls.IAnimatable"/> objects.</summary>
	public static class AnimationExtensions
	{
		// We use a ConcurrentDictionary because Tweener relies on being able to remove
		// animations from the AnimationManager within its finalizer (via the Remove extension
		// method below). Since finalization occurs on a different thread, it risks crashes when
		// the finalizer is running at the same time another animation is finsihing and removing
		// itself from this dictionary. So until we can change that design, this dictionary must
		// be thread-safe.
		static readonly ConcurrentDictionary<int, Animation> s_tweeners;

		static readonly Dictionary<AnimatableKey, Info> s_animations;
		static readonly Dictionary<AnimatableKey, int> s_kinetics;

		/// <summary>
		/// This property is used for UnitTest 
		/// </summary>
		static internal int TweenersCounter => s_tweeners.Count;

		static int s_currentTweener = 1;

		static AnimationExtensions()
		{
			s_animations = new Dictionary<AnimatableKey, Info>();
			s_kinetics = new Dictionary<AnimatableKey, int>();
			s_tweeners = new ConcurrentDictionary<int, Animation>();
		}

		public static int Add(this IAnimationManager animationManager, Action<double> step)
		{
			var id = s_currentTweener++;
			var animation = new Animation
			{
				Name = $"{id}",
				Easing = Easing.Linear,
				Step = step
			};
			s_tweeners[id] = animation;
			animation.Commit(animationManager);

			animation.Finished += () =>
			{
				s_tweeners.TryRemove(id, out _);
				animation.Finished = null;
			};
			return id;
		}

		public static int Insert(this IAnimationManager animationManager, Func<long, bool> step)
		{
			var id = s_currentTweener++;
			Animation animation = null;
			animation = new TweenerAnimation(step)
			{
				Name = $"{id}",
				Easing = Easing.Linear,
			};
			s_tweeners[id] = animation;
			animation.Commit(animationManager);

			animation.Finished += () =>
			{
				s_tweeners.TryRemove(id, out _);
				animation.Finished = null;
			};

			return id;
		}

		public static void Remove(this IAnimationManager animationManager, int tickerId)
		{
			if (s_tweeners.TryRemove(tickerId, out Animation animation))
			{
				animationManager.Remove(animation);
			}
		}

		/// <summary>Stops the animation.</summary>
		/// <param name="self">The object on which this method will be run.</param>
		/// <param name="handle">An animation key that must be unique among its sibling and parent animations for the duration of the animation.</param>
		public static bool AbortAnimation(this IAnimatable self, string handle)
		{
			var key = new AnimatableKey(self, handle);

			if (!s_animations.ContainsKey(key) && !s_kinetics.ContainsKey(key))
			{
				return false;
			}

			Action abort = () =>
			{
				AbortAnimation(key);
				AbortKinetic(key);
			};

			DoAction(self, abort);

			return true;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AnimationExtensions.xml" path="//Member[@MemberName='Animate'][2]/Docs/*" />
		public static void Animate(this IAnimatable self, string name, Animation animation, uint rate = 16, uint length = 250, Easing easing = null, Action<double, bool> finished = null,
								   Func<bool> repeat = null)
		{
			if (repeat == null)
				self.Animate(name, animation.GetCallback(), rate, length, easing, finished, null);
			else
			{
				Func<bool> r = () =>
				{
					var val = repeat();
					if (val)
						animation.ResetChildren();
					return val;
				};
				self.Animate(name, animation.GetCallback(), rate, length, easing, finished, r);
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AnimationExtensions.xml" path="//Member[@MemberName='Animate'][3]/Docs/*" />
		public static void Animate(this IAnimatable self, string name, Action<double> callback, double start, double end, uint rate = 16, uint length = 250, Easing easing = null,
								   Action<double, bool> finished = null, Func<bool> repeat = null)
		{
			self.Animate(name, Interpolate(start, end), callback, rate, length, easing, finished, repeat);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AnimationExtensions.xml" path="//Member[@MemberName='Animate'][1]/Docs/*" />
		public static void Animate(this IAnimatable self, string name, Action<double> callback, uint rate = 16, uint length = 250, Easing easing = null, Action<double, bool> finished = null,
								   Func<bool> repeat = null)
		{
			self.Animate(name, x => x, callback, rate, length, easing, finished, repeat);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AnimationExtensions.xml" path="//Member[@MemberName='Animate&lt;T&gt;'][1]/Docs/*" />
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		public static void Animate<T>(this IAnimatable self, string name, Func<double, T> transform, Action<T> callback,
			uint rate = 16, uint length = 250, Easing easing = null,
			Action<T, bool> finished = null, Func<bool> repeat = null, IAnimationManager animationManager = null)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		{
			if (transform == null)
				throw new ArgumentNullException(nameof(transform));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			animationManager ??= self.GetAnimationManager();

			Action animate = () => AnimateInternal(self, animationManager, name, transform, callback, rate, length, easing, finished, repeat);
			DoAction(self, animate);
		}


		/// <summary>Sets the specified parameters and starts the kinetic animation.</summary>
		/// <param name="self">The object on which this method will be run.</param>
		/// <param name="name">An animation key that should be unique among its sibling and parent animations for the duration of the animation.</param>
		/// <param name="callback">An action that is called with successive animation values.</param>
		/// <param name="velocity">The amount that the animation progresses in each animation step. For example, a velocity of <c>1</c> progresses at the default speed.</param>
		/// <param name="drag">The amount that the progression speed is reduced per frame. Can be negative.</param>
		/// <param name="finished">An action to call when the animation is finished.</param>
		/// <param name="animationManager">The animation manager to use for this animation. If null, the default animation manager for the target object will be used.</param>
		public static void AnimateKinetic(this IAnimatable self, string name, Func<double, double, bool> callback, double velocity, double drag, Action finished = null, IAnimationManager animationManager = null)
		{
			animationManager ??= self.GetAnimationManager();

			Action animate = () => AnimateKineticInternal(self, animationManager, name, callback, velocity, drag, finished);
			DoAction(self, animate);
		}

		/// <summary>Returns a Boolean value that indicates whether or not the animation that is specified by <paramref name="handle"/> is running.</summary>
		/// <param name="self">The object on which this method will be run.</param>
		/// <param name="handle">An animation key that must be unique among its sibling and parent animations for the duration of the animation.</param>
		public static bool AnimationIsRunning(this IAnimatable self, string handle)
		{
			var key = new AnimatableKey(self, handle);
			return s_animations.ContainsKey(key);
		}

		/// <summary>Returns a function that performs a linear interpolation between <paramref name="start"/> and <paramref name="end"/>.</summary>
		/// <param name="start">The fraction into the current animation at which to start the animation.</param>
		/// <param name="end">The fraction into the current animation at which to stop the animation.</param>
		/// <param name="reverseVal">The inverse scale factor to use if <paramref name="reverse"/> is <see langword="true"/>.</param>
		/// <param name="reverse">Whether to use the inverse scale factor in <paramref name="reverseVal"/> to deinterpolate.</param>
		/// <returns>A function that performs a linear interpolation between <paramref name="start"/> and <paramref name="end"/>. Application developers can pass values between 0.0f and 1.0f to this function in order to receive a value that is offset from <paramref name="start"/> or <paramref name="end"/>, depending on the value of <paramref name="reverse"/>, by the passed value times the distance between <paramref name="start"/> and <paramref name="end"/>.</returns>
		/// <remarks>If <paramref name="reverse"/> is <see langword="true"/>, then the interpolation happens between <paramref name="start"/> and <paramref name="reverseVal"/>.</remarks>
		public static Func<double, double> Interpolate(double start, double end = 1.0f, double reverseVal = 0.0f, bool reverse = false)
		{
			double target = reverse ? reverseVal : end;
			return x => start + (target - start) * x;
		}

		/// <param name="self">The object instance.</param>
		public static IDisposable Batch(this IAnimatable self) => new BatchObject(self);

		static void AbortAnimation(AnimatableKey key)
		{
			// If multiple animations on the same view with the same name (IOW, the same AnimatableKey) are invoked
			// asynchronously (e.g., from the `[Animate]To` methods in `ViewExtensions`), it's possible to get into 
			// a situation where after invoking the `Finished` handler below `s_animations` will have a new `Info`
			// object in it with the same AnimatableKey. We need to continue cancelling animations until that is no
			// longer the case; thus, the `while` loop.

			// If we don't cancel all of the animations popping in with this key, `AnimateInternal` will overwrite one
			// of them with the new `Info` object, and the overwritten animation will never complete; any `await` for
			// it will never return.

			while (s_animations.ContainsKey(key))
			{
				Info info = s_animations[key];

				s_animations.Remove(key);

				info.Tweener.ValueUpdated -= HandleTweenerUpdated;
				info.Tweener.Finished -= HandleTweenerFinished;
				info.Tweener.Stop();
				info.Finished?.Invoke(1.0f, true);
			}
		}

		static void AbortKinetic(AnimatableKey key)
		{
			if (s_kinetics.TryGetValue(key, out var ticker))
			{
				if (s_tweeners.TryGetValue(ticker, out Animation animation))
				{
					animation.AnimationManager?.Remove(ticker);
				}
			}

			s_kinetics.Remove(key);
		}

		static void AnimateInternal<T>(IAnimatable self, IAnimationManager animationManager, string name, Func<double, T> transform, Action<T> callback,
			uint rate, uint length, Easing easing, Action<T, bool> finished, Func<bool> repeat)
		{
			var key = new AnimatableKey(self, name);

			AbortAnimation(key);

			Action<double> step = f => callback(transform(f));
			Action<double, bool> final = null;
			if (finished != null)
				final = (f, b) => finished(transform(f), b);

			var info = new Info { Rate = rate, Length = length, Easing = easing ?? Easing.Linear, AnimationManager = animationManager };

			var tweener = new Tweener(info.Length, info.Rate, animationManager);
			tweener.Handle = key;
			tweener.ValueUpdated += HandleTweenerUpdated;
			tweener.Finished += HandleTweenerFinished;

			info.Tweener = tweener;
			info.Callback = step;
			info.Finished = final;
			info.Repeat = repeat;
			info.Owner = new WeakReference<IAnimatable>(self);

			s_animations[key] = info;

			info.Callback(0.0f);
			tweener.Start();
		}

		static void AnimateKineticInternal(IAnimatable self, IAnimationManager animationManager, string name, Func<double, double, bool> callback, double velocity, double drag, Action finished = null)
		{
			var key = new AnimatableKey(self, name);

			AbortKinetic(key);

			double sign = velocity / Math.Abs(velocity);
			velocity = Math.Abs(velocity);
			int tick = 0;
			tick = animationManager.Insert(step =>
			{
				long ms = step;

				velocity -= drag * ms;
				velocity = Math.Max(0, velocity);

				var result = false;
				if (velocity > 0)
				{
					result = callback(sign * velocity * ms, velocity);
				}

				if (!result)
				{
					finished?.Invoke();
					if (s_kinetics.TryGetValue(key, out var ticker))
					{
						animationManager.Remove(ticker);
					}
					s_kinetics.Remove(key);
				}
				return result;
			});
			s_kinetics[key] = tick;
			if (!animationManager.Ticker.IsRunning)
				animationManager.Ticker.Start();
		}

		static void HandleTweenerFinished(object o, EventArgs args)
		{
			var tweener = o as Tweener;

			if (tweener != null && s_animations.TryGetValue(tweener.Handle, out Info info))
			{
				var tweenerValue = tweener.Value;
				info.Owner.TryGetTarget(out IAnimatable owner);

				owner?.BatchBegin();

				info.Callback(tweenerValue);

				var repeat = false;

				// If the Ticker has been disabled (e.g., by power save mode), then don't repeat the animation
				var animationsEnabled = info.AnimationManager.Ticker.SystemEnabled;

				if (info.Repeat != null && animationsEnabled)
				{
					repeat = info.Repeat();
				}

				if (!repeat)
				{
					s_animations.Remove(tweener.Handle);
					tweener.ValueUpdated -= HandleTweenerUpdated;
					tweener.Finished -= HandleTweenerFinished;
					tweener.Stop();
				}

				info.Finished?.Invoke(tweenerValue, !animationsEnabled);

				owner?.BatchCommit();

				if (repeat)
				{
					tweener.Start();
				}
			}
		}

		static void HandleTweenerUpdated(object o, EventArgs args)
		{
			if (o is Tweener tweener && s_animations.TryGetValue(tweener.Handle, out Info info) && info.Owner.TryGetTarget(out IAnimatable owner))
			{
				owner.BatchBegin();
				info.Callback(info.Easing.Ease(tweener.Value));
				owner.BatchCommit();
			}
		}

		static void DoAction(IAnimatable self, Action action)
		{
			IDispatcher dispatcher = null;
			if (self is BindableObject element)
				dispatcher = element.Dispatcher;

			// a null dispatcher is OK as we will find one in Dispatch
			dispatcher.DispatchIfRequired(action);
		}

		class Info
		{
			public Action<double> Callback;
			public Action<double, bool> Finished;
			public Func<bool> Repeat;
			public Tweener Tweener;

			public Easing Easing { get; set; }

			public uint Length { get; set; }
			public IAnimationManager AnimationManager { get; set; }
			public WeakReference<IAnimatable> Owner { get; set; }

			public uint Rate { get; set; }
		}

		sealed class BatchObject : IDisposable
		{
			IAnimatable _animatable;

			public BatchObject(IAnimatable animatable)
			{
				_animatable = animatable;
				_animatable?.BatchBegin();
			}

			public void Dispose()
			{
				_animatable?.BatchCommit();
				_animatable = null;
			}
		}
	}
}
