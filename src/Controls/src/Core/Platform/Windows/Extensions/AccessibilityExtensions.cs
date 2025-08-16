using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace Microsoft.Maui.Controls.Platform
{
	public static class AccessibilityExtensions
	{
		public static void SetAutomationPropertiesAutomationId(this FrameworkElement Control, string? id)
		{
			Control.SetValue(NativeAutomationProperties.AutomationIdProperty, id);
		}

		public static string? SetAutomationPropertiesName(this FrameworkElement Control, Element? Element, string? _defaultAutomationPropertiesName = null)
		{
			if (Element is null)
			{
				return _defaultAutomationPropertiesName;
			}

			string? currentValue = null;

			if (_defaultAutomationPropertiesName is null)
			{
				_defaultAutomationPropertiesName = currentValue = (string)Control.GetValue(NativeAutomationProperties.NameProperty);
			}

			var elemValue = (string)Element.GetValue(SemanticProperties.DescriptionProperty);
			string newValue = !string.IsNullOrWhiteSpace(elemValue) ? elemValue : _defaultAutomationPropertiesName;

			if (currentValue is null || currentValue != newValue)
			{
				Control.SetValue(NativeAutomationProperties.NameProperty, _defaultAutomationPropertiesName);
			}

			return _defaultAutomationPropertiesName;
		}

		public static AccessibilityView? SetAutomationPropertiesAccessibilityView(this FrameworkElement Control, Element? Element, AccessibilityView? _defaultAutomationPropertiesAccessibilityView = null)
		{
			if (Element is null)
			{
				return _defaultAutomationPropertiesAccessibilityView;
			}

			AccessibilityView? currentValue = null;

			if (!_defaultAutomationPropertiesAccessibilityView.HasValue)
			{
				_defaultAutomationPropertiesAccessibilityView = currentValue = (AccessibilityView)Control.GetValue(NativeAutomationProperties.AccessibilityViewProperty);
			}

			var newValue = _defaultAutomationPropertiesAccessibilityView;

			var elemValue = (bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);

			if (elemValue == true)
			{
				newValue = AccessibilityView.Content;
			}
			else if (elemValue == false)
			{
				newValue = AccessibilityView.Raw;
			}

			if (currentValue is null || currentValue != newValue)
			{
				Control.SetValue(NativeAutomationProperties.AccessibilityViewProperty, newValue);
			}

			return _defaultAutomationPropertiesAccessibilityView;

		}
		public static string? SetAutomationPropertiesHelpText(this FrameworkElement Control, Element? Element, string? _defaultAutomationPropertiesHelpText = null)
		{
			if (Element == null)
			{
				return _defaultAutomationPropertiesHelpText;
			}

			string? currentValue = null;

			if (_defaultAutomationPropertiesHelpText is null)
			{
				_defaultAutomationPropertiesHelpText = currentValue = (string)Control.GetValue(NativeAutomationProperties.HelpTextProperty);
			}

			var elemValue = (string)Element.GetValue(SemanticProperties.HintProperty);
			string newValue = !string.IsNullOrWhiteSpace(elemValue) ? elemValue : _defaultAutomationPropertiesHelpText;

			if (currentValue is null || newValue != currentValue)
			{
				Control.SetValue(NativeAutomationProperties.HelpTextProperty, _defaultAutomationPropertiesHelpText);
			}

			return _defaultAutomationPropertiesHelpText;
		}

		public static UIElement? SetAutomationPropertiesLabeledBy(
			this FrameworkElement Control,
			Element? Element,
			IMauiContext? mauiContext,
			UIElement? _defaultAutomationPropertiesLabeledBy = null)
		{
			if (Element is null)
			{
				return _defaultAutomationPropertiesLabeledBy;
			}

			// TODO Maui: this is a bit of a hack because Elements
			// currently don't implement IView but they should
			mauiContext ??= (Element as IView)?.Handler?.MauiContext;

			UIElement? currentValue = null;

			if (_defaultAutomationPropertiesLabeledBy is null)
			{
				_defaultAutomationPropertiesLabeledBy = currentValue = (UIElement)Control.GetValue(NativeAutomationProperties.LabeledByProperty);
			}

			var elemValue = (VisualElement)Element.GetValue(SemanticProperties.DescriptionProperty);
			FrameworkElement? nativeElement = null;

			if (mauiContext != null)
			{
				nativeElement = (elemValue as IView)?.ToHandler(mauiContext)?.PlatformView as FrameworkElement;
			}

			UIElement? newValue = nativeElement is not null ? nativeElement : _defaultAutomationPropertiesLabeledBy;

			if (currentValue is null || newValue != currentValue)
			{
				Control.SetValue(SemanticProperties.DescriptionProperty, newValue);
			}

			return _defaultAutomationPropertiesLabeledBy;
		}

		// TODO MAUI: This is not having any effect on anything I've tested yet. See if we need it  
		// after we test the FP and NP w/ back button explicitly enabled. 
		public static void SetBackButtonTitle(this PageControl Control, Element? Element)
		{
			if (Element is null)
			{
				return;
			}

			var elemValue = ConcatenateNameAndHint(Element);

			Control.BackButtonTitle = elemValue;
		}

		static string ConcatenateNameAndHint(Element Element)
		{
			var name = (string)Element.GetValue(SemanticProperties.DescriptionProperty);
			var hint = (string)Element.GetValue(SemanticProperties.HintProperty);

			string separator = string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(hint) ? "" : ". ";

			return string.Join(separator, name, hint);

		}

		public static void SetAutomationProperties(
			this FrameworkElement frameworkElement,
			Element? element,
			IMauiContext? mauiContext,
			string? defaultName = null)
		{
			frameworkElement.SetAutomationPropertiesAutomationId(element?.AutomationId);
			frameworkElement.SetAutomationPropertiesName(element, defaultName);
			frameworkElement.SetAutomationPropertiesHelpText(element);
			frameworkElement.SetAutomationPropertiesLabeledBy(element, mauiContext);
			frameworkElement.SetAutomationPropertiesAccessibilityView(element);
		}
	}
}
