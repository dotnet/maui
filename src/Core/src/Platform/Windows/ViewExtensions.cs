using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static void UpdateIsEnabled(this FrameworkElement nativeView, IView view) =>
			(nativeView as Control)?.UpdateIsEnabled(view.IsEnabled);

		public static void UpdateBackgroundColor(this FrameworkElement nativeView, IView view)
		{
			if (nativeView is Control control)
				control.UpdateBackgroundColor(view.BackgroundColor);
			else if (nativeView is Border border)
				border.UpdateBackgroundColor(view.BackgroundColor);
			else if (nativeView is Panel panel)
				panel.UpdateBackgroundColor(view.BackgroundColor);
		}

		// TODO ezhart Do we need all three of these? 
		public static void UpdateBackgroundColor(this Control nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.Background = color?.ToNative() ?? defaultBrush ?? nativeControl.Background;

		public static void UpdateBackgroundColor(this Border nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.Background = color?.ToNative() ?? defaultBrush ?? nativeControl.Background;

		public static void UpdateBackgroundColor(this Panel nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.Background = color?.ToNative() ?? defaultBrush ?? nativeControl.Background;

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
	}
}