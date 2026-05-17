#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapAutomationId(IElementHandler handler, IElement element, Action<IElementHandler, IElement> baseMethod)
		{
			baseMethod?.Invoke(handler, element);

			if (element is not Element controlsElement ||
				handler.PlatformView is not FrameworkElement platformView ||
				string.IsNullOrWhiteSpace(controlsElement.AutomationId))
				return;

			Platform.AccessibilityExtensions.SetAutomationPropertiesAccessibilityView(platformView, controlsElement);
		}

		public static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, Element element)
		{
			if (handler.IsConnectingHandler() && element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) is null)
				return;

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
