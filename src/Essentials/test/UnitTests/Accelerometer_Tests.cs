using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Tests
{
	public class Accelerometer_Tests
	{
		// A subscriber that forgets to unsubscribe with "-=" should still be collectible
		// instead of being retained for the lifetime of the process.
		[Fact]
		public void Accelerometer_ReadingChanged_DoesNotLeakForgottenSubscriber()
		{
			var subscriberRef = SubscribeWithoutUnsubscribing();

			for (var i = 0; i < 3 && subscriberRef.IsAlive; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}

			Assert.False(subscriberRef.IsAlive,
				"Accelerometer.ReadingChanged subscriber was still alive after GC; " +
				"a forgotten unsubscribe should not leak the subscriber for the process lifetime.");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference SubscribeWithoutUnsubscribing()
		{
			var subscriber = new LeakySubscriber();
			Accelerometer.ReadingChanged += subscriber.OnReadingChanged;

			try
			{
				Accelerometer.Stop();
			}
			catch
			{
				// Not implemented in the reference assembly used by unit tests; the leak
				// reproduces independently of Start()/Stop() actually running.
			}

			return new WeakReference(subscriber);
		}

		sealed class LeakySubscriber
		{
			public void OnReadingChanged(object sender, AccelerometerChangedEventArgs e)
			{
			}
		}

		// A weak-reference-based fix must not drop live subscribers: a subscriber that is
		// still alive should keep receiving ReadingChanged after a GC.
		[Fact]
		public void Accelerometer_ReadingChanged_StillNotifiesLiveSubscriberAfterGC()
		{
			var subscriber = new CountingSubscriber();
			Accelerometer.ReadingChanged += subscriber.OnReadingChanged;

			try
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();

				var implementation = (AccelerometerImplementation)Accelerometer.Default;
				implementation.OnChanged(new AccelerometerData(1, 2, 3));

				Assert.Equal(1, subscriber.CallCount);
			}
			finally
			{
				Accelerometer.ReadingChanged -= subscriber.OnReadingChanged;
			}
		}

		sealed class CountingSubscriber
		{
			public int CallCount { get; private set; }

			public void OnReadingChanged(object sender, AccelerometerChangedEventArgs e) => CallCount++;
		}

		// A weak-event-backed subscriber invocation must surface the subscriber's original
		// exception instance directly, not a wrapping System.Reflection.TargetInvocationException,
		// to match plain multicast delegate invocation semantics.
		[Fact]
		public void Accelerometer_ReadingChanged_PropagatesSubscriberExceptionDirectly()
		{
			var expected = new InvalidOperationException("subscriber failed");
			EventHandler<AccelerometerChangedEventArgs> handler = (_, _) => throw expected;

			Accelerometer.ReadingChanged += handler;

			try
			{
				var implementation = (AccelerometerImplementation)Accelerometer.Default;
				var actual = Assert.Throws<InvalidOperationException>(() => implementation.OnChanged(new AccelerometerData(1, 2, 3)));

				Assert.Same(expected, actual);
			}
			finally
			{
				Accelerometer.ReadingChanged -= handler;
			}
		}

		[Fact]
		public void Accelerometer_Start() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Accelerometer.Stop());

		[Fact]
		public void Accelerometer_Stop() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Accelerometer.Start(SensorSpeed.Default));

		[Fact]
		public void Accelerometer_IsMonitoring() =>
			Assert.False(Accelerometer.IsMonitoring);

		[Fact]
		public void Accelerometer_IsSupported() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Accelerometer.IsSupported);

		[Theory]
		[InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, true)]
		[InlineData(0.0, 0.0, 0.0, 1.0, 0.0, 0.0, false)]
		[InlineData(0.0, 0.0, 0.0, 0.0, 1.0, 0.0, false)]
		[InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, false)]
		public void Accelerometer_Comparison(
			  float x1,
			  float y1,
			  float z1,
			  float x2,
			  float y2,
			  float z2,
			  bool equals)
		{
			var data1 = new AccelerometerData(x1, y1, z1);
			var data2 = new AccelerometerData(x2, y2, z2);
			if (equals)
			{
				Assert.True(data1.Equals(data2));
				Assert.True(data1 == data2);
				Assert.False(data1 != data2);
				Assert.Equal(data1, data2);
				Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
			}
			else
			{
				Assert.False(data1.Equals(data2));
				Assert.False(data1 == data2);
				Assert.True(data1 != data2);
				Assert.NotEqual(data1, data2);
				Assert.NotEqual(data1.GetHashCode(), data2.GetHashCode());
			}
		}

		[Fact]
		public void InitialShaking()
		{
			var q = new AccelerometerQueue();
			Assert.False(q.IsShaking);
		}

		[Fact]
		public void ShakingTests()
		{
			var now = DateTime.UtcNow;
			var q = new AccelerometerQueue();
			q.Add(GetShakeTime(now, 0), false);
			q.Add(GetShakeTime(now, .3), false);
			q.Add(GetShakeTime(now, .6), false);
			q.Add(GetShakeTime(now, .9), false);
			Assert.False(q.IsShaking);

			// The oldest two entries will be removed.
			q.Add(GetShakeTime(now, 1.2), true);
			q.Add(GetShakeTime(now, 1.5), true);
			Assert.False(q.IsShaking);

			// Another entry should be removed, now 3 out of 4 are true.
			q.Add(GetShakeTime(now, 1.8), true);
			Assert.True(q.IsShaking);

			q.Add(GetShakeTime(now, 2.1), false);
			Assert.True(q.IsShaking);

			q.Add(GetShakeTime(now, 2.4), false);
			Assert.False(q.IsShaking);
		}

		[Fact]
		public void ClearQueue()
		{
			var now = DateTime.UtcNow;
			var q = new AccelerometerQueue();
			q.Add(GetShakeTime(now, 0), true);
			q.Add(GetShakeTime(now, .1), true);
			q.Add(GetShakeTime(now, .3), true);
			Assert.True(q.IsShaking);
			q.Clear();
			Assert.False(q.IsShaking);
		}

		long GetShakeTime(DateTime now, double seconds) =>
			Nanoseconds(now.AddSeconds(seconds));

		static long Nanoseconds(DateTime time) =>
				(time.Ticks / TimeSpan.TicksPerMillisecond) * 1_000_000;
	}
}
