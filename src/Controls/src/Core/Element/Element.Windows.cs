#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, Element element)
		{
			if (handler.IsConnectingHandler() && element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) is null)
				return;

			if (handler.PlatformView is not FrameworkElement platformView)
				return;

			var isInAccessibleTree = (bool?)element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);
			if (isInAccessibleTree == true)
			{
				platformView.SetValue(NativeAutomationProperties.AccessibilityViewProperty, AccessibilityView.Content);
			}
			else if (isInAccessibleTree == false)
			{
				platformView.SetValue(NativeAutomationProperties.AccessibilityViewProperty, AccessibilityView.Raw);
			}
			else
			{
				// Only clear if this mapper itself previously set the value (Content or Raw).
				// Preserve any AccessibilityView set externally by platform code or a custom handler.
				var current = platformView.ReadLocalValue(NativeAutomationProperties.AccessibilityViewProperty);
				if (current is AccessibilityView.Content or AccessibilityView.Raw)
				{
					platformView.ClearValue(NativeAutomationProperties.AccessibilityViewProperty);
				}
			}
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
