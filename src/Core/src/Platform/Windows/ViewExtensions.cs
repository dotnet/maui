#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using WFlowDirection = Microsoft.UI.Xaml.FlowDirection;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static void UpdateIsEnabled(this FrameworkElement nativeView, IView view) =>
			(nativeView as Control)?.UpdateIsEnabled(view.IsEnabled);
		
		public static void UpdateVisibility(this FrameworkElement nativeView, IView view)
		{
			double opacity = view.Opacity;

			switch (view.Visibility)
			{
				case Visibility.Visible:
					nativeView.Opacity = opacity;
					nativeView.Visibility = UI.Xaml.Visibility.Visible;
					break;
				case Visibility.Hidden:
					nativeView.Opacity = 0;
					nativeView.Visibility = UI.Xaml.Visibility.Visible;
					break;
				case Visibility.Collapsed:
					nativeView.Opacity = opacity;
					nativeView.Visibility = UI.Xaml.Visibility.Collapsed;
					break;
			}
		}

		public static void UpdateBackground(this FrameworkElement nativeView, IView view)
		{
			if (nativeView is Control control)
				control.UpdateBackground(view.Background);
			else if (nativeView is Border border)
				border.UpdateBackground(view.Background);
			else if (nativeView is Panel panel)
				panel.UpdateBackground(view.Background);
		}

		// TODO ezhart Do we need all three of these? 
		public static void UpdateBackgroundColor(this Control nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.Background = color?.ToNative() ?? defaultBrush ?? nativeControl.Background;

		public static void UpdateBackgroundColor(this Border nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.Background = color?.ToNative() ?? defaultBrush ?? nativeControl.Background;

		public static void UpdateBackgroundColor(this Panel nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.Background = color?.ToNative() ?? defaultBrush ?? nativeControl.Background;

		public static void UpdateFlowDirection(this FrameworkElement nativeView, IView view)
		{
			if (view.FlowDirection.IsRightToLeft())
				nativeView.FlowDirection = WFlowDirection.RightToLeft;
			else if (view.FlowDirection.IsLeftToRight())
				nativeView.FlowDirection = WFlowDirection.LeftToRight;
		}

		public static void UpdateAutomationId(this FrameworkElement nativeView, IView view) =>
			AutomationProperties.SetAutomationId(nativeView, view.AutomationId);

		public static void UpdateSemantics(this FrameworkElement nativeView, IView view)
		{
			var semantics = view.Semantics;
			if (semantics == null)
				return;

			AutomationProperties.SetName(nativeView, semantics.Description);
			AutomationProperties.SetHelpText(nativeView, semantics.Hint);
			AutomationProperties.SetHeadingLevel(nativeView, (UI.Xaml.Automation.Peers.AutomationHeadingLevel)((int)semantics.HeadingLevel));
		}

		internal static void UpdateProperty(this FrameworkElement nativeControl, DependencyProperty property, Color color)
		{
			if (color.IsDefault())
				nativeControl.ClearValue(property);
			else
				nativeControl.SetValue(property, color.ToNative());
		}

		internal static void UpdateProperty(this FrameworkElement nativeControl, DependencyProperty property, object? value)
		{
			if (value == null)
				nativeControl.ClearValue(property);
			else
				nativeControl.SetValue(property, value);
		}

		public static void InvalidateMeasure(this FrameworkElement nativeView, IView view) 
		{
			nativeView.InvalidateMeasure();
		}

		public static void UpdateWidth(this FrameworkElement nativeView, IView view)
		{
			// WinUI uses NaN for "unspecified"
			nativeView.Width = view.Width >= 0 ? view.Width : double.NaN;
		}

		public static void UpdateHeight(this FrameworkElement nativeView, IView view)
		{
			// WinUI uses NaN for "unspecified"
			nativeView.Height = view.Height >= 0 ? view.Height : double.NaN;
		}
	}
}
