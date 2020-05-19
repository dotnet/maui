using ElmSharp.Accessible;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class AccessibilityExtensions
	{
		public static string SetAccessibilityName(this IAccessibleObject Control, Element Element, string _defaultAccessibilityName = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityName;

			if (_defaultAccessibilityName == null)
				_defaultAccessibilityName = Control.Name;

			Control.Name = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityName;
			return _defaultAccessibilityName;
		}

		public static string SetAccessibilityDescription(this IAccessibleObject Control, Element Element, string _defaultAccessibilityDescription = null)
		{
			if (Element == null || Control == null)
				return _defaultAccessibilityDescription;

			if (_defaultAccessibilityDescription == null)
				_defaultAccessibilityDescription = Control.Description;

			Control.Description = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityDescription;
			return _defaultAccessibilityDescription;
		}

		public static bool? SetIsAccessibilityElement(this IAccessibleObject Control, Element Element, bool? _defaultIsAccessibilityElement = null)
		{
			if (Element == null || Control == null)
				return _defaultIsAccessibilityElement;

			if (!_defaultIsAccessibilityElement.HasValue)
				_defaultIsAccessibilityElement = Control.CanHighlight;

			Control.CanHighlight = (bool)((bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultIsAccessibilityElement);

			// Images are ignored by default on Tizen. So, make accessible in order to enable the gesture and narration
			if (Control.CanHighlight && Element is Image)
			{
				Control.Role = AccessRole.PushButton;
			}
			return _defaultIsAccessibilityElement;
		}

		public static void SetLabeledBy(this IAccessibleObject Control, Element Element)
		{
			if (Element == null || Control == null)
				return;

			var targetElement = (VisualElement)Element.GetValue(AutomationProperties.LabeledByProperty);
			AccessibleObject view = (AccessibleObject)Platform.GetRenderer(targetElement)?.NativeView;
			if (view != null)
			{
				Control.AppendRelation(new LabelledBy() { Target = view });
			}
		}
	}
}
