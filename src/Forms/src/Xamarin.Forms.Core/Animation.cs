//
// Tweener.cs
//
// Author:
//       Jason Smith <jason.smith@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc.
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
using System.Collections;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public class Animation : IEnumerable
	{
		readonly List<Animation> _children;
		readonly Easing _easing;
		readonly Action _finished;
		readonly Action<double> _step;
		double _beginAt;
		double _finishAt;
		bool _finishedTriggered;

		public Animation()
		{
			_children = new List<Animation>();
			_easing = Easing.Linear;
			_step = f => { };
		}

		public Animation(Action<double> callback, double start = 0.0f, double end = 1.0f, Easing easing = null, Action finished = null)
		{
			_children = new List<Animation>();
			_easing = easing ?? Easing.Linear;
			_finished = finished;

			Func<double, double> transform = AnimationExtensions.Interpolate(start, end);
			_step = f => callback(transform(f));
		}

		public IEnumerator GetEnumerator()
		{
			return _children.GetEnumerator();
		}

		public void Add(double beginAt, double finishAt, Animation animation)
		{
			if (beginAt < 0 || beginAt > 1)
				throw new ArgumentOutOfRangeException("beginAt");

			if (finishAt < 0 || finishAt > 1)
				throw new ArgumentOutOfRangeException("finishAt");

			if (finishAt <= beginAt)
				throw new ArgumentException("finishAt must be greater than beginAt");

			animation._beginAt = beginAt;
			animation._finishAt = finishAt;
			_children.Add(animation);
		}

		public void Commit(IAnimatable owner, string name, uint rate = 16, uint length = 250, Easing easing = null, Action<double, bool> finished = null, Func<bool> repeat = null)
		{
			owner.Animate(name, this, rate, length, easing, finished, repeat);
		}

		public Action<double> GetCallback()
		{
			Action<double> result = f =>
			{
				_step(_easing.Ease(f));
				foreach (Animation animation in _children)
				{
					if (animation._finishedTriggered)
						continue;

					double val = Math.Max(0.0f, Math.Min(1.0f, (f - animation._beginAt) / (animation._finishAt - animation._beginAt)));

					if (val <= 0.0f) // not ready to process yet
						continue;

					Action<double> callback = animation.GetCallback();
					callback(val);

					if (val >= 1.0f)
					{
						animation._finishedTriggered = true;
						if (animation._finished != null)
							animation._finished();
					}
				}
			};
			return result;
		}

		internal void ResetChildren()
		{
			foreach (var anim in _children)
				anim._finishedTriggered = false;
		}

		public Animation Insert(double beginAt, double finishAt, Animation animation)
		{
			Add(beginAt, finishAt, animation);
			return this;
		}

		public Animation WithConcurrent(Animation animation, double beginAt = 0.0f, double finishAt = 1.0f)
		{
			animation._beginAt = beginAt;
			animation._finishAt = finishAt;
			_children.Add(animation);
			return this;
		}

		public Animation WithConcurrent(Action<double> callback, double start = 0.0f, double end = 1.0f, Easing easing = null, double beginAt = 0.0f, double finishAt = 1.0f)
		{
			var child = new Animation(callback, start, end, easing);
			child._beginAt = beginAt;
			child._finishAt = finishAt;
			_children.Add(child);
			return this;
		}

		public static bool IsEnabled
		{
			get
			{
				return Internals.Ticker.Default.SystemEnabled;
			}
		}
	}
}