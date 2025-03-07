using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class GetHandlersBenchmarker
	{
		MauiApp _mauiApp;
		Label _label;

		Registrar<IView, IViewHandler> _registrar;

		[Params(100_000)]
		public int N { get; set; }

		[GlobalSetup(Target = nameof(GetHandlerUsingInstance))]
		public void SetupForInstanceGetter()
		{
			_label = new Label();
			_mauiApp = MauiApp
				.CreateBuilder()
				.Build();
		}

		[GlobalSetup(Target = nameof(GetHandlerUsingDI))]
		public void SetupForDI()
		{
			_label = new Label();
			_mauiApp = MauiApp
				.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, ButtonHandler>())
				.Build();
		}

		[GlobalSetup(Target = nameof(GetHandlerUsingRegistrar))]
		public void SetupForRegistrar()
		{
			_registrar = new Registrar<IView, IViewHandler>();
			_registrar.Register<ILabel, LabelHandler>();
		}

		[Benchmark]
		public void GetHandlerUsingInstance()
		{
			var handlers = _mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			for (int i = 0; i < N; i++)
			{
				handlers.GetHandler(_label, context: null);
			}
		}

		[Benchmark]
		public void GetHandlerUsingDI()
		{
			var handlers = _mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			for (int i = 0; i < N; i++)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				handlers.GetHandler(typeof(Button));
#pragma warning restore CS0618
			}
		}

		[Benchmark(Baseline = true)]
		public void GetHandlerUsingRegistrar()
		{
			for (int i = 0; i < N; i++)
			{
				_registrar.GetHandler<ILabel>();
			}
		}
	}
}