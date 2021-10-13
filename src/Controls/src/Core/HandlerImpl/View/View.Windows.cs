using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		public static void MapIsInAccessibleTree(IViewHandler handler, View view)
		{
			Platform.AccessibilityExtensions.SetAutomationPropertiesAccessibilityView(
				handler.NativeView as Microsoft.UI.Xaml.FrameworkElement, view);
		}

		public static void MapExcludedWithChildren(IViewHandler handler, View view)
		{
		}
	}
}
