using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class GetHandlersBenchmarker
	{
		AppStub _app;

		Registrar<IFrameworkElement, IViewHandler> _registrar;

		[Params(100_000)]
		public int N { get; set; }

		[GlobalSetup(Target = nameof(GetHandlerUsingDI))]
		public void SetupForDI()
		{
			var startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder();

			startup.Configure(appBuilder);

			var host = appBuilder.Build();

			_app = new AppStub();

			host.SetServiceProvider(_app);
		}

		[GlobalSetup(Target = nameof(GetHandlerUsingRegistrar))]
		public void SetupForRegistrar()
		{
			_registrar = new Registrar<IFrameworkElement, IViewHandler>();
			_registrar.Register<IButton, ButtonHandler>();
		}

		[Benchmark]
		public void GetHandlerUsingDI()
		{
			for (int i = 0; i < N; i++)
			{
				_app.Context.Handlers.GetHandler<IButton>();
			}
		}

		[Benchmark(Baseline = true)]
		public void GetHandlerUsingRegistrar()
		{
			for (int i = 0; i < N; i++)
			{
				_registrar.GetHandler<IButton>();
			}
		}
	}
}
