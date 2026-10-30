using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Layouts;
using GtkScrolledWindow = Gtk.ScrolledWindow;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="ScrollViewHandler"/> over a <c>Gtk.ScrolledWindow</c> whose child is a <see cref="MauiGtkPanel"/> laid out by this
	/// handler (the same arrangement as the Windows handler: the handler is the panel's cross-platform layout so padding
	/// and fill alignment are applied around the content).
	/// </summary>
	/// <remarks>Programmatic scrolling is instant (<c>animated</c> requests are honoured without animation).</remarks>
	public class GtkScrollViewHandler : ScrollViewHandler, ICrossPlatformLayout
	{
		MauiGtkPanel? _contentPanel;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkScrollViewHandler()
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkScrollViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		/// <summary>The panel inside the viewport that hosts the scroll content.</summary>
		internal MauiGtkPanel ContentPanel => _contentPanel ?? throw new InvalidOperationException("The platform view has not been created.");

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			_contentPanel = new MauiGtkPanel { CrossPlatformLayout = this };

			return new GtkScrolledWindow
			{
				PropagateNaturalWidth = true,
				PropagateNaturalHeight = true,
				HasFrame = false,
				Child = _contentPanel,
			};
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is GtkScrolledWindow scroller)
			{
				scroller.Hadjustment.ValueChanged += OnScrolled;
				scroller.Vadjustment.ValueChanged += OnScrolled;
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkScrolledWindow scroller)
			{
				scroller.Hadjustment.ValueChanged -= OnScrolled;
				scroller.Vadjustment.ValueChanged -= OnScrolled;
			}

			_contentPanel?.ClearChildren();
			base.DisconnectHandler(platformView);
		}

		void OnScrolled(object? sender, EventArgs e)
		{
			if (PlatformView is not GtkScrolledWindow scroller || VirtualView is null)
				return;

			VirtualView.HorizontalOffset = scroller.Hadjustment.Value;
			VirtualView.VerticalOffset = scroller.Vadjustment.Value;
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var scrollView = VirtualView;
			var padding = scrollView.Padding;
			var presentedContent = scrollView.PresentedContent;

			if (presentedContent is null)
				return new Size(padding.HorizontalThickness, padding.VerticalThickness);

			// Exclude the padding while measuring the internal content ...
			var result = ((ICrossPlatformLayout)scrollView).CrossPlatformMeasure(
				widthConstraint - padding.HorizontalThickness,
				heightConstraint - padding.VerticalThickness);

			// ... and add it back to the final result.
			var fullSize = new Size(result.Width + padding.HorizontalThickness, result.Height + padding.VerticalThickness);

			if (double.IsInfinity(widthConstraint))
				widthConstraint = result.Width;
			if (double.IsInfinity(heightConstraint))
				heightConstraint = result.Height;

			// Content with Fill alignment is stretched to the viewport.
			return fullSize.AdjustForFill(new Rect(0, 0, widthConstraint, heightConstraint), presentedContent);
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds) =>
			((ICrossPlatformLayout)VirtualView).CrossPlatformArrange(bounds);

		internal static void ApplyMappers()
		{
			var mapper = Mapper;

			mapper.MapGtk<IScrollView, IScrollViewHandler, GtkScrolledWindow>(nameof(IScrollView.Content), (h, v, w) =>
			{
				if (h is not GtkScrollViewHandler handler)
					return;

				var panel = handler.ContentPanel;
				panel.ClearChildren();
				if (v.PresentedContent is { } content && h.MauiContext is { } context)
					panel.AddChild((global::Gtk.Widget)content.ToPlatform(context));
			});
			mapper.MapGtk<IScrollView, IScrollViewHandler, GtkScrolledWindow>(nameof(IScrollView.Orientation), (h, v, w) => UpdatePolicies(w, v));
			mapper.MapGtk<IScrollView, IScrollViewHandler, GtkScrolledWindow>(nameof(IScrollView.HorizontalScrollBarVisibility), (h, v, w) => UpdatePolicies(w, v));
			mapper.MapGtk<IScrollView, IScrollViewHandler, GtkScrolledWindow>(nameof(IScrollView.VerticalScrollBarVisibility), (h, v, w) => UpdatePolicies(w, v));
			mapper.MapGtk<IScrollView, IScrollViewHandler, GtkScrolledWindow>(nameof(IView.FlowDirection), (h, v, w) =>
				w.Direction = v.FlowDirection == FlowDirection.RightToLeft ? global::Gtk.TextDirection.Rtl : global::Gtk.TextDirection.Ltr);
			mapper.MapGtk<IScrollView, IScrollViewHandler, GtkScrolledWindow>(nameof(IView.IsEnabled), (h, v, w) => w.Sensitive = v.IsEnabled);

			CommandMapper.MapGtk<IScrollView, IScrollViewHandler, GtkScrolledWindow>(nameof(IScrollView.RequestScrollTo), (h, v, args, w) =>
			{
				if (args is not ScrollToRequest request)
					return;

				SetOffset(w.Hadjustment, request.HorizontalOffset);
				SetOffset(w.Vadjustment, request.VerticalOffset);
				v.ScrollFinished();
			});
			CommandMapper.MapGtk<IScrollView, IScrollViewHandler, GtkScrolledWindow>(nameof(IView.InvalidateMeasure), (h, v, args, w) =>
			{
				w.QueueResize();
				(h as GtkScrollViewHandler)?._contentPanel?.QueueResize();
			});
		}

		static void SetOffset(global::Gtk.Adjustment adjustment, double offset)
		{
			double max = Math.Max(adjustment.Lower, adjustment.Upper - adjustment.PageSize);
			adjustment.Value = Math.Max(adjustment.Lower, Math.Min(offset, max));
		}

		static void UpdatePolicies(GtkScrolledWindow scroller, IScrollView view)
		{
			var horizontal = global::Gtk.PolicyType.Never;
			var vertical = global::Gtk.PolicyType.Never;

			if (view.Orientation is ScrollOrientation.Horizontal or ScrollOrientation.Both)
				horizontal = ToPolicy(view.HorizontalScrollBarVisibility);
			if (view.Orientation is ScrollOrientation.Vertical or ScrollOrientation.Both)
				vertical = ToPolicy(view.VerticalScrollBarVisibility);

			scroller.SetPolicy(horizontal, vertical);
		}

		static global::Gtk.PolicyType ToPolicy(ScrollBarVisibility visibility) => visibility switch
		{
			ScrollBarVisibility.Always => global::Gtk.PolicyType.Always,
			ScrollBarVisibility.Never => global::Gtk.PolicyType.External,
			_ => global::Gtk.PolicyType.Automatic,
		};
	}
}
