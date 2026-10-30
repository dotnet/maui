using System;

namespace Microsoft.Maui.Gtk.Hosting
{
	/// <summary>
	/// Builds the property mapper of a GTK handler: GTK implementations of the properties in front of the stock mapper.
	/// </summary>
	/// <remarks>
	/// <para>The GTK handlers pass this mapper to their stock base class, instead of patching the process-wide static mapper. A
	/// mapping first runs the GTK implementation (when the platform view is the expected GTK widget) and then the stock mapping
	/// of the same key, so Controls' remaps and user <c>AppendToMapping</c>/<c>PrependToMapping</c> customisations on the stock mapper
	/// still run, whenever they were registered. Patching the static mapper in place was not an option: Controls re-maps those
	/// statics lazily (for example <c>Label</c>'s static constructor) and its <c>ReplaceMapping</c> discards the previous mapping.</para>
	/// <para>The stock mapper is read live, so customisations made before the first handler of the type is created are honoured;
	/// create the mapper once per handler type (a static field).</para>
	/// </remarks>
	/// <typeparam name="TVirtualView">The virtual view type of the stock mapper.</typeparam>
	/// <typeparam name="THandler">The handler interface of the stock mapper.</typeparam>
	public sealed class GtkMapperBuilder<TVirtualView, THandler>
		where TVirtualView : IElement
		where THandler : IElementHandler
	{
		readonly IPropertyMapper<TVirtualView, THandler> _stock;
		readonly PropertyMapper<TVirtualView, THandler> _mapper;

		/// <summary>Starts a mapper in front of <paramref name="stock"/>.</summary>
		/// <param name="stock">The stock static mapper of the handler (for example <c>LabelHandler.Mapper</c>).</param>
		public GtkMapperBuilder(IPropertyMapper<TVirtualView, THandler> stock)
		{
			_stock = stock ?? throw new ArgumentNullException(nameof(stock));
			_mapper = new PropertyMapper<TVirtualView, THandler>(stock);
		}

		/// <summary>
		/// Maps <paramref name="key"/>: <paramref name="action"/> runs when the platform view is a <typeparamref name="TWidget"/>, then the stock
		/// mapping of the same key runs (whatever the platform view is).
		/// </summary>
		/// <typeparam name="TWidget">The expected GTK platform view type.</typeparam>
		/// <param name="key">The property name.</param>
		/// <param name="action">The GTK implementation.</param>
		/// <param name="callStock">
		/// Whether the stock mapping runs afterwards. Turn it off for the few stock mappings that throw on the platform-less assemblies
		/// (the layout's <c>MapBackground</c> is <c>NotImplemented</c>).
		/// </param>
		public GtkMapperBuilder<TVirtualView, THandler> MapGtk<TWidget>(string key, Action<THandler, TVirtualView, TWidget> action, bool callStock = true)
			where TWidget : class
		{
			var stock = _stock;

			_mapper.Add(key, (handler, view) =>
			{
				bool isGtk = handler.PlatformView is TWidget;
				if (handler.PlatformView is TWidget widget)
					action(handler, view, widget);

				if (callStock || !isGtk)
					stock.GetProperty(key)?.Invoke(handler, view);
			});

			return this;
		}

		/// <summary>The finished mapper.</summary>
		public IPropertyMapper<TVirtualView, THandler> Build() => _mapper;
	}

	/// <summary>
	/// The command-mapper counterpart of <see cref="GtkMapperBuilder{TVirtualView, THandler}"/>.
	/// </summary>
	/// <typeparam name="TVirtualView">The virtual view type of the stock command mapper.</typeparam>
	/// <typeparam name="THandler">The handler type of the stock command mapper.</typeparam>
	public sealed class GtkCommandMapperBuilder<TVirtualView, THandler>
		where TVirtualView : IElement
		where THandler : IElementHandler
	{
		readonly CommandMapper<TVirtualView, THandler> _stock;
		readonly CommandMapper<TVirtualView, THandler> _mapper;

		/// <summary>Starts a command mapper in front of <paramref name="stock"/>.</summary>
		/// <param name="stock">The stock static command mapper of the handler.</param>
		public GtkCommandMapperBuilder(CommandMapper<TVirtualView, THandler> stock)
		{
			_stock = stock ?? throw new ArgumentNullException(nameof(stock));
			_mapper = new CommandMapper<TVirtualView, THandler>(stock);
		}

		/// <summary>
		/// Maps the command <paramref name="key"/>: <paramref name="action"/> runs when the platform view is a <typeparamref name="TWidget"/>,
		/// then the stock command of the same key runs.
		/// </summary>
		/// <typeparam name="TWidget">The expected GTK platform view type.</typeparam>
		/// <param name="key">The command name.</param>
		/// <param name="action">The GTK implementation.</param>
		/// <param name="callStock">Whether the stock command runs afterwards.</param>
		public GtkCommandMapperBuilder<TVirtualView, THandler> MapGtk<TWidget>(string key, Action<THandler, TVirtualView, object?, TWidget> action, bool callStock = true)
			where TWidget : class
		{
			var stock = _stock;

			_mapper.Add(key, (handler, view, args) =>
			{
				bool isGtk = handler.PlatformView is TWidget;
				if (handler.PlatformView is TWidget widget)
					action(handler, view, args, widget);

				if (callStock || !isGtk)
					stock.GetCommand(key)?.Invoke(handler, view, args);
			});

			return this;
		}

		/// <summary>The finished command mapper.</summary>
		public CommandMapper<TVirtualView, THandler> Build() => _mapper;
	}
}
