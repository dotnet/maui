using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class AccessibilityExtensions
	{
		public static void SetAutomationPropertiesAutomationId(this FrameworkElement Control, string id)
		{
			Control.SetValue(NativeAutomationProperties.AutomationIdProperty, id);
		}

		public static string SetAutomationPropertiesName(this FrameworkElement Control, Element Element, string _defaultAutomationPropertiesName = null)
		{
			if (Element == null)
				return _defaultAutomationPropertiesName;

			if (_defaultAutomationPropertiesName == null)
				_defaultAutomationPropertiesName = (string)Control.GetValue(NativeAutomationProperties.NameProperty);

			var elemValue = (string)Element.GetValue(AutomationProperties.NameProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(NativeAutomationProperties.NameProperty, elemValue);
			else
				Control.SetValue(NativeAutomationProperties.NameProperty, _defaultAutomationPropertiesName);

			return _defaultAutomationPropertiesName;
		}

		public static AccessibilityView? SetAutomationPropertiesAccessibilityView(this FrameworkElement Control, Element Element, AccessibilityView? _defaultAutomationPropertiesAccessibilityView = null)
		{
			if (Element == null)
				return _defaultAutomationPropertiesAccessibilityView;

			if (!_defaultAutomationPropertiesAccessibilityView.HasValue)
				_defaultAutomationPropertiesAccessibilityView = (AccessibilityView)Control.GetValue(NativeAutomationProperties.AccessibilityViewProperty);

			var newValue = _defaultAutomationPropertiesAccessibilityView;

			var elemValue = (bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);

			if (elemValue == true)
				newValue = AccessibilityView.Content;
			else if (elemValue == false)
				newValue = AccessibilityView.Raw;

			Control.SetValue(NativeAutomationProperties.AccessibilityViewProperty, newValue);

			return _defaultAutomationPropertiesAccessibilityView;

		}
		public static string SetAutomationPropertiesHelpText(this FrameworkElement Control, Element Element, string _defaultAutomationPropertiesHelpText = null)
		{
			if (Element == null)
				return _defaultAutomationPropertiesHelpText;

			if (_defaultAutomationPropertiesHelpText == null)
				_defaultAutomationPropertiesHelpText = (string)Control.GetValue(NativeAutomationProperties.HelpTextProperty);

			var elemValue = (string)Element.GetValue(AutomationProperties.HelpTextProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(NativeAutomationProperties.HelpTextProperty, elemValue);
			else
				Control.SetValue(NativeAutomationProperties.HelpTextProperty, _defaultAutomationPropertiesHelpText);

			return _defaultAutomationPropertiesHelpText;
		}

		public static UIElement SetAutomationPropertiesLabeledBy(this FrameworkElement Control, Element Element, UIElement _defaultAutomationPropertiesLabeledBy = null)
		{
			if (Element == null)
				return _defaultAutomationPropertiesLabeledBy;

			if (_defaultAutomationPropertiesLabeledBy == null)
				_defaultAutomationPropertiesLabeledBy = (UIElement)Control.GetValue(NativeAutomationProperties.LabeledByProperty);

			var elemValue = (VisualElement)Element.GetValue(AutomationProperties.LabeledByProperty);

			var renderer = elemValue?.GetOrCreateRenderer();

			var nativeElement = renderer?.GetNativeElement();

			if (nativeElement != null)
				Control.SetValue(AutomationProperties.LabeledByProperty, nativeElement);
			else
				Control.SetValue(NativeAutomationProperties.LabeledByProperty, _defaultAutomationPropertiesLabeledBy);

			return _defaultAutomationPropertiesLabeledBy;
		}

		// TODO: This is not having any effect on anything I've tested yet. See if we need it  
		// after we test the FP and NP w/ back button explicitly enabled. 
		public static void SetBackButtonTitle(this PageControl Control, Element Element)
		{
			if (Element == null)
				return;

			var elemValue = ConcatenateNameAndHint(Element);

			Control.BackButtonTitle = elemValue;
		}

		static string ConcatenateNameAndHint(Element Element)
		{
			string separator;
			
			var name = (string)Element.GetValue(AutomationProperties.NameProperty);

			var hint = (string)Element.GetValue(AutomationProperties.HelpTextProperty);


			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(hint))
				separator = "";

			else
				separator = ". ";


			return string.Join(separator, name, hint);

		}

		internal static void SetAutomationProperties(
			this FrameworkElement frameworkElement, 
			Element element,
			string defaultName = null)
		{
			frameworkElement.SetAutomationPropertiesAutomationId(element?.AutomationId);
			 frameworkElement.SetAutomationPropertiesName(element, defaultName);
			frameworkElement.SetAutomationPropertiesHelpText(element);
			frameworkElement.SetAutomationPropertiesLabeledBy(element);
			frameworkElement.SetAutomationPropertiesAccessibilityView(element);
		}
	}
}
