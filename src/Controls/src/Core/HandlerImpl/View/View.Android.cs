#nullable enable

using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		partial void HandlerChangedPartial()
		{
			this.AddOrRemoveControlsAccessibilityDelegate();
		}

		public static void MapIsInAccessibleTree(IViewHandler handler, View view)
		{
			Platform.AutomationPropertiesProvider.SetImportantForAccessibility(
				handler.NativeView as Android.Views.View, view);
		}

		public static void MapExcludedWithChildren(IViewHandler handler, View view)
		{
			Platform.AutomationPropertiesProvider.SetImportantForAccessibility(
				handler.NativeView as Android.Views.View, view);
		}
	}
}
