namespace Microsoft.Maui.Devices.Sensors
{
	// Detect if 3/4ths of the accelerometer events in the last half second are accelerating
	// this means we are free falling or shaking
	class AccelerometerQueue
	{
		readonly AccelerometerDataPool pool = new AccelerometerDataPool();

		// in nanoseconds
		readonly long maxWindowSize = 500_000_000;
		readonly long minWindowSize = 250_000_000;

		readonly int minQueueSize = 4;

		AccelerometerSample oldest;
		AccelerometerSample newest;
		int sampleCount;
		int acceleratingCount;

		internal void Add(long timestamp, bool accelerating)
		{
			Purge(timestamp - maxWindowSize);
			var added = pool.Acquire();
			added.Timestamp = timestamp;
			added.IsAccelerating = accelerating;
			added.Next = null;

			newest?.Next = added;

			newest = added;

			if (oldest == null)
				oldest = added;

			sampleCount++;

			if (accelerating)
				acceleratingCount++;
		}

		internal void Clear()
		{
			while (oldest != null)
			{
				var removed = oldest;
				oldest = removed.Next;
				pool.Release(removed);
			}
			newest = null;
			sampleCount = 0;
			acceleratingCount = 0;
		}

		void Purge(long cutoff)
		{
			while (sampleCount >= minQueueSize &&
				   oldest != null &&
				   cutoff - oldest.Timestamp > 0)
			{
				var removed = oldest;
				if (removed.IsAccelerating)
					acceleratingCount--;

				sampleCount--;
				oldest = removed.Next;

				if (oldest == null)
					newest = null;

				pool.Release(removed);
			}
		}

		// Returns true if we have enough samples to detect if we are shaking the device and that more than 3/4th of them are accelerating
		internal bool IsShaking => newest != null &&
						  oldest != null &&
						  newest.Timestamp - oldest.Timestamp >= minWindowSize &&
						  acceleratingCount >= (sampleCount >> 1) + (sampleCount >> 2);

		internal class AccelerometerSample
		{
			public long Timestamp { get; set; }

			public bool IsAccelerating { get; set; }

			public AccelerometerSample Next { get; set; }
		}

		internal class AccelerometerDataPool
		{
			AccelerometerSample head;

			internal AccelerometerSample Acquire()
			{
				var aquired = head;
				if (aquired == null)
					aquired = new AccelerometerSample();
				else
					head = aquired.Next;

				return aquired;
			}

			internal void Release(AccelerometerSample sample)
			{
				sample.Next = head;
				head = sample;
			}
		}
	}
}
