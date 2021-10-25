#nullable enable

using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapIsInAccessibleTree(IElementHandler handler, Element view)
		{
			Platform.AutomationPropertiesProvider.SetImportantForAccessibility(
				handler.NativeView as Android.Views.View, view);
		}

		public static void MapExcludedWithChildren(IElementHandler handler, Element view)
		{
			Platform.AutomationPropertiesProvider.SetImportantForAccessibility(
				handler.NativeView as Android.Views.View, view);
		}
	}
}
