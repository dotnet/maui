using System;
using System.Collections.Generic;

namespace Xamarin.Essentials
{
	class LowPassFilter
	{
		const int length = 10;

		readonly Queue<float> history = new Queue<float>(length);
		float sin;
		float cos;

		internal void Add(float radians)
		{
			sin += (float)Math.Sin(radians);

			cos += (float)Math.Cos(radians);

			history.Enqueue(radians);

			if (history.Count > length)
			{
				var old = history.Dequeue();

				sin -= (float)Math.Sin(old);

				cos -= (float)Math.Cos(old);
			}
		}

		internal float Average()
		{
			var size = history.Count;

			return (float)Math.Atan2(sin / size, cos / size);
		}
	}
}
