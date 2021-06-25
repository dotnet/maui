using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Maui.Animations
{
	public class Animation : IDisposable, IEnumerable
	{
		public Animation()
		{

		}
		public Animation(Action<double> callback, double start = 0.0f, double duration = 1.0f, Easing? easing = null, Action? finished = null)
		{
			StartDelay = start;
			Duration = duration;
			Finished = finished;
			Easing = easing ?? Easing.Default;
			Step = callback;

		}
		public Animation(List<Animation> animations)
		{
			childrenAnimations = animations;
		}
		internal WeakReference<IAnimator>? Parent { get; set; }

		public Action? Finished { get; set; }
		public Action<double>? Step { get; set; }
		bool paused;
		public bool IsPaused => paused;
		protected List<Animation> childrenAnimations = new List<Animation>();

		public string? Name { get; set; }
		public double StartDelay { get; set; }
		public double Duration { get; set; }
		public double CurrentTime { get; protected set; }
		public double Progress { get; protected set; }
		public Easing Easing { get; set; } = Easing.Default;
		public bool HasFinished { get; protected set; }
		public bool Repeats { get; set; }
		double _skippedSeconds;
		int _usingResource = 0;

		public IEnumerator GetEnumerator() => childrenAnimations.GetEnumerator();

		public void Add(double beginAt, double duration, Animation animation)
		{
			if (beginAt < 0 || beginAt > 1)
				throw new ArgumentOutOfRangeException("beginAt");

			if (duration < 0 || duration > 1)
				throw new ArgumentOutOfRangeException("finishAt");

			if (duration <= beginAt)
				throw new ArgumentException("finishAt must be greater than beginAt");

			animation.StartDelay = beginAt;
			animation.Duration = duration;
			childrenAnimations.Add(animation);
		}

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
		public IAnimationManager? AnimationManager => animationManger;
		protected IAnimationManager? animationManger;

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

		public virtual void Update(double percent)
		{
			try
			{
				Progress = Easing.Ease(percent);
				Step?.Invoke(Progress);
				HasFinished = percent == 1;
			}
			catch (Exception ex)
			{
				//TODO log exception
				Console.WriteLine(ex);
				HasFinished = true;
			}
		}
		public void Commit(IAnimationManager animationManger)
		{
			this.animationManger = animationManger;
			animationManger.Add(this);

		}

		public Animation CreateAutoReversing()
		{
			var reveresedChildren = childrenAnimations.ToList();
			reveresedChildren.Reverse();
			var reveresed = CreateReverse();
			var parentAnimation = new Animation
			{
				Duration = reveresed.StartDelay + reveresed.Duration,
				Repeats = Repeats,
				childrenAnimations =
				{
					this,
					reveresed,
				}
			};
			Repeats = false;
			return parentAnimation;
		}

		protected virtual Animation CreateReverse()
		{
			var reveresedChildren = childrenAnimations.ToList();
			reveresedChildren.Reverse();
			return new Animation
			{
				Easing = Easing,
				Duration = Duration,
				StartDelay = StartDelay + Duration,
				childrenAnimations = reveresedChildren,
			};
		}

		public virtual void Reset()
		{
			CurrentTime = 0;
			HasFinished = false;
			foreach (var x in childrenAnimations)
				x.Reset();
		}

		public void Pause()
		{
			paused = true;
			animationManger?.Remove(this);
		}
		public void Resume()
		{
			paused = false;
			animationManger?.Add(this);
		}
		public void RemoveFromParent()
		{
			IAnimator? view = null;
			if (this.Parent?.TryGetTarget(out view) ?? false)
				view?.RemoveAnimation(this);
		}





		#region IDisposable Support
		public bool IsDisposed => disposedValue;


		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var child in childrenAnimations)
						child.Dispose();
					childrenAnimations.Clear();
				}
				disposedValue = true;
				animationManger?.Remove(this);
				Finished = null;
				Step = null;
			}
		}


		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}

		#endregion
	}
}