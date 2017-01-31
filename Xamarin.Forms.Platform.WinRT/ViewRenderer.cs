using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
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
				SetValue(AutomationProperties.AutomationIdProperty, $"{id}_Container");
				Control.SetValue(AutomationProperties.AutomationIdProperty, id);
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
				_defaultAutomationPropertiesName = (string)Control.GetValue(AutomationProperties.NameProperty);

			var elemValue = (string)Element.GetValue(Accessibility.NameProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(AutomationProperties.NameProperty, elemValue);
			else
				Control.SetValue(AutomationProperties.NameProperty, _defaultAutomationPropertiesName);
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
				_defaultAutomationPropertiesAccessibilityView = (AccessibilityView)Control.GetValue(AutomationProperties.AccessibilityViewProperty);

			var newValue = _defaultAutomationPropertiesAccessibilityView;
			var elemValue = (bool?)Element.GetValue(Accessibility.IsInAccessibleTreeProperty);

			if (elemValue == true)
				newValue = AccessibilityView.Content;
			else if (elemValue == false)
				newValue = AccessibilityView.Raw;

			Control.SetValue(AutomationProperties.AccessibilityViewProperty, newValue);
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
				_defaultAutomationPropertiesHelpText = (string)Control.GetValue(AutomationProperties.HelpTextProperty);

			var elemValue = (string)Element.GetValue(Accessibility.HintProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(AutomationProperties.HelpTextProperty, elemValue);
			else
				Control.SetValue(AutomationProperties.HelpTextProperty, _defaultAutomationPropertiesHelpText);
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
				_defaultAutomationPropertiesLabeledBy = (UIElement)Control.GetValue(AutomationProperties.LabeledByProperty);

			var elemValue = (VisualElement)Element.GetValue(Accessibility.LabeledByProperty);
			var renderer = elemValue?.GetOrCreateRenderer();
			var nativeElement = renderer?.GetNativeElement();

			if (nativeElement != null)
				Control.SetValue(AutomationProperties.LabeledByProperty, nativeElement);
			else
				Control.SetValue(AutomationProperties.LabeledByProperty, _defaultAutomationPropertiesLabeledBy);
		}
	}
}