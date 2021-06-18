using System;
using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class FastActivatorBenchmarker
	{
		[Benchmark]
		public object CreateInstanceUsingNew()
		{
			return new ButtonStub();
		}

		[Benchmark]
		public object CreateInstanceUsingActivator()
		{
			return Activator.CreateInstance(typeof(ButtonStub));
		}

		[Benchmark]
		public object RegisterHandlerUsingFastActivator1()
		{
			return FastActivator.CreateInstance<ButtonStub>();
		}

		[Benchmark]
		public object RegisterHandlerUsingFastActivator2()
		{
			return FastActivator.CreateInstance(typeof(ButtonStub));
		}
	}
}