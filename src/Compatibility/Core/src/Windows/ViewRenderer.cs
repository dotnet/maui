using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.ViewRenderer instead")]
	public partial class ViewRenderer<TElement, TNativeElement> : VisualElementRenderer<TElement, TNativeElement> where TElement : View where TNativeElement : FrameworkElement
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
				UpdateBackground();
				UpdateFlowDirection();
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
				this.SetAutomationPropertiesAutomationId($"{id}_Container");
				Control.SetAutomationPropertiesAutomationId(id);
			}
		}
		protected override void SetAutomationPropertiesName()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesName();
				return;
			}

			_defaultAutomationPropertiesName = Control.SetAutomationPropertiesName(Element, _defaultAutomationPropertiesName);
		}

		protected override void SetAutomationPropertiesAccessibilityView()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesAccessibilityView();
				return;
			}

			_defaultAutomationPropertiesAccessibilityView = Control.SetAutomationPropertiesAccessibilityView(Element, _defaultAutomationPropertiesAccessibilityView);
		}

		protected override void SetAutomationPropertiesHelpText()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesHelpText();
				return;
			}

			_defaultAutomationPropertiesHelpText = Control.SetAutomationPropertiesHelpText(Element, _defaultAutomationPropertiesHelpText);
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
				_defaultAutomationPropertiesLabeledBy = (UIElement)Control.GetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.LabeledByProperty);

			var elemValue = (VisualElement)Element.GetValue(AutomationProperties.LabeledByProperty);
			var renderer = elemValue?.GetOrCreateRenderer();
			var nativeElement = renderer?.GetNativeElement();

			if (nativeElement != null)
				Control.SetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.LabeledByProperty, nativeElement);
			else
				Control.SetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.LabeledByProperty, _defaultAutomationPropertiesLabeledBy);
		}

		[PortHandler]
		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}
	}
}