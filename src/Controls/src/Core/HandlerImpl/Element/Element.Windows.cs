using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapIsInAccessibleTree(IElementHandler handler, Element view)
		{
			Platform.AccessibilityExtensions.SetAutomationPropertiesAccessibilityView(
				handler.NativeView as Microsoft.UI.Xaml.FrameworkElement, view);
		}

		public static void MapExcludedWithChildren(IElementHandler handler, Element view)
		{
		}
	}
}
