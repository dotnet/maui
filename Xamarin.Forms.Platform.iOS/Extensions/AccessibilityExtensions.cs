using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public static class AccessibilityExtensions
	{
		public static string SetAccessibilityHint(this UIView Control, Element Element, string _defaultAccessibilityHint = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityHint;

			if (_defaultAccessibilityHint == null)
				_defaultAccessibilityHint = Control.AccessibilityHint;

			Control.AccessibilityHint = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;

			return _defaultAccessibilityHint;
		}

		public static string SetAccessibilityLabel(this UIView Control, Element Element, string _defaultAccessibilityLabel = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityLabel;

			if (_defaultAccessibilityLabel == null)
				_defaultAccessibilityLabel = Control.AccessibilityLabel;

			Control.AccessibilityLabel = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;

			return _defaultAccessibilityLabel;
		}

		public static string SetAccessibilityHint(this UIBarItem Control, Element Element, string _defaultAccessibilityHint = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityHint;

			if (_defaultAccessibilityHint == null)
				_defaultAccessibilityHint = Control.AccessibilityHint;

			Control.AccessibilityHint = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;

			return _defaultAccessibilityHint;

		}

		public static string SetAccessibilityLabel(this UIBarItem Control, Element Element, string _defaultAccessibilityLabel = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityLabel;

			if (_defaultAccessibilityLabel == null)
				_defaultAccessibilityLabel = Control.AccessibilityLabel;

			Control.AccessibilityLabel = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;

			return _defaultAccessibilityLabel;
		}

		public static bool? SetIsAccessibilityElement(this UIView Control, Element Element, bool? _defaultIsAccessibilityElement = null)
		{
			if (Element == null || Control == null)
				return _defaultIsAccessibilityElement;

			if (!_defaultIsAccessibilityElement.HasValue)
				_defaultIsAccessibilityElement = Control.IsAccessibilityElement;

			Control.IsAccessibilityElement = (bool)((bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultIsAccessibilityElement);

			return _defaultIsAccessibilityElement;
		}
	}
}
