#nullable disable
using System;
using ObjCRuntime;
using UIKit;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public static class AccessibilityExtensions
	{
		public static void SetAccessibilityProperties(this NativeView nativeViewElement, Element element)
		{
			if (element == null)
				return;

			nativeViewElement.AccessibilityIdentifier = element?.AutomationId;
#pragma warning disable CS0618 // Type or member is obsolete
			SetAccessibilityLabel(nativeViewElement, element);
			SetAccessibilityHint(nativeViewElement, element);
#pragma warning restore CS0618 // Type or member is obsolete
			SetIsAccessibilityElement(nativeViewElement, element);
			SetAccessibilityElementsHidden(nativeViewElement, element);
		}

		[Obsolete("Use Microsoft.Maui.Platform.UpdateSemantics")]
		public static string SetAccessibilityHint(this NativeView Control, Element Element, string _defaultAccessibilityHint = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityHint;

			var semantics = SemanticProperties.UpdateSemantics(Element, null);
			if (semantics is not null)
			{
				Microsoft.Maui.Platform.SemanticExtensions.UpdateSemantics(Control, semantics);
				return String.Empty;
			}

			if (_defaultAccessibilityHint == null)
				_defaultAccessibilityHint = Control.AccessibilityHint;

#pragma warning disable CS0618 // Type or member is obsolete
			Control.AccessibilityHint = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;
#pragma warning restore CS0618 // Type or member is obsolete

			return _defaultAccessibilityHint;
		}

		[Obsolete("Use Microsoft.Maui.Platform.UpdateSemantics")]
		public static string SetAccessibilityLabel(this NativeView Control, Element Element, string _defaultAccessibilityLabel = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityLabel;

			var semantics = SemanticProperties.UpdateSemantics(Element, null);
			if (semantics is not null)
			{
				Microsoft.Maui.Platform.SemanticExtensions.UpdateSemantics(Control, semantics);
				return String.Empty;
			}

			if (_defaultAccessibilityLabel == null)
				_defaultAccessibilityLabel = Control.AccessibilityLabel;

#pragma warning disable CS0618 // Type or member is obsolete
			Control.AccessibilityLabel = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;
#pragma warning restore CS0618 // Type or member is obsolete

			return _defaultAccessibilityLabel;
		}

		[Obsolete("Use Microsoft.Maui.Platform.UpdateSemantics")]
		public static string SetAccessibilityHint(this UIBarItem Control, Element Element, string _defaultAccessibilityHint = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityHint;

			var semantics = SemanticProperties.UpdateSemantics(Element, null);
			if (semantics is not null)
			{
				Microsoft.Maui.Platform.SemanticExtensions.UpdateSemantics(Control, semantics);
				return String.Empty;
			}

			if (_defaultAccessibilityHint == null)
				_defaultAccessibilityHint = Control.AccessibilityHint;

#pragma warning disable CS0618 // Type or member is obsolete
			Control.AccessibilityHint = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;
#pragma warning restore CS0618 // Type or member is obsolete

			return _defaultAccessibilityHint;

		}

		[Obsolete("Use Microsoft.Maui.Platform.UpdateSemantics")]
		public static string SetAccessibilityLabel(this UIBarItem Control, Element Element, string _defaultAccessibilityLabel = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityLabel;

			var semantics = SemanticProperties.UpdateSemantics(Element, null);
			if (semantics is not null)
			{
				semantics.Description = semantics.Description ?? (Element as ToolbarItem)?.Text;
				Microsoft.Maui.Platform.SemanticExtensions.UpdateSemantics(Control, semantics);
				return String.Empty;
			}
			else if (!string.IsNullOrEmpty((Element as ToolbarItem)?.Text))
			{
				// If there is an icon and text on the UIBarItem, the platforms will behave as follows:
				//     On Windows both will be displayed and the text will be read by screenreaders.
				//     On Android only the icon will be displayed but the text will be read by screenreaders.
				//     On MacCatalyst and iOS only the icon will be displayed but the text will NOT be read by screenreaders.
				// As a result, we will add the text value to the Accessibility Label to ensure that the text is read by screenreaders on all the platforms.
				Microsoft.Maui.Platform.SemanticExtensions.UpdateSemantics(Control, new Semantics() { Description = (Element as ToolbarItem)?.Text });
				return String.Empty;
			}

			if (_defaultAccessibilityLabel == null)
				_defaultAccessibilityLabel = Control.AccessibilityLabel;

#pragma warning disable CS0618 // Type or member is obsolete
			Control.AccessibilityLabel = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;
#pragma warning restore CS0618 // Type or member is obsolete

			return _defaultAccessibilityLabel;
		}

		public static bool? SetIsAccessibilityElement(this NativeView Control, Element Element, bool? _defaultIsAccessibilityElement = null)
		{
			if (Element == null || Control == null)
				return _defaultIsAccessibilityElement;

			// If the user hasn't set IsInAccessibleTree then just don't do anything
			if (!Element.IsSet(AutomationProperties.IsInAccessibleTreeProperty))
				return null;

			if (!_defaultIsAccessibilityElement.HasValue)
			{
				// iOS sets the default value for IsAccessibilityElement late in the layout cycle
				// But if we set it to false ourselves then that causes it to act like it's false

				// from the docs:
				// https://developer.apple.com/documentation/objectivec/nsobject/1615141-isaccessibilityelement
				// The default value for this property is false unless the receiver is a standard UIKit control,
				// in which case the value is true.
				//
				// So we just base the default on that logic				
				_defaultIsAccessibilityElement = Control.IsAccessibilityElement || Control is UIControl;
			}

			Control.IsAccessibilityElement = (bool)((bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultIsAccessibilityElement);

			return _defaultIsAccessibilityElement;
		}

		public static bool? SetAccessibilityElementsHidden(this NativeView Control, Element Element, bool? _defaultAccessibilityElementsHidden = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityElementsHidden;

			if (!Element.IsSet(AutomationProperties.ExcludedWithChildrenProperty))
				return null;

			if (!_defaultAccessibilityElementsHidden.HasValue)
			{
				_defaultAccessibilityElementsHidden = Control.AccessibilityElementsHidden || Control is UIControl;
			}

			Control.AccessibilityElementsHidden = (bool)((bool?)Element.GetValue(AutomationProperties.ExcludedWithChildrenProperty) ?? _defaultAccessibilityElementsHidden);

			return _defaultAccessibilityElementsHidden;
		}
	}
}
