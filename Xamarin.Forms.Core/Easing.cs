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

namespace Xamarin.Forms
{
	public class Easing
	{
		public static readonly Easing Linear = new Easing(x => x);

		public static readonly Easing SinOut = new Easing(x => Math.Sin(x * Math.PI * 0.5f));
		public static readonly Easing SinIn = new Easing(x => 1.0f - Math.Cos(x * Math.PI * 0.5f));
		public static readonly Easing SinInOut = new Easing(x => -Math.Cos(Math.PI * x) / 2.0f + 0.5f);

		public static readonly Easing CubicIn = new Easing(x => x * x * x);
		public static readonly Easing CubicOut = new Easing(x => Math.Pow(x - 1.0f, 3.0f) + 1.0f);

		public static readonly Easing CubicInOut = new Easing(x => x < 0.5f ? Math.Pow(x * 2.0f, 3.0f) / 2.0f : (Math.Pow((x - 1) * 2.0f, 3.0f) + 2.0f) / 2.0f);

		public static readonly Easing BounceOut;
		public static readonly Easing BounceIn;

		public static readonly Easing SpringIn = new Easing(x => x * x * ((1.70158f + 1) * x - 1.70158f));
		public static readonly Easing SpringOut = new Easing(x => (x - 1) * (x - 1) * ((1.70158f + 1) * (x - 1) + 1.70158f) + 1);

		readonly Func<double, double> _easingFunc;

		static Easing()
		{
			BounceOut = new Easing(p =>
			{
				if (p < 1 / 2.75f)
				{
					return 7.5625f * p * p;
				}
				if (p < 2 / 2.75f)
				{
					p -= 1.5f / 2.75f;

					return 7.5625f * p * p + .75f;
				}
				if (p < 2.5f / 2.75f)
				{
					p -= 2.25f / 2.75f;

					return 7.5625f * p * p + .9375f;
				}
				p -= 2.625f / 2.75f;

				return 7.5625f * p * p + .984375f;
			});

			BounceIn = new Easing(p => 1.0f - BounceOut.Ease(1 - p));
		}

		public Easing(Func<double, double> easingFunc)
		{
			if (easingFunc == null)
				throw new ArgumentNullException("easingFunc");

			_easingFunc = easingFunc;
		}

		public double Ease(double v)
		{
			return _easingFunc(v);
		}

		public static implicit operator Easing(Func<double, double> func)
		{
			return new Easing(func);
		}
	}
}