using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks;

[MemoryDiagnoser]
public class PropertyMapperBenchmarker
{
	class TestButtonHandler : ButtonHandler
	{
		protected override object CreatePlatformView()
		{
			return new object();
		}
	}

	readonly Registrar<IView, IViewHandler> _registrar;
	readonly IViewHandler _handler;
	readonly Button _button;

	public PropertyMapperBenchmarker()
	{
		_button = new Button();

		_registrar = new Registrar<IView, IViewHandler>();
		_registrar.Register<IButton, TestButtonHandler>();

		_handler = _registrar.GetHandler<IButton>();
		_handler.SetVirtualView(new Button());
	}

	[Benchmark]
	public void BenchmarkUpdateProperties()
	{
		var button = new Button();

		for (int i = 0; i < 100_000; i++)
		{
			var handler = _registrar.GetHandler<IButton>();
			handler.SetVirtualView(button);
		}
	}

	[Benchmark]
	public void BenchmarkUpdateProperty()
	{
		for (int i = 0; i < 1_000_000; i++)
		{
			_handler.UpdateValue(nameof(IView.Opacity));
		}
	}
}