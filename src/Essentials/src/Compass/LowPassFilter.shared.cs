using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Devices.Sensors
{
	class LowPassFilter
	{
		const int length = 10;

		readonly Queue<float> history = new Queue<float>(length);
		float sin;
		float cos;

		internal void Add(float radians)
		{
			sin += MathF.Sin(radians);

			cos += MathF.Cos(radians);

			history.Enqueue(radians);

			if (history.Count > length)
			{
				var old = history.Dequeue();

				sin -= MathF.Sin(old);

				cos -= MathF.Cos(old);
			}
		}

		internal float Average()
		{
			var size = history.Count;

			return MathF.Atan2(sin / size, cos / size);
		}
	}
}
