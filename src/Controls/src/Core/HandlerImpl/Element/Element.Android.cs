#nullable enable

using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, Element element)
		{
			Platform.AutomationPropertiesProvider.SetImportantForAccessibility(
				handler.NativeView as Android.Views.View, element);
		}

		public static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, Element element)
		{
			Platform.AutomationPropertiesProvider.SetImportantForAccessibility(
				handler.NativeView as Android.Views.View, element);
		}
	}
}
