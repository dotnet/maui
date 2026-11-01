using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Hosting
{
	/// <summary>
	/// GTK implementations of the <see cref="ViewHandler.ViewMapper"/> / <see cref="ViewHandler.ViewCommandMapper"/> properties that
	/// every view shares: visibility, opacity, background, enabled state, input transparency, transforms, automation id,
	/// semantics, tooltip, clip, shadow and context flyout.
	/// </summary>
	internal static class GtkViewMappers
	{
		/// <summary>Adds the shared view properties to the mapper of a view handler.</summary>
		internal static GtkMapperBuilder<TVirtualView, THandler> AddViewMappings<TVirtualView, THandler>(this GtkMapperBuilder<TVirtualView, THandler> mapper)
			where TVirtualView : class, IView
			where THandler : class, IViewHandler
		{
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.AutomationId), (h, v, w) => UpdateAutomationId(w, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.Visibility), (h, v, w) => UpdateVisibility(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.Opacity), (h, v, w) => UpdateOpacity(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.Background), (h, v, w) => GtkBackgroundImage.Update(h, w, v.Background));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.IsEnabled), (h, v, w) => w.Sensitive = v.IsEnabled);
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.InputTransparent), (h, v, w) => UpdateInputTransparent(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.FlowDirection), (h, v, w) => UpdateFlowDirection(w, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.Semantics), (h, v, w) => GtkAccessibility.UpdateSemantics(w, v));
			mapper.MapGtk<global::Gtk.Widget>(GtkAccessibility.IsInAccessibleTreeKey, (h, v, w) => GtkAccessibility.UpdateHidden(w, v));
			mapper.MapGtk<global::Gtk.Widget>(GtkAccessibility.ExcludedWithChildrenKey, (h, v, w) => GtkAccessibility.UpdateHidden(w, v));
			mapper.MapGtk<global::Gtk.Widget>(GtkAccessibility.LabeledByKey, (h, v, w) => GtkAccessibility.UpdateLabeledBy(w, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IToolTipElement.ToolTip), (h, v, w) => UpdateToolTip(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.Clip), (h, v, w) => UpdateClip(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.Shadow), (h, v, w) => UpdateShadow(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IContextFlyoutElement.ContextFlyout), (h, v, w) => GtkContextFlyout.Update(h, v));

			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.TranslationX), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.TranslationY), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.Scale), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.ScaleX), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.ScaleY), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.Rotation), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.RotationX), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.RotationY), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.AnchorX), (h, v, w) => UpdateTransform(h, v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IView.AnchorY), (h, v, w) => UpdateTransform(h, v));

			return mapper;
		}

		/// <summary>Adds the shared view commands to the command mapper of a view handler.</summary>
		internal static GtkCommandMapperBuilder<TVirtualView, THandler> AddViewCommands<TVirtualView, THandler>(this GtkCommandMapperBuilder<TVirtualView, THandler> commands)
			where TVirtualView : class, IView
			where THandler : class, IViewHandler
		{
			commands.MapGtk<global::Gtk.Widget>(nameof(IView.InvalidateMeasure), (h, v, args, w) => (h.ContainerView as global::Gtk.Widget ?? w).QueueResize());
			commands.MapGtk<global::Gtk.Widget>(nameof(IView.Focus), (h, v, args, w) =>
			{
				if (args is FocusRequest request)
					request.TrySetResult(w.GrabFocus());
			});
			commands.MapGtk<global::Gtk.Widget>(GtkAccessibility.SetSemanticFocusCommand, (h, v, args, w) => GtkAccessibility.SetSemanticFocus(w));
			commands.MapGtk<global::Gtk.Widget>(nameof(IView.Unfocus), (h, v, args, w) =>
			{
				if (w.HasFocus && w.Root is global::Gtk.Window window)
					window.Focus = null;
			});

			return commands;
		}

		/// <summary>The widget a view's layout and chrome properties apply to: its container when it has one.</summary>
		static global::Gtk.Widget Outer(IViewHandler handler, global::Gtk.Widget platformView) =>
			handler.ContainerView as global::Gtk.Widget ?? platformView;

		static void UpdateAutomationId(global::Gtk.Widget widget, IView view)
		{
			// The widget name is what CSS (#id) and GTK Inspector show; it is the closest GTK has to an automation id.
			widget.Name = view.AutomationId ?? string.Empty;
		}

		static void UpdateVisibility(IViewHandler handler, IView view)
		{
			var platformView = (global::Gtk.Widget)handler.PlatformView!;
			var outer = Outer(handler, platformView);

			// Hidden keeps its layout slot, so it stays "visible" to GTK and is made transparent instead.
			bool visible = view.Visibility != Visibility.Collapsed;
			outer.Visible = visible;
			if (!ReferenceEquals(outer, platformView))
				platformView.Visible = visible;

			// GTK rewrites the accessible Hidden state when a widget is shown or hidden: re-apply an exclusion from the accessibility tree.
			GtkAccessibility.UpdateHidden(platformView, view);

			UpdateOpacity(handler, view);
		}

		static void UpdateOpacity(IViewHandler handler, IView view)
		{
			var platformView = (global::Gtk.Widget)handler.PlatformView!;
			var outer = Outer(handler, platformView);

			bool hidden = view.Visibility == Visibility.Hidden;
			outer.Opacity = hidden ? 0 : view.Opacity;
			outer.CanTarget = !hidden && !view.InputTransparent;

			// A container carries the opacity; the wrapped control stays opaque so it is not applied twice.
			if (!ReferenceEquals(outer, platformView))
				platformView.Opacity = 1;
		}

		static void UpdateInputTransparent(IViewHandler handler, IView view)
		{
			var platformView = (global::Gtk.Widget)handler.PlatformView!;
			var outer = Outer(handler, platformView);
			bool transparent = view.InputTransparent || view.Visibility == Visibility.Hidden;

			outer.CanTarget = !transparent;
			if (!ReferenceEquals(outer, platformView))
				platformView.CanTarget = !transparent;
		}

		static void UpdateFlowDirection(global::Gtk.Widget widget, IView view)
		{
			switch (view.FlowDirection)
			{
				case FlowDirection.LeftToRight:
					widget.Direction = global::Gtk.TextDirection.Ltr;
					break;
				case FlowDirection.RightToLeft:
					widget.Direction = global::Gtk.TextDirection.Rtl;
					break;
			}
		}

		static void UpdateToolTip(IViewHandler handler, IView view)
		{
			var platformView = (global::Gtk.Widget)handler.PlatformView!;
			var outer = Outer(handler, platformView);
			outer.TooltipText = (view as IToolTipElement)?.ToolTip?.Content?.ToString();
		}

		static void UpdateClip(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			if (handler.ContainerView is GtkWrapperView container)
				container.Clip = view.Clip;
		}

		static void UpdateShadow(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			if (handler.ContainerView is GtkWrapperView container)
				container.Shadow = view.Shadow;
		}

		static void UpdateTransform(IViewHandler handler, IView view)
		{
			var platformView = (global::Gtk.Widget)handler.PlatformView!;
			var outer = Outer(handler, platformView);
			var state = GtkViewState.For(outer);

			state.TranslationX = view.TranslationX;
			state.TranslationY = view.TranslationY;
			state.Rotation = view.Rotation;
			state.RotationX = view.RotationX;
			state.RotationY = view.RotationY;
			state.ScaleX = view.Scale * view.ScaleX;
			state.ScaleY = view.Scale * view.ScaleY;
			state.AnchorX = view.AnchorX;
			state.AnchorY = view.AnchorY;

			GtkPlatformViewBackend.Reallocate(outer);
		}
	}
}
