using System;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	// The Controls shape views (Line, Path, Polygon, Polyline, Rectangle, RoundRectangle, BoxView) are IShapeView. Their stock handlers
	// (LineHandler, ...) take only a property mapper, so a GTK command mapper cannot be supplied: these derive from GtkShapeViewHandler
	// and are registered by virtual-view type instead. Each adds the repaint keys of its own properties to the shape mappings.

	static class GtkShapeMappers
	{
		/// <summary>A builder over <paramref name="stock"/> with the shape mappings plus a repaint for each of <paramref name="keys"/>.</summary>
		internal static GtkMapperBuilder<TView, IShapeViewHandler> CreateBuilder<TView>(IPropertyMapper<TView, IShapeViewHandler> stock, params string[] keys)
			where TView : class, IShapeView
		{
			var builder = new GtkMapperBuilder<TView, IShapeViewHandler>(stock)
				.AddViewMappings()
				.AddShapeMappings();

			foreach (var key in keys)
				builder.MapGtk<MauiGtkGraphicsView>(key, (h, v, w) => w.Invalidate());

			return builder;
		}

		/// <summary>
		/// The mapper of <typeparamref name="TView"/>: chained to its stock handler mapper (so user customisations of
		/// <c>LineHandler.Mapper</c>, ... run on GTK too) plus the GTK mappings.
		/// </summary>
		internal static IPropertyMapper<TView, IShapeViewHandler> Create<TView>(IPropertyMapper<TView, IShapeViewHandler> stock, params string[] keys)
			where TView : class, IShapeView =>
			CreateBuilder(stock, keys).Build();

		/// <summary>
		/// Polygon/Polyline mapper: <c>Points</c> repaints and (re)subscribes the handler to the <see cref="PointCollection"/>, so in-place
		/// <c>Add</c>/<c>Clear</c> repaint as on the stock handlers.
		/// </summary>
		internal static IPropertyMapper<TView, IShapeViewHandler> CreatePoints<TView>(IPropertyMapper<TView, IShapeViewHandler> stock, Func<TView, PointCollection?> points)
			where TView : class, IShapeView
		{
			var builder = CreateBuilder(stock, "FillRule");
			builder.MapGtk<MauiGtkGraphicsView>("Points", (h, v, w) =>
			{
				(h as IGtkPointsHandler)?.Points.Watch(points(v));
				w.Invalidate();
			});

			return builder.Build();
		}
	}

	/// <summary>A handler whose shape has a <see cref="PointCollection"/> to watch.</summary>
	interface IGtkPointsHandler
	{
		GtkPointsWatcher Points { get; }
	}

	/// <summary>
	/// Follows one <see cref="PointCollection"/> (the stock <c>UpdatePointsSubscription</c>): a change runs <c>UpdateValue("Points")</c> so the
	/// mapper (and user customisations of it) repaint. Unsubscribed on <see cref="Clear"/> (disconnect).
	/// </summary>
	sealed class GtkPointsWatcher
	{
		readonly IElementHandler _handler;
		PointCollection? _points;

		public GtkPointsWatcher(IElementHandler handler) => _handler = handler;

		/// <summary>Moves the subscription to <paramref name="points"/> (no-op when it is the one already watched).</summary>
		public void Watch(PointCollection? points)
		{
			if (ReferenceEquals(_points, points))
				return;

			Clear();
			if (points is null)
				return;

			_points = points;
			points.CollectionChanged += OnChanged;
		}

		public void Clear()
		{
			if (_points is null)
				return;

			_points.CollectionChanged -= OnChanged;
			_points = null;
		}

		void OnChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => _handler.UpdateValue("Points");
	}

	/// <summary>GTK handler of <see cref="Shapes.Line"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; X1, Y1, X2, Y2 repaint it.</summary>
	public class GtkLineHandler : GtkShapeViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkLineHandler()
			: base(GtkMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkLineHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper)
		{
		}

		internal static new readonly IPropertyMapper<Shapes.Line, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create(LineHandler.Mapper, "X1", "Y1", "X2", "Y2");
	}

	/// <summary>GTK handler of <see cref="Shapes.Path"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; Data, RenderTransform repaint it.</summary>
	public class GtkPathHandler : GtkShapeViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkPathHandler()
			: base(GtkMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkPathHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper)
		{
		}

		internal static new readonly IPropertyMapper<Shapes.Path, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create(PathHandler.Mapper, "Data", "RenderTransform");
	}

	/// <summary>GTK handler of <see cref="Shapes.Polygon"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; Points, FillRule repaint it.</summary>
	public class GtkPolygonHandler : GtkShapeViewHandler, IGtkPointsHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkPolygonHandler()
			: base(GtkMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkPolygonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper)
		{
		}

		GtkPointsWatcher? _points;

		GtkPointsWatcher IGtkPointsHandler.Points => _points ??= new GtkPointsWatcher(this);

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			((IGtkPointsHandler)this).Points.Watch((VirtualView as Shapes.Polygon)?.Points);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			_points?.Clear();
			base.DisconnectHandler(platformView);
		}

		internal static new readonly IPropertyMapper<Shapes.Polygon, IShapeViewHandler> GtkMapper = GtkShapeMappers.CreatePoints<Shapes.Polygon>(PolygonHandler.Mapper, v => v.Points);
	}

	/// <summary>GTK handler of <see cref="Shapes.Polyline"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; Points, FillRule repaint it.</summary>
	public class GtkPolylineHandler : GtkShapeViewHandler, IGtkPointsHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkPolylineHandler()
			: base(GtkMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkPolylineHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper)
		{
		}

		GtkPointsWatcher? _points;

		GtkPointsWatcher IGtkPointsHandler.Points => _points ??= new GtkPointsWatcher(this);

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			((IGtkPointsHandler)this).Points.Watch((VirtualView as Shapes.Polyline)?.Points);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			_points?.Clear();
			base.DisconnectHandler(platformView);
		}

		internal static new readonly IPropertyMapper<Shapes.Polyline, IShapeViewHandler> GtkMapper = GtkShapeMappers.CreatePoints<Shapes.Polyline>(PolylineHandler.Mapper, v => v.Points);
	}

	/// <summary>GTK handler of <see cref="Shapes.Rectangle"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; RadiusX, RadiusY repaint it.</summary>
	public class GtkRectangleHandler : GtkShapeViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkRectangleHandler()
			: base(GtkMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkRectangleHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper)
		{
		}

		internal static new readonly IPropertyMapper<Shapes.Rectangle, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create(RectangleHandler.Mapper, "RadiusX", "RadiusY");
	}

	/// <summary>GTK handler of <see cref="Shapes.RoundRectangle"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; CornerRadius repaint it.</summary>
	public class GtkRoundRectangleHandler : GtkShapeViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkRoundRectangleHandler()
			: base(GtkMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkRoundRectangleHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper)
		{
		}

		internal static new readonly IPropertyMapper<Shapes.RoundRectangle, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create(RoundRectangleHandler.Mapper, "CornerRadius");
	}

	/// <summary>GTK handler of <see cref="BoxView"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; Color, CornerRadius repaint it.</summary>
	public class GtkBoxViewHandler : GtkShapeViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkBoxViewHandler()
			: base(GtkMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkBoxViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper)
		{
		}

		internal static new readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create(ShapeViewHandler.Mapper, "Color", "CornerRadius");
	}
}
