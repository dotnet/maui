using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class GetHandlersBenchmarker
	{
		MauiApp _mauiApp;

		Registrar<IView, IViewHandler> _registrar;

		[Params(100_000)]
		public int N { get; set; }

		[GlobalSetup(Target = nameof(GetHandlerUsingDI))]
		public void SetupForDI()
		{
			_mauiApp = MauiApp
				.CreateBuilder()
				.Build();
		}

		[GlobalSetup(Target = nameof(GetHandlerUsingRegistrar))]
		public void SetupForRegistrar()
		{
			_registrar = new Registrar<IView, IViewHandler>();
			_registrar.Register<IButton, ButtonHandler>();
		}

		[Benchmark]
		public void GetHandlerUsingDI()
		{
			var handlers = _mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			for (int i = 0; i < N; i++)
			{
				handlers.GetHandler<IButton>();
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