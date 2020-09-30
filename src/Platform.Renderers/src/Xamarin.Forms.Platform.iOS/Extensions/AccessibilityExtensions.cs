#if __MOBILE__
using UIKit;
using NativeView = UIKit.UIView;

namespace Xamarin.Forms.Platform.iOS
#else
using NativeView = AppKit.NSView;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public static class AccessibilityExtensions
	{
		public static void SetAccessibilityProperties(this NativeView nativeViewElement, Element element)
		{
			if (element == null)
				return;

			nativeViewElement.AccessibilityIdentifier = element?.AutomationId;
			SetAccessibilityLabel(nativeViewElement, element);
			SetAccessibilityHint(nativeViewElement, element);
			SetIsAccessibilityElement(nativeViewElement, element);
		}

		public static string SetAccessibilityHint(this NativeView Control, Element Element, string _defaultAccessibilityHint = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityHint;
#if __MOBILE__
			if (_defaultAccessibilityHint == null)
				_defaultAccessibilityHint = Control.AccessibilityHint;

			Control.AccessibilityHint = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;
#else
			if (_defaultAccessibilityHint == null)
				_defaultAccessibilityHint = Control.AccessibilityTitle;

			Control.AccessibilityTitle = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;
#endif

			return _defaultAccessibilityHint;
		}

		public static string SetAccessibilityLabel(this NativeView Control, Element Element, string _defaultAccessibilityLabel = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityLabel;

			if (_defaultAccessibilityLabel == null)
				_defaultAccessibilityLabel = Control.AccessibilityLabel;

			Control.AccessibilityLabel = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;

			return _defaultAccessibilityLabel;
		}

#if __MOBILE__
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
#endif

		public static bool? SetIsAccessibilityElement(this NativeView Control, Element Element, bool? _defaultIsAccessibilityElement = null)
		{
			if (Element == null || Control == null)
				return _defaultIsAccessibilityElement;

#if __MOBILE__
			if (!_defaultIsAccessibilityElement.HasValue)
				_defaultIsAccessibilityElement = Control.IsAccessibilityElement;

			Control.IsAccessibilityElement = (bool)((bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultIsAccessibilityElement);
#else
			if (!_defaultIsAccessibilityElement.HasValue)
				_defaultIsAccessibilityElement = Control.AccessibilityElement;

			Control.AccessibilityElement = (bool)((bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultIsAccessibilityElement);
#endif

			return _defaultIsAccessibilityElement;
		}
	}
}
