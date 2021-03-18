using System;
using Microsoft.Maui;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static void UpdateIsEnabled(this FrameworkElement nativeView, IView view)
		{			
			if (nativeView is Control control)
				control.IsEnabled = view.IsEnabled;
		}

		public static void UpdateBackgroundColor(this FrameworkElement nativeView, IView view)
		{
			if (!(nativeView is Control control))
				return;

			control.Background = view.BackgroundColor.ToNative();
			
		}

		public static void UpdateAutomationId(this FrameworkElement nativeView, IView view)
		{
			AutomationProperties.SetAutomationId(nativeView, view.AutomationId);
		}
	}
}