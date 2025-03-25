using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class PropertyMapperExtensionsBenchmarker
	{
		class TestButtonHandler : ButtonHandler
		{
			protected override object CreatePlatformView()
			{
				return new object();
			}
		}

		Registrar<IView, IViewHandler> _registrar;
		IElementHandler _handler;

		[Params(100_000)]
		public int N { get; set; }

		const string Basic = "basic";
		const string Append = "append";
		const string Prepend = "prepend";

		[GlobalSetup()]
		public void Setup()
		{
			_registrar = new Registrar<IView, IViewHandler>();
			_registrar.Register<IButton, TestButtonHandler>();

			var button = new Button();

			_handler = _registrar.GetHandler<IButton>();
			_handler.SetVirtualView(button);

			ViewHandler.ViewMapper.Add(Basic, MapTest);

			// Add an original mapping to which we can prepend a mapping, then prepend to it
			ViewHandler.ViewMapper.Add(Prepend, MapTest);
			ViewHandler.ViewMapper.PrependToMapping(Prepend, MapPrepend);

			// Add an original mapping to which we can append a mapping, then prepend to it
			ViewHandler.ViewMapper.Add(Append, MapTest);
			ViewHandler.ViewMapper.AppendToMapping(Append, MapAppend);
		}

		[Benchmark]
		public void BenchmarkBasicPropertyMapping()
		{
			_handler.UpdateValue(Basic);
		}

		[Benchmark]
		public void BenchmarkPrependPropertyMapping()
		{
			_handler.UpdateValue(Prepend);
		}

		[Benchmark]
		public void BenchmarkAppendPropertyMapping()
		{
			_handler.UpdateValue(Append);
		}

		static void MapTest(IViewHandler handler, IView view)
		{
		}

		static void MapPrepend(IViewHandler handler, IView view)
		{
		}

		static void MapAppend(IViewHandler handler, IView view)
		{
		}
	}
}