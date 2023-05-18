#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, Element element)
		{
			// If the user hasn't set IsInAccessibleTree then just don't do anything
			if (!element.IsSet(AutomationProperties.IsInAccessibleTreeProperty))
				return;

			var Control = handler.PlatformView as UIView;

			// iOS sets the default value for IsAccessibilityElement late in the layout cycle
			// But if we set it to false ourselves then that causes it to act like it's false

			// from the docs:
			// https://developer.apple.com/documentation/objectivec/nsobject/1615141-isaccessibilityelement
			// The default value for this property is false unless the receiver is a standard UIKit control,
			// in which case the value is true.
			//
			// So we just base the default on that logic				
			var _defaultIsAccessibilityElement = Control.IsAccessibilityElement || Control is UIControl;

			Control.IsAccessibilityElement = (bool)((bool?)element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultIsAccessibilityElement);
		}

		public static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, Element view)
		{
			if (!view.IsSet(AutomationProperties.ExcludedWithChildrenProperty))
				return;

			var Control = handler.PlatformView as UIView;

			var _defaultAccessibilityElementsHidden = Control.AccessibilityElementsHidden || Control is UIControl;
			Control.AccessibilityElementsHidden = (bool)((bool?)view.GetValue(AutomationProperties.ExcludedWithChildrenProperty) ?? _defaultAccessibilityElementsHidden);
		}

		static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, IElement element)
		{
			if (element is Element e)
				MapAutomationPropertiesIsInAccessibleTree(handler, e);
		}

		static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, IElement element)
		{
			if (element is Element e)
				MapAutomationPropertiesExcludedWithChildren(handler, e);
		}
	}
}
