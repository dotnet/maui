using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;

namespace Xamarin.Forms.Platform.UWP
{
	public class ViewRenderer<TElement, TNativeElement> : VisualElementRenderer<TElement, TNativeElement> where TElement : View where TNativeElement : FrameworkElement
	{
		string _defaultAutomationPropertiesName;
		AccessibilityView? _defaultAutomationPropertiesAccessibilityView;
		string _defaultAutomationPropertiesHelpText;
		UIElement _defaultAutomationPropertiesLabeledBy;

		protected override void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				UpdateBackgroundColor();
				UpdateFlowDirection();
				UpdateMargins();
			}
		}

		protected override void SetAutomationId(string id)
		{
			if (Control == null)
			{
				base.SetAutomationId(id);
			}
			else
			{
				SetValue(Windows.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, $"{id}_Container");
				Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, id);
			}
		}
		protected override void SetAutomationPropertiesName()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesName();
				return;
			}

			if (Element == null)
				return;

			if (_defaultAutomationPropertiesName == null)
				_defaultAutomationPropertiesName = (string)Control.GetValue(Windows.UI.Xaml.Automation.AutomationProperties.NameProperty);

			var elemValue = (string)Element.GetValue(AutomationProperties.NameProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.NameProperty, elemValue);
			else
				Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.NameProperty, _defaultAutomationPropertiesName);
		}

		protected override void SetAutomationPropertiesAccessibilityView()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesAccessibilityView();
				return;
			}

			if (Element == null)
				return;

			if (!_defaultAutomationPropertiesAccessibilityView.HasValue)
				_defaultAutomationPropertiesAccessibilityView = (AccessibilityView)Control.GetValue(Windows.UI.Xaml.Automation.AutomationProperties.AccessibilityViewProperty);

			var newValue = _defaultAutomationPropertiesAccessibilityView;
			var elemValue = (bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);

			if (elemValue == true)
				newValue = AccessibilityView.Content;
			else if (elemValue == false)
				newValue = AccessibilityView.Raw;

			Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.AccessibilityViewProperty, newValue);
		}

		protected override void SetAutomationPropertiesHelpText()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesHelpText();
				return;
			}

			if (Element == null)
				return;

			if (_defaultAutomationPropertiesHelpText == null)
				_defaultAutomationPropertiesHelpText = (string)Control.GetValue(Windows.UI.Xaml.Automation.AutomationProperties.HelpTextProperty);

			var elemValue = (string)Element.GetValue(AutomationProperties.HelpTextProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.HelpTextProperty, elemValue);
			else
				Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.HelpTextProperty, _defaultAutomationPropertiesHelpText);
		}

		protected override void SetAutomationPropertiesLabeledBy()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesLabeledBy(); 
				return;
			}

			if (Element == null)
				return;

			if (_defaultAutomationPropertiesLabeledBy == null)
				_defaultAutomationPropertiesLabeledBy = (UIElement)Control.GetValue(Windows.UI.Xaml.Automation.AutomationProperties.LabeledByProperty);

			var elemValue = (VisualElement)Element.GetValue(AutomationProperties.LabeledByProperty);
			var renderer = elemValue?.GetOrCreateRenderer();
			var nativeElement = renderer?.GetNativeElement();

			if (nativeElement != null)
				Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.LabeledByProperty, nativeElement);
			else
				Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.LabeledByProperty, _defaultAutomationPropertiesLabeledBy);
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		void UpdateMargins()
		{
			Margin = new Windows.UI.Xaml.Thickness(Element.Margin.Left, Element.Margin.Top, Element.Margin.Right, Element.Margin.Bottom);
		}
	}
}