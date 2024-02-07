using System;
using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class WeakEventBenchmarker
	{
		const int Iterations = 100;
		const string EventName = "MyEvent";

		void Handler(object sender, EventArgs e) { }

		[Benchmark]
		public void WeakEventHandler()
		{
			var eventManager = new WeakEventHandler<EventArgs>();

			for (int i = 0; i < Iterations; i++)
			{
				eventManager.AddEventHandler(Handler);
				eventManager.HandleEvent(this, EventArgs.Empty);
				eventManager.RemoveEventHandler(Handler);
			}
		}

		[Benchmark]
		public void WeakEventManager()
		{
			var eventManager = new WeakEventManager();

			for (int i = 0; i < Iterations; i++)
			{
				eventManager.AddEventHandler(Handler, EventName);
				eventManager.HandleEvent(this, EventArgs.Empty, EventName);
				eventManager.RemoveEventHandler(Handler, EventName);
			}
		}
	}
}
