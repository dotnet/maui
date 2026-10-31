using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	// The Controls shape views (Line, Path, Polygon, Polyline, Rectangle, RoundRectangle, BoxView) are IShapeView. Their stock handlers
	// (LineHandler, ...) take only a property mapper, so a GTK command mapper cannot be supplied: these derive from GtkShapeViewHandler
	// and are registered by virtual-view type instead. Each adds the repaint keys of its own properties to the shape mappings.

	static class GtkShapeMappers
	{
		/// <summary>The shape mapper plus a repaint for each of <paramref name="keys"/>.</summary>
		internal static IPropertyMapper<IShapeView, IShapeViewHandler> Create(params string[] keys)
		{
			var builder = new GtkMapperBuilder<IShapeView, IShapeViewHandler>(ShapeViewHandler.Mapper)
				.AddViewMappings()
				.AddShapeMappings();

			foreach (var key in keys)
				builder.MapGtk<MauiGtkGraphicsView>(key, (h, v, w) => w.Invalidate());

			return builder.Build();
		}
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

		internal static new readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create("X1", "Y1", "X2", "Y2");
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

		internal static new readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create("Data", "RenderTransform");
	}

	/// <summary>GTK handler of <see cref="Shapes.Polygon"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; Points, FillRule repaint it.</summary>
	public class GtkPolygonHandler : GtkShapeViewHandler
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

		internal static new readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create("Points", "FillRule");
	}

	/// <summary>GTK handler of <see cref="Shapes.Polyline"/>: a <see cref="MauiGtkGraphicsView"/> painting the shape; Points, FillRule repaint it.</summary>
	public class GtkPolylineHandler : GtkShapeViewHandler
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

		internal static new readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create("Points", "FillRule");
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

		internal static new readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create("RadiusX", "RadiusY");
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

		internal static new readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create("CornerRadius");
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

		internal static new readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper = GtkShapeMappers.Create("Color", "CornerRadius");
	}
}
