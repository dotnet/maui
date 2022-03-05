using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, Element element)
		{
			Platform.AccessibilityExtensions.SetAutomationPropertiesAccessibilityView(
				handler.PlatformView as Microsoft.UI.Xaml.FrameworkElement, element);
		}

		public static void MapAutomationPropertiesLabeledBy(IElementHandler handler, Element element)
		{
		}

		public static void MapAutomationPropertiesHelpText(IElementHandler handler, Element element)
		{
		}

		public static void MapAutomationPropertiesName(IElementHandler handler, Element element)
		{
		}

		public static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, Element view)
		{
		}
	}
}
