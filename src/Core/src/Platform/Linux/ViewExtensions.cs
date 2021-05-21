using Gtk;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static void UpdateAutomationId(this Widget nativeView, IView view)
		{

		}

		public static void UpdateBackgroundColor(this Widget nativeView, IView view)
		{
			nativeView.SetBackgroundColor(view.BackgroundColor);
		}

		public static void UpdateIsEnabled(this Widget nativeView, IView view) =>
			nativeView?.UpdateIsEnabled(view.IsEnabled);

		public static void UpdateSemantics(this Widget nativeView, IView view)
		{

		}
	}
}