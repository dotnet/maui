using System;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	/// <summary>
	/// The handler of Controls' <see cref="Frame"/> (a platform-only renderer elsewhere): a <see cref="MauiGtkBorder"/> whose shape is a
	/// rounded rectangle of <see cref="Frame.CornerRadius"/>, filled with the background and stroked 1 px with <see cref="Frame.BorderColor"/>.
	/// </summary>
	/// <remarks>
	/// <para><c>Padding</c> and the border inset come from the frame's own layout. <see cref="Frame.HasShadow"/> is a CSS
	/// <c>box-shadow</c> (rounded like the frame) unless the frame carries an explicit <see cref="VisualElement.Shadow"/>, which the
	/// shared container shadow paints. An unset corner radius (-1) is 5, like the Android card.</para>
	/// </remarks>
#pragma warning disable CS0618 // Frame is obsolete but still supported.
	public class GtkFrameHandler : ContentViewHandler
	{
		const float DefaultCornerRadius = 5;
		const string ShadowCss = "0 2px 5px rgba(0,0,0,0.4)";

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkFrameHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkFrameHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkBorder { CrossPlatformLayout = VirtualView, IsMeasuredByMaui = true };

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

		static float Radius(Frame frame) => frame.CornerRadius < 0 ? DefaultCornerRadius : frame.CornerRadius;

		static void UpdateShape(MauiGtkBorder border, Frame frame)
		{
			float radius = Radius(frame);
			border.Shape = new RoundRectangle { CornerRadius = new CornerRadius(radius) };
			UpdateShadow(border, frame);
		}

		static void UpdateBorder(MauiGtkBorder border, Frame frame)
		{
			border.Stroke = frame.BorderColor is { } color ? new SolidPaint(color) : null;
			border.StrokeThickness = frame.BorderColor is null ? 0 : 1;
		}

		static void UpdateShadow(MauiGtkBorder border, Frame frame)
		{
			var style = GtkStyle.For(border);
			bool css = frame.HasShadow && ((IView)frame).Shadow is null;

			// The CSS radius makes the shadow follow the rounded shape; the widget paints no CSS background or border of its own.
			style.Set("border-radius", css ? FormattableString.Invariant($"{Radius(frame)}px") : null);
			style.Set("box-shadow", css ? ShadowCss : null);
		}

		internal static readonly IPropertyMapper<IContentView, IContentViewHandler> GtkMapper =
			new GtkMapperBuilder<IContentView, IContentViewHandler>(ContentViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkBorder>(nameof(IContentView.Content), (h, v, w) => GtkContainerBehavior.UpdateContent(h, v, w))
				.MapGtk<MauiGtkBorder>(nameof(IView.Background), (h, v, w) => w.Fill = v.Background)
				.MapGtk<MauiGtkBorder>(nameof(Frame.CornerRadius), (h, v, w) => UpdateShape(w, (Frame)v))
				.MapGtk<MauiGtkBorder>(nameof(Frame.BorderColor), (h, v, w) => UpdateBorder(w, (Frame)v))
				.MapGtk<MauiGtkBorder>(nameof(Frame.HasShadow), (h, v, w) => UpdateShadow(w, (Frame)v))
				.MapGtk<MauiGtkBorder>(nameof(IView.Shadow), (h, v, w) => UpdateShadow(w, (Frame)v))
				.Build();

		internal static readonly CommandMapper<IContentView, IContentViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IContentView, IContentViewHandler>(ContentViewHandler.CommandMapper).AddViewCommands().Build();
	}
}
