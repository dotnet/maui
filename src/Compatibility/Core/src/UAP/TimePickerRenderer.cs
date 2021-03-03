using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Controls.Primitives;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, Microsoft.UI.Xaml.Controls.TimePicker>, ITabStopOnDescendants
	{
		WBrush _defaultBrush;
		bool _fontApplied;
		FontFamily _defaultFontFamily;

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.TimeChanged -= OnControlTimeChanged;
				Control.Loaded -= ControlOnLoaded;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var picker = new Microsoft.UI.Xaml.Controls.TimePicker();

					SetNativeControl(picker);

					Control.TimeChanged += OnControlTimeChanged;
					Control.Loaded += ControlOnLoaded;
				}
				else
				{
					WireUpFormsVsm();
				}

				UpdateTime();
				UpdateCharacterSpacing();
				UpdateFlowDirection();
			}
		}

		internal override void OnElementFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			base.OnElementFocusChangeRequested(sender, args);

			// Show a picker fly out on focus to match iOS and Android behavior
			var flyout = new TimePickerFlyout { Placement = FlyoutPlacementMode.Bottom, Time = Control.Time };
			flyout.TimePicked += (p, e) => Control.Time = p.Time;
			if (!Element.IsVisible)
				flyout.Placement = FlyoutPlacementMode.Full;
			flyout.ShowAt(Control);
		}

		void ControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			WireUpFormsVsm();

			// The defaults from the control template won't be available
			// right away; we have to wait until after the template has been applied
			_defaultBrush = Control.Foreground;
			_defaultFontFamily = Control.FontFamily;
			UpdateFont();
			UpdateTextColor();
		}

		void WireUpFormsVsm()
		{
			if (!Element.UseFormsVsm())
			{
				return;
			}

			InterceptVisualStateManager.Hook(Control.GetFirstDescendant<StackPanel>(), Control, Element);

			// We also have to intercept the VSM changes on the TimePicker's button
			var button = Control.GetDescendantsByName<Microsoft.UI.Xaml.Controls.Button>("FlyoutButton").FirstOrDefault();

			if (button != null)
				InterceptVisualStateManager.Hook(button.GetFirstDescendant<Microsoft.UI.Xaml.Controls.Grid>(), button, Element);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName || e.PropertyName == TimePicker.FormatProperty.PropertyName)
				UpdateTime();
			else if (e.PropertyName == TimePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == TimePicker.FontAttributesProperty.PropertyName || e.PropertyName == TimePicker.FontFamilyProperty.PropertyName || e.PropertyName == TimePicker.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == TimePicker.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();

			if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnControlTimeChanged(object sender, TimePickerValueChangedEventArgs e)
		{
			Element.Time = e.NewTime;
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}
		
		void PickerOnForceInvalidate(object sender, EventArgs eventArgs)
		{
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			TimePicker timePicker = Element;

			if (timePicker == null)
				return;

			bool timePickerIsDefault = timePicker.FontFamily == null && timePicker.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(TimePicker), true) && timePicker.FontAttributes == FontAttributes.None;

			if (timePickerIsDefault && !_fontApplied)
				return;

			if (timePickerIsDefault)
			{
				// ReSharper disable AccessToStaticMemberViaDerivedType
				Control.ClearValue(ComboBox.FontStyleProperty);
				Control.ClearValue(ComboBox.FontSizeProperty);
				Control.ClearValue(ComboBox.FontFamilyProperty);
				Control.ClearValue(ComboBox.FontWeightProperty);
				Control.ClearValue(ComboBox.FontStretchProperty);
				// ReSharper restore AccessToStaticMemberViaDerivedType
			}
			else
			{
				Control.ApplyFont(timePicker);
			}

			_fontApplied = true;
		}

		void UpdateTime()
		{
			Control.Time = Element.Time;
			if (Element.Format?.Contains('H') == true)
			{
				Control.ClockIdentifier = "24HourClock";
			}
			else
			{
				Control.ClockIdentifier = "12HourClock";
			}
		}

		void UpdateCharacterSpacing()
		{
			Control.CharacterSpacing = Element.CharacterSpacing.ToEm();
		}

		void UpdateTextColor()
		{
			Color color = Element.TextColor;
			Control.Foreground = color.IsDefault ? (_defaultBrush ?? color.ToBrush()) : color.ToBrush();
		}
	}
}