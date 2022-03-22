using System;
using System.Collections;
using System.Collections.Generic;
using BaseAnimation = Microsoft.Maui.Animations.Animation;
namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="Type[@FullName='Microsoft.Maui.Controls.Animation']/Docs" />
	public class Animation : BaseAnimation
	{
		bool _finishedTriggered;

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public Animation()
		{
			Easing = Easing.Linear;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public Animation(Action<double> callback, double start = 0.0f, double end = 1.0f, Easing easing = null, Action finished = null) : base(callback, start, end - start, easing, finished)
		{

			Easing = easing ?? Easing.Linear;
			Func<double, double> transform = AnimationExtensions.Interpolate(start, end);
			Step = f => callback(transform(f));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='Add']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='Commit']/Docs" />
		public void Commit(IAnimatable owner, string name, uint rate = 16, uint length = 250, Easing easing = null, Action<double, bool> finished = null, Func<bool> repeat = null)
		{
			owner.Animate(name, this, rate, length, easing, finished, repeat);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='GetCallback']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='Reset']/Docs" />
		public override void Reset()
		{
			base.Reset();
			_finishedTriggered = false;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='Insert']/Docs" />
		public Animation Insert(double beginAt, double finishAt, Animation animation)
		{
			Add(beginAt, finishAt, animation);
			return this;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='WithConcurrent'][1]/Docs" />
		public Animation WithConcurrent(Animation animation, double beginAt = 0.0f, double finishAt = 1.0f)
		{
			animation.StartDelay = beginAt;
			animation.Duration = finishAt - beginAt;
			childrenAnimations.Add(animation);
			return this;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='WithConcurrent'][2]/Docs" />
		public Animation WithConcurrent(Action<double> callback, double start = 0.0f, double end = 1.0f, Easing easing = null, double beginAt = 0.0f, double finishAt = 1.0f)
		{
			var child = new Animation(callback, start, end, easing);
			child.StartDelay = beginAt;
			child.Duration = finishAt - beginAt;
			childrenAnimations.Add(child);
			return this;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Animation.xml" path="//Member[@MemberName='IsEnabled']/Docs" />
		public bool IsEnabled
		{
			get
			{
				return animationManger?.Ticker?.SystemEnabled ?? false;
			}
		}
	}
}
