using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapIsInAccessibleTree(IElementHandler handler, Element element)
		{
			Platform.AccessibilityExtensions.SetAutomationPropertiesAccessibilityView(
				handler.NativeView as Microsoft.UI.Xaml.FrameworkElement, element);
		}

		public static void MapExcludedWithChildren(IElementHandler handler, Element view)
		{
		}
	}
}
