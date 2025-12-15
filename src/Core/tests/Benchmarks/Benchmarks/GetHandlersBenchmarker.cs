using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class GetHandlersBenchmarker
	{
		MauiApp _mauiApp;
		MauiContext _mauiContext;

		Registrar<IView, IViewHandler> _registrar;

		[Params(100_000)]
		public int N { get; set; }

		[GlobalSetup(Target = nameof(GetHandlerUsingDI))]
		public void SetupForDI()
		{
			_mauiApp = MauiApp
				.CreateBuilder()
				.Build();
			_mauiContext = _mauiApp.Services.GetService<MauiContext>();
		}

		[GlobalSetup(Target = nameof(GetHandlerUsingRegistrar))]
		public void SetupForRegistrar()
		{
			_registrar = new Registrar<IView, IViewHandler>();
			_registrar.Register<Button, ButtonHandler>();
		}

		[Benchmark]
		public void GetHandlerUsingDI()
		{
			var handlers = _mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			for (int i = 0; i < N; i++)
			{
				handlers.GetHandler<Button>(_mauiContext);
			}
		}

		[Benchmark(Baseline = true)]
		public void GetHandlerUsingRegistrar()
		{
			for (int i = 0; i < N; i++)
			{
				_registrar.GetHandler<Button>();
			}
		}
	}
}