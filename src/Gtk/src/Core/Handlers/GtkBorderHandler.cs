using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// The platform view of a MAUI <c>Border</c>: a layout panel that paints the border shape (fill and stroke) and clips its
	/// content to the shape, using GSK paths so any <see cref="IShape"/> works.
	/// </summary>
	public class MauiGtkBorder : MauiGtkPanel
	{
		IShape? _shape;
		Paint? _fill;
		Paint? _stroke;
		double _strokeThickness;
		Gsk.Path? _path;
		int _pathWidth = -1;
		int _pathHeight = -1;

		/// <summary>The shape of the border; <see langword="null"/> is a rectangle.</summary>
		public IShape? Shape
		{
			get => _shape;
			set { _shape = value; InvalidatePath(); }
		}

		/// <summary>The fill (the view's background).</summary>
		public Paint? Fill
		{
			get => _fill;
			set { _fill = value; QueueDraw(); }
		}

		/// <summary>The stroke paint.</summary>
		public Paint? Stroke
		{
			get => _stroke;
			set { _stroke = value; QueueDraw(); }
		}

		/// <summary>The stroke thickness.</summary>
		public double StrokeThickness
		{
			get => _strokeThickness;
			set { _strokeThickness = value; InvalidatePath(); }
		}

		/// <summary>Line cap of the stroke.</summary>
		public LineCap StrokeLineCap { get; set; }

		/// <summary>Line join of the stroke.</summary>
		public LineJoin StrokeLineJoin { get; set; }

		/// <summary>Miter limit of the stroke.</summary>
		public double StrokeMiterLimit { get; set; } = 10;

		/// <summary>Dash pattern of the stroke (alternating dash/gap lengths in multiples of the thickness).</summary>
		public float[]? StrokeDashPattern { get; set; }

		/// <summary>Dash offset of the stroke.</summary>
		public double StrokeDashOffset { get; set; }

		void InvalidatePath()
		{
			_path = null;
			_pathWidth = _pathHeight = -1;
			QueueDraw();
		}

		Gsk.Path GetPath(int width, int height)
		{
			if (_path is null || width != _pathWidth || height != _pathHeight)
			{
				// Inset by half the stroke so the stroke lies inside the allocation like on the other platforms.
				double inset = _strokeThickness / 2;
				var bounds = new Rect(inset, inset, Math.Max(0, width - _strokeThickness), Math.Max(0, height - _strokeThickness));
				PathF source;
				if (_shape is not null)
				{
					source = _shape.PathForBounds(bounds);
				}
				else
				{
					source = new PathF();
					source.AppendRectangle((float)bounds.X, (float)bounds.Y, (float)bounds.Width, (float)bounds.Height);
				}

				_path = GtkPathConverter.ToGskPath(source);
				_pathWidth = width;
				_pathHeight = height;
			}

			return _path;
		}

		/// <inheritdoc/>
		protected override void OnSnapshot(global::Gtk.Snapshot snapshot)
		{
			int width = Width;
			int height = Height;

			if (width <= 0 || height <= 0)
			{
				base.OnSnapshot(snapshot);
				return;
			}

			var path = GetPath(width, height);
			var bounds = GtkInterop.ToGrapheneRect(0, 0, width, height);

			if (GtkPaintRenderer.CanRender(_fill))
			{
				snapshot.PushFill(path, Gsk.FillRule.Winding);
				GtkPaintRenderer.Render(snapshot, _fill!, bounds, width, height);
				snapshot.Pop();
			}

			if (_shape is not null)
			{
				snapshot.PushFill(path, Gsk.FillRule.Winding);
				base.OnSnapshot(snapshot);
				snapshot.Pop();
			}
			else
			{
				base.OnSnapshot(snapshot);
			}

			if (_strokeThickness > 0 && _stroke is SolidPaint { Color: { } color })
			{
				var stroke = new Gsk.Stroke((float)_strokeThickness);
				stroke.LineCap = StrokeLineCap switch
				{
					LineCap.Round => Gsk.LineCap.Round,
					LineCap.Square => Gsk.LineCap.Square,
					_ => Gsk.LineCap.Butt,
				};
				stroke.LineJoin = StrokeLineJoin switch
				{
					LineJoin.Round => Gsk.LineJoin.Round,
					LineJoin.Bevel => Gsk.LineJoin.Bevel,
					_ => Gsk.LineJoin.Miter,
				};
				stroke.MiterLimit = (float)StrokeMiterLimit;

				if (StrokeDashPattern is { Length: > 0 } pattern)
				{
					var scaled = new float[pattern.Length];
					for (int i = 0; i < pattern.Length; i++)
						scaled[i] = pattern[i] * (float)_strokeThickness;
					stroke.Dash = scaled;
					stroke.DashOffset = (float)(StrokeDashOffset * _strokeThickness);
				}

				snapshot.AppendStroke(path, stroke, color.ToGdkRgba());
			}
		}
	}

	/// <summary>
	/// <see cref="BorderHandler"/> over a <see cref="MauiGtkBorder"/>.
	/// </summary>
	public class GtkBorderHandler : BorderHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkBorderHandler()
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkBorderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkBorder { CrossPlatformLayout = VirtualView };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			GtkContainerBehavior.Connect(platformView, VirtualView);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			GtkContainerBehavior.Disconnect(platformView);
			base.DisconnectHandler(platformView);
		}

		internal static void ApplyMappers()
		{
			var mapper = Mapper;

			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.Content), (h, v, w) => GtkContainerBehavior.UpdateContent(h, v, w));
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.Background), (h, v, w) => w.Fill = v.Background);
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.Shape), (h, v, w) => w.Shape = v.Shape);
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.Stroke), (h, v, w) => w.Stroke = v.Stroke);
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.StrokeThickness), (h, v, w) => w.StrokeThickness = v.StrokeThickness);
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.StrokeLineCap), (h, v, w) => { w.StrokeLineCap = v.StrokeLineCap; w.QueueDraw(); });
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.StrokeLineJoin), (h, v, w) => { w.StrokeLineJoin = v.StrokeLineJoin; w.QueueDraw(); });
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.StrokeDashPattern), (h, v, w) => { w.StrokeDashPattern = v.StrokeDashPattern; w.QueueDraw(); });
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.StrokeDashOffset), (h, v, w) => { w.StrokeDashOffset = v.StrokeDashOffset; w.QueueDraw(); });
			mapper.MapGtk<IBorderView, IBorderHandler, MauiGtkBorder>(nameof(IBorderView.StrokeMiterLimit), (h, v, w) => { w.StrokeMiterLimit = v.StrokeMiterLimit; w.QueueDraw(); });

			BorderHandler.CommandMapper.MapGtk<IBorderView, BorderHandler, MauiGtkBorder>(nameof(IView.InvalidateMeasure), (h, v, args, w) => w.QueueResize());
		}
	}
}
