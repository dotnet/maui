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

		public static void UpdateSemantics(this FrameworkElement nativeView, IView view)
		{
			AutomationProperties.SetName(nativeView, view.Semantics.Description);
			AutomationProperties.SetHelpText(nativeView, view.Semantics.Hint);

			// For now if users are only setting the boolean IsHeading property then we just
			// default WinUI to heading Level1
			if (view.Semantics.IsHeading && view.Semantics.HeadingLevel == SemanticHeadingLevel.Default)
				AutomationProperties.SetHeadingLevel(nativeView, UI.Xaml.Automation.Peers.AutomationHeadingLevel.Level1);
			else if (!view.Semantics.IsHeading && view.Semantics.HeadingLevel == SemanticHeadingLevel.Default)
				AutomationProperties.SetHeadingLevel(nativeView, UI.Xaml.Automation.Peers.AutomationHeadingLevel.None);
			else if (view.Semantics.HeadingLevel == SemanticHeadingLevel.None)
				AutomationProperties.SetHeadingLevel(nativeView, UI.Xaml.Automation.Peers.AutomationHeadingLevel.None);
			else
				AutomationProperties.SetHeadingLevel(nativeView, (UI.Xaml.Automation.Peers.AutomationHeadingLevel)((int)view.Semantics.HeadingLevel));

		}
	}
}