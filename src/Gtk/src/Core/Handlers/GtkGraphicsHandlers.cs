using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using CairoCanvas = Microsoft.Maui.Graphics.Cairo.CairoCanvas;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// A <c>Gtk.DrawingArea</c> that renders an <see cref="IDrawable"/> with the Cairo canvas of <c>Microsoft.Maui.Graphics.Cairo</c>, and reports
	/// pointer interactions (press/drag/release/cancel, hover) in the widget's coordinates.
	/// </summary>
	public class MauiGtkGraphicsView : global::Gtk.DrawingArea
	{
		global::Gtk.DrawingAreaDrawFunc? _drawFunc;
		global::Gtk.GestureDrag? _drag;
		global::Gtk.EventControllerMotion? _motion;
		double _startX;
		double _startY;
		bool _interactionsEnabled;

		/// <summary>Creates the view.</summary>
		public MauiGtkGraphicsView()
		{
			// The delegate must stay referenced for as long as GTK can call it.
			_drawFunc = OnDraw;
			DrawFunc = _drawFunc;
		}

		/// <summary>The drawable rendered into the view; set it and call <see cref="Invalidate"/> to repaint.</summary>
		public IDrawable? Drawable { get; set; }

		/// <summary>Number of times the drawable has been rendered (diagnostics and tests).</summary>
		public int DrawCount { get; private set; }

		/// <summary>Requests a repaint.</summary>
		public void Invalidate() => QueueDraw();

		/// <summary>Raised when a press starts an interaction.</summary>
		public event Action<PointF[]>? InteractionStarted;

		/// <summary>Raised while the pointer is dragged.</summary>
		public event Action<PointF[]>? InteractionDragged;

		/// <summary>Raised when the pointer is released; the flag tells whether it is inside the view.</summary>
		public event Action<PointF[], bool>? InteractionEnded;

		/// <summary>Raised when the interaction was cancelled.</summary>
		public event Action? InteractionCancelled;

		/// <summary>Raised when the pointer enters the view.</summary>
		public event Action<PointF[]>? HoverStarted;

		/// <summary>Raised when the pointer moves over the view.</summary>
		public event Action<PointF[]>? HoverMoved;

		/// <summary>Raised when the pointer leaves the view.</summary>
		public event Action? HoverEnded;

		/// <summary>Whether press/drag/hover gestures are installed. They are only needed when the virtual view listens to interactions.</summary>
		public bool InteractionsEnabled
		{
			get => _interactionsEnabled;
			set
			{
				if (_interactionsEnabled == value)
					return;

				_interactionsEnabled = value;

				if (value)
				{
					_drag = new global::Gtk.GestureDrag { PropagationPhase = global::Gtk.PropagationPhase.Bubble };
					_drag.DragStarted += OnDragStarted;
					_drag.DragUpdate += OnDragUpdate;
					_drag.DragEnded += OnDragEnded;
					_drag.Cancel += OnCancel;
					AddController(_drag);

					_motion = new global::Gtk.EventControllerMotion();
					_motion.Enter += OnEnter;
					_motion.Motion += OnMotion;
					_motion.Leave += OnLeave;
					AddController(_motion);
				}
				else
				{
					if (_drag is not null)
					{
						_drag.DragStarted -= OnDragStarted;
						_drag.DragUpdate -= OnDragUpdate;
						_drag.DragEnded -= OnDragEnded;
						_drag.Cancel -= OnCancel;
						RemoveController(_drag);
						_drag = null;
					}

					if (_motion is not null)
					{
						_motion.Enter -= OnEnter;
						_motion.Motion -= OnMotion;
						_motion.Leave -= OnLeave;
						RemoveController(_motion);
						_motion = null;
					}
				}
			}
		}

		void OnDraw(global::Gtk.DrawingArea area, global::Cairo.Context context, int width, int height)
		{
			DrawCount++;

			if (Drawable is not { } drawable || width <= 0 || height <= 0)
				return;

			using var canvas = new CairoCanvas(context);
			drawable.Draw(canvas, new RectF(0, 0, width, height));
		}

		static PointF[] Point(double x, double y) => new[] { new PointF((float)x, (float)y) };

		void OnDragStarted(object o, global::Gtk.DragStartedArgs args)
		{
			_startX = args.StartX;
			_startY = args.StartY;
			InteractionStarted?.Invoke(Point(_startX, _startY));
		}

		void OnDragUpdate(object o, global::Gtk.DragUpdateArgs args) =>
			InteractionDragged?.Invoke(Point(_startX + args.OffsetX, _startY + args.OffsetY));

		void OnDragEnded(object o, global::Gtk.DragEndedArgs args)
		{
			double x = _startX + args.OffsetX;
			double y = _startY + args.OffsetY;
			InteractionEnded?.Invoke(Point(x, y), x >= 0 && y >= 0 && x <= Width && y <= Height);
		}

		void OnCancel(object o, global::Gtk.CancelArgs args) => InteractionCancelled?.Invoke();

		void OnEnter(object o, global::Gtk.EnterArgs args) => HoverStarted?.Invoke(Point(args.X, args.Y));

		void OnMotion(object o, global::Gtk.MotionArgs args) => HoverMoved?.Invoke(Point(args.X, args.Y));

		void OnLeave(object? sender, EventArgs e) => HoverEnded?.Invoke();
	}

	/// <summary>
	/// <see cref="GraphicsViewHandler"/> over a <see cref="MauiGtkGraphicsView"/>: the <c>Drawable</c> is rendered with a Cairo canvas;
	/// <c>Invalidate</c> repaints; pointer interactions are forwarded to the <see cref="IGraphicsView"/> start/drag/end/cancel/hover methods.
	/// </summary>
	public class GtkGraphicsViewHandler : GraphicsViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkGraphicsViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkGraphicsViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkGraphicsView();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not MauiGtkGraphicsView view)
				return;

			view.InteractionStarted += OnStarted;
			view.InteractionDragged += OnDragged;
			view.InteractionEnded += OnEnded;
			view.InteractionCancelled += OnCancelled;
			view.HoverStarted += OnHoverStarted;
			view.HoverMoved += OnHoverMoved;
			view.HoverEnded += OnHoverEnded;
			view.InteractionsEnabled = true;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkGraphicsView view)
			{
				view.InteractionsEnabled = false;
				view.InteractionStarted -= OnStarted;
				view.InteractionDragged -= OnDragged;
				view.InteractionEnded -= OnEnded;
				view.InteractionCancelled -= OnCancelled;
				view.HoverStarted -= OnHoverStarted;
				view.HoverMoved -= OnHoverMoved;
				view.HoverEnded -= OnHoverEnded;
				view.Drawable = null;
			}

			base.DisconnectHandler(platformView);
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IGraphicsView? View => ((IElementHandler)this).VirtualView as IGraphicsView;

		void OnStarted(PointF[] points) => View?.StartInteraction(points);

		void OnDragged(PointF[] points) => View?.DragInteraction(points);

		void OnEnded(PointF[] points, bool inside) => View?.EndInteraction(points, inside);

		void OnCancelled() => View?.CancelInteraction();

		void OnHoverStarted(PointF[] points) => View?.StartHoverInteraction(points);

		void OnHoverMoved(PointF[] points) => View?.MoveHoverInteraction(points);

		void OnHoverEnded() => View?.EndHoverInteraction();

		internal static readonly IPropertyMapper<IGraphicsView, IGraphicsViewHandler> GtkMapper =
			new GtkMapperBuilder<IGraphicsView, IGraphicsViewHandler>(GraphicsViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkGraphicsView>(nameof(IGraphicsView.Drawable), (h, v, w) =>
				{
					w.Drawable = v.Drawable;
					w.Invalidate();
				})
				.Build();

		internal static readonly CommandMapper<IGraphicsView, IGraphicsViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IGraphicsView, IGraphicsViewHandler>(GraphicsViewHandler.CommandMapper)
				.AddViewCommands()
				.MapGtk<MauiGtkGraphicsView>(nameof(IGraphicsView.Invalidate), (h, v, args, w) => w.Invalidate())
				.Build();
	}

	/// <summary>
	/// The GTK implementation of a <c>ShapeView</c>, reusable by handlers that derive from <see cref="ShapeViewHandler"/> (for example the
	/// Controls <c>LineHandler</c>/<c>PathHandler</c>/...): the platform view is a <see cref="MauiGtkGraphicsView"/> drawing a <see cref="ShapeDrawable"/>.
	/// </summary>
	public static class GtkShapeViewBehavior
	{
		/// <summary>Creates the platform view for <paramref name="shapeView"/>.</summary>
		/// <param name="shapeView">The virtual view.</param>
		public static MauiGtkGraphicsView CreatePlatformView(IShapeView shapeView) =>
			new() { Drawable = new ShapeDrawable(shapeView) };

		/// <summary>Repaints the shape of <paramref name="handler"/> (any shape property changed).</summary>
		/// <param name="handler">The handler.</param>
		public static void Invalidate(IViewHandler handler)
		{
			if (handler.PlatformView is MauiGtkGraphicsView view)
				view.Invalidate();
		}

		/// <summary>Adds the shape mappings to <paramref name="mapper"/>; every property of the shape simply repaints.</summary>
		/// <typeparam name="TVirtualView">The virtual view type.</typeparam>
		/// <typeparam name="THandler">The handler interface.</typeparam>
		/// <param name="mapper">The mapper builder.</param>
		public static GtkMapperBuilder<TVirtualView, THandler> AddShapeMappings<TVirtualView, THandler>(this GtkMapperBuilder<TVirtualView, THandler> mapper)
			where TVirtualView : class, IShapeView
			where THandler : class, IViewHandler
		{
			foreach (var key in ShapeKeys)
				mapper.MapGtk<MauiGtkGraphicsView>(key, (h, v, w) => w.Invalidate());

			return mapper;
		}

		/// <summary>The property names of an <see cref="IShapeView"/> that change how it is painted.</summary>
		internal static readonly string[] ShapeKeys =
		{
			nameof(IShapeView.Shape),
			nameof(IShapeView.Aspect),
			nameof(IShapeView.Fill),
			nameof(IShapeView.Stroke),
			nameof(IShapeView.StrokeThickness),
			nameof(IShapeView.StrokeDashPattern),
			nameof(IShapeView.StrokeDashOffset),
			nameof(IShapeView.StrokeLineCap),
			nameof(IShapeView.StrokeLineJoin),
			nameof(IShapeView.StrokeMiterLimit),
			nameof(IShapeView.Background),
		};
	}

	/// <summary>
	/// <see cref="ShapeViewHandler"/> over a <see cref="MauiGtkGraphicsView"/> that paints the shape with a <see cref="ShapeDrawable"/> (fill, stroke,
	/// dashes, caps, joins and miter limit are all taken from the virtual view when painting).
	/// </summary>
	public class GtkShapeViewHandler : ShapeViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkShapeViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkShapeViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => GtkShapeViewBehavior.CreatePlatformView(VirtualView);

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkGraphicsView view)
				view.Drawable = null;

			base.DisconnectHandler(platformView);
		}

		internal static readonly IPropertyMapper<IShapeView, IShapeViewHandler> GtkMapper =
			new GtkMapperBuilder<IShapeView, IShapeViewHandler>(ShapeViewHandler.Mapper)
				.AddViewMappings()
				.AddShapeMappings()
				.Build();

		internal static readonly CommandMapper<IShapeView, IShapeViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IShapeView, IShapeViewHandler>(ShapeViewHandler.CommandMapper).AddViewCommands().Build();
	}

	/// <summary>Paints the dots of an <see cref="IIndicatorView"/>.</summary>
	internal sealed class GtkIndicatorDrawable : IDrawable
	{
		public int Count { get; set; }

		public int Position { get; set; }

		public int MaximumVisible { get; set; } = int.MaxValue;

		public bool HideSingle { get; set; } = true;

		public double IndicatorSize { get; set; } = 6;

		public Color? IndicatorColor { get; set; }

		public Color? SelectedIndicatorColor { get; set; }

		public IShape? Shape { get; set; }

		/// <summary>The number of dots shown.</summary>
		public int VisibleCount
		{
			get
			{
				if (Count <= 0 || (HideSingle && Count == 1))
					return 0;

				return MaximumVisible > 0 ? Math.Min(Count, MaximumVisible) : Count;
			}
		}

		double Gap => IndicatorSize;

		public double DesiredWidth
		{
			get
			{
				int n = VisibleCount;
				return n == 0 ? 0 : n * IndicatorSize + (n - 1) * Gap;
			}
		}

		public double DesiredHeight => VisibleCount == 0 ? 0 : IndicatorSize;

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			int n = VisibleCount;
			if (n == 0)
				return;

			float size = (float)IndicatorSize;
			float gap = (float)Gap;
			float total = n * size + (n - 1) * gap;
			float x = (dirtyRect.Width - total) / 2f;
			float y = (dirtyRect.Height - size) / 2f;
			int selected = Math.Min(Math.Max(Position, 0), n - 1);

			for (int i = 0; i < n; i++)
			{
				var bounds = new Rect(x + i * (size + gap), y, size, size);
				var path = Shape?.PathForBounds(bounds);
				if (path is null)
				{
					path = new PathF();
					path.AppendEllipse((float)bounds.X, (float)bounds.Y, size, size);
				}

				canvas.FillColor = i == selected
					? SelectedIndicatorColor ?? Colors.DarkGray
					: IndicatorColor ?? Colors.LightGray;
				canvas.FillPath(path);
			}
		}
	}

	/// <summary>
	/// <see cref="IndicatorViewHandler"/> over a <see cref="MauiGtkGraphicsView"/> drawing one dot per page; the selected dot has its own colour.
	/// </summary>
	public class GtkIndicatorViewHandler : IndicatorViewHandler
	{
		readonly GtkIndicatorDrawable _drawable = new();

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkIndicatorViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkIndicatorViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkGraphicsView { Drawable = _drawable };

		internal GtkIndicatorDrawable Drawable => _drawable;

		static void Refresh(IIndicatorViewHandler handler, IIndicatorView view, MauiGtkGraphicsView widget)
		{
			if (handler is not GtkIndicatorViewHandler gtk)
				return;

			var drawable = gtk._drawable;
			drawable.Count = view.Count;
			drawable.Position = view.Position;
			drawable.HideSingle = view.HideSingle;
			drawable.MaximumVisible = view.MaximumVisible;
			drawable.IndicatorSize = view.IndicatorSize > 0 ? view.IndicatorSize : 6;
			drawable.IndicatorColor = view.IndicatorColor?.ToColor();
			drawable.SelectedIndicatorColor = view.SelectedIndicatorColor?.ToColor();
			drawable.Shape = view.IndicatorsShape;

			// The dots decide the natural size of the view (a drawing area has none of its own).
			widget.ContentWidth = (int)Math.Ceiling(drawable.DesiredWidth);
			widget.ContentHeight = (int)Math.Ceiling(drawable.DesiredHeight);
			widget.Invalidate();
		}

		internal static readonly IPropertyMapper<IIndicatorView, IIndicatorViewHandler> GtkMapper =
			new GtkMapperBuilder<IIndicatorView, IIndicatorViewHandler>(IndicatorViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkGraphicsView>(nameof(IIndicatorView.Count), (h, v, w) => Refresh(h, v, w))
				.MapGtk<MauiGtkGraphicsView>(nameof(IIndicatorView.Position), (h, v, w) => Refresh(h, v, w))
				.MapGtk<MauiGtkGraphicsView>(nameof(IIndicatorView.HideSingle), (h, v, w) => Refresh(h, v, w))
				.MapGtk<MauiGtkGraphicsView>(nameof(IIndicatorView.MaximumVisible), (h, v, w) => Refresh(h, v, w))
				.MapGtk<MauiGtkGraphicsView>(nameof(IIndicatorView.IndicatorSize), (h, v, w) => Refresh(h, v, w))
				.MapGtk<MauiGtkGraphicsView>(nameof(IIndicatorView.IndicatorColor), (h, v, w) => Refresh(h, v, w))
				.MapGtk<MauiGtkGraphicsView>(nameof(IIndicatorView.SelectedIndicatorColor), (h, v, w) => Refresh(h, v, w))
				.MapGtk<MauiGtkGraphicsView>(nameof(IIndicatorView.IndicatorsShape), (h, v, w) => Refresh(h, v, w))
				.Build();

		internal static readonly CommandMapper<IIndicatorView, IIndicatorViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IIndicatorView, IIndicatorViewHandler>(IndicatorViewHandler.CommandMapper).AddViewCommands().Build();
	}
}
