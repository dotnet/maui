using Android.Content;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Benchmarks.Droid;

public class ViewHandlerBenchmark
{
	readonly MauiContext _context = new();

	// FIXME: BoxView hits an exception on API 31 in Maui.Graphics
	// [Benchmark]
	public void BoxView()
	{
		var handler = new ShapeViewHandler();
		handler.SetMauiContext(_context);

		new BoxView
		{
			Handler = handler,
		};
	}

	[Benchmark]
	public void Border()
	{
		var handler = new BorderHandler();
		handler.SetMauiContext(_context);

		new Border
		{
			Handler = handler,
		};
	}

	[Benchmark]
	public void ActivityIndicator()
	{
		var handler = new ActivityIndicatorHandler();
		handler.SetMauiContext(_context);

		new ActivityIndicator
		{
			Handler = handler,
		};
	}

	[Benchmark]
	public void ContentView()
	{
		var handler = new ContentViewHandler();
		handler.SetMauiContext(_context);

		new ContentView
		{
			Handler = handler,
		};
	}

	[Benchmark]
	public void Label()
	{
		var handler = new LabelHandler();
		handler.SetMauiContext(_context);

		new Label
		{
			Handler = handler,
		};
	}

	[Benchmark]
	public void Entry()
	{
		var handler = new EntryHandler();
		handler.SetMauiContext(_context);

		new Entry
		{
			Handler = handler,
		};
	}

	class MauiContext : IMauiContext
	{
		readonly ServiceProvider _services = new();
		readonly Context _context = Android.App.Application.Context;

		public IServiceProvider Services => _services;

		public IMauiHandlersFactory Handlers => _services;

		public Context? Context => _context;
	}

	class ServiceProvider : IServiceProvider, IMauiHandlersFactory
	{
		readonly FontManager _fontManager = new(new FontRegistrar(new EmbeddedFontLoader()));

		public IMauiHandlersCollection GetCollection() => throw new NotImplementedException();

		public IElementHandler? GetHandler(Type type, IMauiContext context) => throw new NotImplementedException();

		public IElementHandler? GetHandler<T>(IMauiContext context) where T : IElement => throw new NotImplementedException();

		public Type? GetHandlerType(Type iview) => throw new NotImplementedException();

		public object? GetService(Type serviceType)
		{
			if (serviceType == typeof(IFontManager))
				return _fontManager;

			throw new NotImplementedException();
		}
	}
}