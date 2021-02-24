using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class RegisterHandlersBenchmarker
	{
		IAppHostBuilder _builder;

		Registrar<IFrameworkElement, IViewHandler> _registrar;

		[Params(100_000)]
		public int N { get; set; }

		[IterationSetup(Target = nameof(RegisterHandlerUsingDI))]
		public void SetupForDI()
		{
			_builder = new AppHostBuilder();
		}

		[IterationSetup(Target = nameof(RegisterHandlerUsingRegistrar))]
		public void SetupForRegistrar()
		{
			_registrar = new Registrar<IFrameworkElement, IViewHandler>();
		}

		[Benchmark]
		public void RegisterHandlerUsingDI()
		{
			for (int i = 0; i < N; i++)
			{
				_builder.RegisterHandler<IButton, ButtonHandler>();
			}
		}

		[Benchmark(Baseline = true)]
		public void RegisterHandlerUsingRegistrar()
		{
			for (int i = 0; i < N; i++)
			{
				_registrar.Register<IButton, ButtonHandler>();
			}
		}
	}
}
