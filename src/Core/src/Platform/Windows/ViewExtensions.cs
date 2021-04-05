using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static void UpdateIsEnabled(this FrameworkElement nativeView, IView view) =>
			(nativeView as Control)?.UpdateIsEnabled(view.IsEnabled);

		public static void UpdateBackgroundColor(this FrameworkElement nativeView, IView view) =>
			(nativeView as Control)?.UpdateBackgroundColor(view.BackgroundColor);

		public static void UpdateAutomationId(this FrameworkElement nativeView, IView view) =>
			AutomationProperties.SetAutomationId(nativeView, view.AutomationId);

		internal static void UpdateProperty(this FrameworkElement nativeControl, DependencyProperty property, Color color)
		{
			if (color.IsDefault)
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