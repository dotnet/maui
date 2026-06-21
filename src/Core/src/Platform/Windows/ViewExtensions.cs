#nullable enable
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;
using Microsoft.Maui.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WFlowDirection = Microsoft.UI.Xaml.FlowDirection;
using WinPoint = Windows.Foundation.Point;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		public static void TryMoveFocus(this FrameworkElement platformView, FocusNavigationDirection direction)
		{
			if (platformView?.XamlRoot?.Content is UIElement elem)
				FocusManager.TryMoveFocus(direction, new FindNextElementOptions { SearchRoot = elem });
		}

		public static void UpdateIsEnabled(this FrameworkElement platformView, IView view) =>
			(platformView as Control)?.UpdateIsEnabled(view.IsEnabled);

		public static void Focus(this FrameworkElement platformView, FocusRequest request)
		{
			request.TrySetResult(platformView.Focus(FocusState.Programmatic));
		}

		public static void Unfocus(this FrameworkElement platformView, IView view)
		{
			if (platformView is Control control)
			{
				UnfocusControl(control);
			}
		}

		public static void UpdateVisibility(this FrameworkElement platformView, IView view)
		{
			double opacity = view.Opacity;
			var wasCollapsed = platformView.Visibility == UI.Xaml.Visibility.Collapsed;

			switch (view.Visibility)
			{
				case Visibility.Visible:
					platformView.Opacity = opacity;
					platformView.Visibility = UI.Xaml.Visibility.Visible;
					break;
				case Visibility.Hidden:
					platformView.Opacity = 0;
					platformView.Visibility = UI.Xaml.Visibility.Visible;
					break;
				case Visibility.Collapsed:
					platformView.Opacity = opacity;
					platformView.Visibility = UI.Xaml.Visibility.Collapsed;
					break;
			}

			if (view.Visibility != Visibility.Collapsed && wasCollapsed)
			{
				// We may need to force the parent layout (if any) to re-layout to accomodate the new size
				(platformView.Parent as FrameworkElement)?.InvalidateMeasure();
			}
		}

		public static void UpdateClip(this FrameworkElement platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
			{
				wrapper.Clip = view.Clip;
			}
		}

		public static void UpdateShadow(this FrameworkElement platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
			{
				wrapper.Shadow = view.Shadow;
			}
		}

		[Obsolete("IBorder is not used and will be removed in a future release.")]
		public static void UpdateBorder(this FrameworkElement platformView, IView view)
		{
			var border = (view as IBorder)?.Border;
			if (platformView is WrapperView wrapperView)
				wrapperView.Border = border;
		}

		public static void UpdateOpacity(this FrameworkElement platformView, IView view)
		{
			var opacity = view.Visibility == Visibility.Hidden ? 0 : view.Opacity;

			platformView.UpdateOpacity(opacity);
		}

		internal static void UpdateOpacity(this FrameworkElement platformView, double opacity) => platformView.Opacity = (float)opacity;

		public static void UpdateBackground(this ContentPanel platformView, IBorderStroke border)
		{
			var hasBorder = border.Shape != null;

			if (hasBorder)
			{
				platformView?.UpdateBorderBackground(border);
			}
			else if (border is IView v)
			{
				platformView?.UpdatePlatformViewBackground(v);
			}
		}

		public static void UpdateBackground(this FrameworkElement platformView, IView view)
		{
			platformView?.UpdatePlatformViewBackground(view);
		}

		public static void UpdateFlowDirection(this FrameworkElement platformView, IView view)
		{
			var flowDirection = view.FlowDirection;
			switch (flowDirection)
			{
				case FlowDirection.MatchParent:
					platformView.ClearValue(FrameworkElement.FlowDirectionProperty);
					break;
				case FlowDirection.LeftToRight:
					platformView.FlowDirection = WFlowDirection.LeftToRight;
					break;
				case FlowDirection.RightToLeft:
					platformView.FlowDirection = WFlowDirection.RightToLeft;
					break;
			}
		}

		public static void UpdateAutomationId(this FrameworkElement platformView, IView view) =>
			AutomationProperties.SetAutomationId(platformView, view.AutomationId);

		public static void UpdateSemantics(this FrameworkElement platformView, IView view)
		{
			var semantics = view.Semantics;

			if (view is IPicker picker && string.IsNullOrEmpty(semantics?.Description))
				AutomationProperties.SetName(platformView, picker.Title);
			else if (semantics != null)
				AutomationProperties.SetName(platformView, semantics.Description);

			if (semantics == null)
				return;

			AutomationProperties.SetHelpText(platformView, semantics.Hint);
			AutomationProperties.SetHeadingLevel(platformView, (UI.Xaml.Automation.Peers.AutomationHeadingLevel)((int)semantics.HeadingLevel));
		}

		internal static void UpdateProperty(this FrameworkElement platformControl, DependencyProperty property, Color color)
		{
			if (color.IsDefault())
				platformControl.ClearValue(property);
			else
				platformControl.SetValue(property, color.ToPlatform());
		}

		internal static void UpdateProperty(this FrameworkElement platformControl, DependencyProperty property, object? value)
		{
			if (value == null)
				platformControl.ClearValue(property);
			else
				platformControl.SetValue(property, value);
		}

		public static void InvalidateMeasure(this FrameworkElement platformView, IView view)
		{
			platformView.InvalidateMeasure();
		}

		public static void UpdateWidth(this FrameworkElement platformView, IView view)
		{
			// WinUI uses NaN for "unspecified", so as long as we're using NaN for unspecified on the xplat side, 
			// we can just propagate the value straight through
			platformView.Width = view.Width;
		}

		public static void UpdateHeight(this FrameworkElement platformView, IView view)
		{
			// WinUI uses NaN for "unspecified", so as long as we're using NaN for unspecified on the xplat side, 
			// we can just propagate the value straight through
			platformView.Height = view.Height;
		}

		public static void UpdateMinimumHeight(this FrameworkElement platformView, IView view)
		{
			var minHeight = view.MinimumHeight;

			if (Dimension.IsMinimumSet(minHeight))
			{
				// We only use the minimum value if it's been explicitly set; otherwise, clear the local
				// value so that the platform/theme can use the default minimum height for this control
				platformView.MinHeight = minHeight;
			}
			else
			{
				platformView.ClearValue(FrameworkElement.MinHeightProperty);
			}
		}

		public static void UpdateMinimumWidth(this FrameworkElement platformView, IView view)
		{
			var minWidth = view.MinimumWidth;

			if (Dimension.IsMinimumSet(minWidth))
			{
				// We only use the minimum value if it's been explicitly set; otherwise, clear the local
				// value so that the platform/theme can use the default minimum width for this control
				platformView.MinWidth = minWidth;
			}
			else
			{
				platformView.ClearValue(FrameworkElement.MinWidthProperty);
			}
		}

		public static void UpdateMaximumHeight(this FrameworkElement platformView, IView view)
		{
			platformView.MaxHeight = view.MaximumHeight;
		}

		public static void UpdateMaximumWidth(this FrameworkElement platformView, IView view)
		{
			platformView.MaxWidth = view.MaximumWidth;
		}

		internal static void UpdateBorderBackground(this FrameworkElement platformView, IBorderStroke border)
		{
			if (border is IView view)
				(platformView as ContentPanel)?.UpdateBackground(view.Background);

			if (platformView is Control control)
				control.UpdateBackground((Paint?)null);
			else if (platformView is Border b)
				b.UpdateBackground(null);
			else if (platformView is Panel panel)
				panel.UpdateBackground(null);
		}

		internal static void UpdatePlatformViewBackground(this FrameworkElement platformView, IView view)
		{
			(platformView as ContentPanel)?.UpdateBackground(null);

			if (platformView is Control control)
				control.UpdateBackground(view.Background);
			else if (platformView is Border border)
				border.UpdateBackground(view.Background);
			else if (platformView is Panel panel)
				panel.UpdateBackground(view.Background);
		}

		internal static void UpdatePlatformViewBackground(this FrameworkElement platformView, IView view, Brush? defaultBrush = null)
		{
			(platformView as ContentPanel)?.UpdateBackground(null);

			if (platformView is Control control)
				control.UpdateBackground(view.Background, defaultBrush);
			else if (platformView is Border border)
				border.UpdateBackground(view.Background, defaultBrush);
			else if (platformView is Panel panel)
				panel.UpdateBackground(view.Background, defaultBrush);
		}

		public static async Task UpdateBackgroundImageSourceAsync(this FrameworkElement platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		{
			if (platformView is Control control)
				await control.UpdateBackgroundImageSourceAsync(imageSource, provider);
			else if (platformView is Panel panel)
				await panel.UpdateBackgroundImageSourceAsync(imageSource, provider);
		}

		public static void UpdateToolTip(this FrameworkElement platformView, ToolTip? tooltip)
		{
			ToolTipService.SetToolTip(platformView, tooltip?.Content);
		}

		/// <summary>
		/// Background and InputTransparent for Windows layouts are heavily intertwined, so setting one
		/// usually requires setting the other at the same time.
		/// </summary>
		internal static void UpdatePlatformViewBackground(this LayoutPanel layoutPanel, ILayout layout)
		{
			layoutPanel.UpdateInputTransparent(layout.InputTransparent, layout?.Background?.ToPlatform());
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return new Matrix4x4();
			return GetViewTransform(platformView);
		}

		internal static Matrix4x4 GetViewTransform(this FrameworkElement element)
		{
			var root = element?.XamlRoot;
			var offset = element?.TransformToVisual(root?.Content ?? element) as MatrixTransform;
			if (offset == null)
				return new Matrix4x4();
			Matrix matrix = offset.Matrix;
			return new Matrix4x4()
			{
				M11 = (float)matrix.M11,
				M12 = (float)matrix.M12,
				M21 = (float)matrix.M21,
				M22 = (float)matrix.M22,
				Translation = new Vector3((float)matrix.OffsetX, (float)matrix.OffsetY, 0)
			};
		}

		internal static Rect GetPlatformViewBounds(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView != null)
				return platformView.GetPlatformViewBounds();
			return new Rect();
		}

		internal static Rect GetPlatformViewBounds(this FrameworkElement platformView)
		{
			if (platformView == null)
				return new Rect();

			var root = platformView.XamlRoot;
			var offset = platformView.TransformToVisual(root.Content) as UI.Xaml.Media.MatrixTransform;
			if (offset != null)
				return new Rect(offset.Matrix.OffsetX, offset.Matrix.OffsetY, platformView.ActualWidth, platformView.ActualHeight);

			return new Rect();
		}

		internal static Graphics.Rect GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rect GetBoundingBox(this FrameworkElement? platformView)
		{
			if (platformView == null)
				return new Rect();

			var rootView = platformView.XamlRoot.Content;
			if (platformView == rootView)
			{
				if (rootView is not FrameworkElement el)
					return new Rect();

				return new Rect(0, 0, el.ActualWidth, el.ActualHeight);
			}


			var topLeft = platformView.TransformToVisual(rootView).TransformPoint(new WinPoint());
			var topRight = platformView.TransformToVisual(rootView).TransformPoint(new WinPoint(platformView.ActualWidth, 0));
			var bottomLeft = platformView.TransformToVisual(rootView).TransformPoint(new WinPoint(0, platformView.ActualHeight));
			var bottomRight = platformView.TransformToVisual(rootView).TransformPoint(new WinPoint(platformView.ActualWidth, platformView.ActualHeight));


			var x1 = new[] { topLeft.X, topRight.X, bottomLeft.X, bottomRight.X }.Min();
			var x2 = new[] { topLeft.X, topRight.X, bottomLeft.X, bottomRight.X }.Max();
			var y1 = new[] { topLeft.Y, topRight.Y, bottomLeft.Y, bottomRight.Y }.Min();
			var y2 = new[] { topLeft.Y, topRight.Y, bottomLeft.Y, bottomRight.Y }.Max();
			return new Rect(x1, y1, x2 - x1, y2 - y1);
		}

		internal static DependencyObject? GetParent(this FrameworkElement? view)
		{
			return view?.Parent;
		}

		internal static DependencyObject? GetParent(this DependencyObject? view)
		{
			if (view is FrameworkElement pv)
				return pv.Parent;

			return null;
		}

		internal static T? GetChildAt<T>(this DependencyObject view, int index) where T : DependencyObject
		{
			if (VisualTreeHelper.GetChildrenCount(view) >= index)
				return null;

			return VisualTreeHelper.GetChild(view, index) as T;
		}

		internal static void UnfocusControl(Control control)
		{
			if (!control.IsEnabled)
			{
				return;
			}

			var isTabStop = control.IsTabStop;
			control.IsTabStop = false;
			control.IsEnabled = false;
			control.IsEnabled = true;
			control.IsTabStop = isTabStop;
		}

		internal static IWindow? GetHostedWindow(this IView? view)
			=> GetHostedWindow(view?.Handler?.PlatformView as FrameworkElement);

		internal static IWindow? GetHostedWindow(this FrameworkElement? view)
			=> GetWindowForXamlRoot(view?.XamlRoot);

		internal static IWindow? GetWindowForXamlRoot(XamlRoot? root)
		{
			if (root is null)
				return null;

			var windows = WindowExtensions.GetWindows();
			foreach (var window in windows)
			{
				if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window win)
				{
					if (win.Content?.XamlRoot == root)
						return window;
				}
			}

			return null;
		}

		public static void UpdateInputTransparent(this FrameworkElement nativeView, IViewHandler handler, IView view)
		{
			if (nativeView is UIElement element)
			{
				element.IsHitTestVisible = !view.InputTransparent;
			}
		}

		public static void UpdateInputTransparent(this LayoutPanel layoutPanel, ILayoutHandler handler, ILayout layout)
		{
			// Nothing to do yet, but we might need to adjust the wrapper view 
		}
	}
}
