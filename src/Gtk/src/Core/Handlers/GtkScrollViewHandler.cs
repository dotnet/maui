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
	/// <remarks>
	/// Programmatic scrolling that is not <see cref="ScrollToRequest.Instant"/> eases to the target over <see cref="ScrollAnimationMilliseconds"/>
	/// on the frame clock of the scroller's window and then calls <see cref="IScrollView.ScrollFinished"/>; a newer request replaces a running
	/// animation without finishing it. <see cref="IScrollView.ScrollFinished"/> completes only the newest request's task: the Controls layer
	/// swaps its completion source before calling the handler and does not complete the task of an in-flight request it replaces (same on
	/// every platform), so the handler must not call it for the superseded one. A scroller that is not mapped has no frame clock and scrolls instantly.
	/// </remarks>
	public class GtkScrollViewHandler : ScrollViewHandler, ICrossPlatformLayout
	{
		/// <summary>Duration of an animated <c>ScrollToAsync</c>.</summary>
		internal const int ScrollAnimationMilliseconds = 250;

		MauiGtkPanel? _contentPanel;
		uint _scrollTickId;
		long _scrollStartTime;
		double _scrollFromX, _scrollFromY, _scrollToX, _scrollToY;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkScrollViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkScrollViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
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
				CancelScrollAnimation(scroller);
				scroller.Hadjustment.ValueChanged -= OnScrolled;
				scroller.Vadjustment.ValueChanged -= OnScrolled;
			}

			_contentPanel?.ClearChildren();
			base.DisconnectHandler(platformView);
		}

		void OnScrolled(object? sender, EventArgs e)
		{
			if (((IElementHandler)this).PlatformView is not GtkScrolledWindow scroller || ((IElementHandler)this).VirtualView is not IScrollView view)
				return;

			view.HorizontalOffset = scroller.Hadjustment.Value;
			view.VerticalOffset = scroller.Vadjustment.Value;
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

		internal static readonly IPropertyMapper<IScrollView, IScrollViewHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IScrollView, IScrollViewHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IScrollView, IScrollViewHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IScrollView, IScrollViewHandler>(ScrollViewHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IScrollView, IScrollViewHandler>(ScrollViewHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IScrollView, IScrollViewHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IScrollView, IScrollViewHandler>(ScrollViewHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IScrollView, IScrollViewHandler>(ScrollViewHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IScrollView, IScrollViewHandler> mapper, GtkCommandMapperBuilder<IScrollView, IScrollViewHandler> commands)
		{
			mapper.MapGtk<GtkScrolledWindow>(nameof(IScrollView.Content), (h, v, w) =>
			{
				if (h is not GtkScrollViewHandler handler)
					return;

				var panel = handler.ContentPanel;
				panel.ClearChildren();
				if (v.PresentedContent is { } content && h.MauiContext is { } context)
					panel.AddChild((global::Gtk.Widget)content.ToPlatform(context));
			});
			mapper.MapGtk<GtkScrolledWindow>(nameof(IScrollView.Orientation), (h, v, w) => UpdatePolicies(w, v));
			mapper.MapGtk<GtkScrolledWindow>(nameof(IScrollView.HorizontalScrollBarVisibility), (h, v, w) => UpdatePolicies(w, v));
			mapper.MapGtk<GtkScrolledWindow>(nameof(IScrollView.VerticalScrollBarVisibility), (h, v, w) => UpdatePolicies(w, v));
			mapper.MapGtk<GtkScrolledWindow>(nameof(IView.FlowDirection), (h, v, w) =>
				w.Direction = v.FlowDirection == FlowDirection.RightToLeft ? global::Gtk.TextDirection.Rtl : global::Gtk.TextDirection.Ltr);
			mapper.MapGtk<GtkScrolledWindow>(nameof(IView.IsEnabled), (h, v, w) => w.Sensitive = v.IsEnabled);

			commands.MapGtk<GtkScrolledWindow>(nameof(IScrollView.RequestScrollTo), (h, v, args, w) =>
			{
				if (args is not ScrollToRequest request)
					return;

				var handler = h as GtkScrollViewHandler;
				handler?.CancelScrollAnimation(w);

				if (request.Instant || handler is null || !handler.TryStartScrollAnimation(w, v, request))
				{
					SetOffset(w.Hadjustment, request.HorizontalOffset);
					SetOffset(w.Vadjustment, request.VerticalOffset);
					v.ScrollFinished();
				}
			});
			commands.MapGtk<GtkScrolledWindow>(nameof(IView.InvalidateMeasure), (h, v, args, w) =>
			{
				w.QueueResize();
				(h as GtkScrollViewHandler)?._contentPanel?.QueueResize();
			});
		}

		static void SetOffset(global::Gtk.Adjustment adjustment, double offset) =>
			adjustment.Value = ClampOffset(adjustment, offset);

		static double ClampOffset(global::Gtk.Adjustment adjustment, double offset)
		{
			double max = Math.Max(adjustment.Lower, adjustment.Upper - adjustment.PageSize);
			return Math.Max(adjustment.Lower, Math.Min(offset, max));
		}

		/// <summary>Starts easing the scroller to <paramref name="request"/>; <see langword="false"/> when it cannot animate (not mapped, already there).</summary>
		bool TryStartScrollAnimation(GtkScrolledWindow scroller, IScrollView view, ScrollToRequest request)
		{
			_scrollFromX = scroller.Hadjustment.Value;
			_scrollFromY = scroller.Vadjustment.Value;
			_scrollToX = ClampOffset(scroller.Hadjustment, request.HorizontalOffset);
			_scrollToY = ClampOffset(scroller.Vadjustment, request.VerticalOffset);

			// Without a mapped window there is no frame clock to drive the animation.
			if (!scroller.IsMapped || (_scrollFromX == _scrollToX && _scrollFromY == _scrollToY))
				return false;

			_scrollStartTime = 0;
			_scrollTickId = scroller.AddTickCallback((widget, clock) => OnScrollTick(scroller, clock));
			return true;
		}

		bool OnScrollTick(GtkScrolledWindow scroller, Gdk.FrameClock clock)
		{
			if (_scrollTickId == 0)
				return false;

			long now = clock.FrameTime;
			if (_scrollStartTime == 0)
				_scrollStartTime = now;

			double progress = Math.Min(1, (now - _scrollStartTime) / (ScrollAnimationMilliseconds * 1000.0));
			double eased = 1 - Math.Pow(1 - progress, 3);

			scroller.Hadjustment.Value = _scrollFromX + (_scrollToX - _scrollFromX) * eased;
			scroller.Vadjustment.Value = _scrollFromY + (_scrollToY - _scrollFromY) * eased;

			if (progress < 1)
				return true;

			_scrollTickId = 0;
			(((IElementHandler)this).VirtualView as IScrollView)?.ScrollFinished();
			return false;
		}

		void CancelScrollAnimation(GtkScrolledWindow scroller)
		{
			if (_scrollTickId == 0)
				return;

			scroller.RemoveTickCallback(_scrollTickId);
			_scrollTickId = 0;
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

			// A viewport sizes its content to the content's minimum: the scrolling directions must report the full content size.
			if (scroller.Child is global::Gtk.Viewport { Child: MauiGtkPanel panel })
			{
				panel.MinimumWidthFollowsContent = horizontal != global::Gtk.PolicyType.Never;
				panel.MinimumHeightFollowsContent = vertical != global::Gtk.PolicyType.Never;
				panel.QueueResize();
			}
		}

		static global::Gtk.PolicyType ToPolicy(ScrollBarVisibility visibility) => visibility switch
		{
			ScrollBarVisibility.Always => global::Gtk.PolicyType.Always,
			ScrollBarVisibility.Never => global::Gtk.PolicyType.External,
			_ => global::Gtk.PolicyType.Automatic,
		};
	}
}
