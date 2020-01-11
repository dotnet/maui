using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, Windows.UI.Xaml.Controls.DatePicker>, ITabStopOnDescendants
	{
		Brush _defaultBrush;
		bool _fontApplied;
		FontFamily _defaultFontFamily;

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.DateChanged -= OnControlDateChanged;
				Control.Loaded -= ControlOnLoaded;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var picker = new Windows.UI.Xaml.Controls.DatePicker();
					SetNativeControl(picker);
					Control.Loaded += ControlOnLoaded;
					Control.DateChanged += OnControlDateChanged;
				}
				else
				{
					WireUpFormsVsm();
				}

				UpdateMinimumDate();
				UpdateMaximumDate();
				UpdateDate(e.NewElement.Date);
				UpdateFlowDirection();
				UpdateCharacterSpacing();
			}

			base.OnElementChanged(e);
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

		internal override void OnElementFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			base.OnElementFocusChangeRequested(sender, args);

			// Show a picker fly out on focus to match iOS and Android behavior
			var flyout = new DatePickerFlyout { Placement = FlyoutPlacementMode.Bottom, Date = Control.Date };
			flyout.DatePicked += (p, e) => Control.Date = p.Date;
			if (!Element.IsVisible)
				flyout.Placement = FlyoutPlacementMode.Full;
			flyout.ShowAt(Control);
		}

		void WireUpFormsVsm()
		{
			if (!Element.UseFormsVsm())
			{
				return;
			}

			InterceptVisualStateManager.Hook(Control.GetFirstDescendant<StackPanel>(), Control, Element);

			// We also have to intercept the VSM changes on the DatePicker's button
			var button = Control.GetDescendantsByName<Windows.UI.Xaml.Controls.Button>("FlyoutButton").FirstOrDefault();
			InterceptVisualStateManager.Hook(button.GetFirstDescendant<Windows.UI.Xaml.Controls.Grid>(), button, Element);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == DatePicker.DateProperty.PropertyName)
				UpdateDate(Element.Date);
			else if (e.PropertyName == DatePicker.MaximumDateProperty.PropertyName)
				UpdateMaximumDate();
			else if (e.PropertyName == DatePicker.MinimumDateProperty.PropertyName)
				UpdateMinimumDate();
			else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == DatePicker.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
			else if (e.PropertyName == DatePicker.FontAttributesProperty.PropertyName || e.PropertyName == DatePicker.FontFamilyProperty.PropertyName || e.PropertyName == DatePicker.FontSizeProperty.PropertyName)
				UpdateFont();
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnControlDateChanged(object sender, DatePickerValueChangedEventArgs e)
		{
			if (Element == null)
				return;

			if (Element.Date.CompareTo(e.NewDate.Date) != 0)
			{
				var date = e.NewDate.Date.Clamp(Element.MinimumDate, Element.MaximumDate);
				Element.Date = date;

				// set the control date-time to clamped value, if it exceeded the limits at the time of installation.
				if (date != e.NewDate.Date)
				{
					UpdateDate(date);
					Control.UpdateLayout();
				}
				((IVisualElementController)Element).InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
			}
		}

		void UpdateDate(DateTime date)
		{
			if (Control != null)
				Control.Date = new DateTimeOffset(new DateTime(date.Ticks, DateTimeKind.Unspecified));
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		void UpdateCharacterSpacing()
		{
			Control.CharacterSpacing = Element.CharacterSpacing.ToEm();
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			DatePicker datePicker = Element;

			if (datePicker == null)
				return;

			bool datePickerIsDefault = datePicker.FontFamily == null && datePicker.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(DatePicker), true) && datePicker.FontAttributes == FontAttributes.None;

			if (datePickerIsDefault && !_fontApplied)
				return;

			if (datePickerIsDefault)
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
				Control.ApplyFont(datePicker);
			}

			_fontApplied = true;
		}

		void UpdateMaximumDate()
		{
			if (Element != null && Control != null)
				Control.MaxYear = new DateTimeOffset(new DateTime(Element.MaximumDate.Ticks, DateTimeKind.Unspecified));			
		}

		void UpdateMinimumDate()
		{
			DateTime mindate = Element.MinimumDate;

			try
			{
				if (Element != null && Control != null)
					Control.MinYear = new DateTimeOffset(new DateTime(Element.MinimumDate.Ticks, DateTimeKind.Unspecified));
			}
			catch (ArgumentOutOfRangeException)
			{
				// This will be thrown when mindate equals DateTime.MinValue and the UTC offset is positive
				// because the resulting DateTimeOffset.UtcDateTime will be out of range. In that case let's
				// specify the Kind as UTC so there is no offset.
				mindate = DateTime.SpecifyKind(mindate, DateTimeKind.Utc);
				Control.MinYear = new DateTimeOffset(mindate);
			}
		}

		void UpdateTextColor()
		{
			Color color = Element.TextColor;
			Control.Foreground = color.IsDefault ? (_defaultBrush ?? color.ToBrush()) : color.ToBrush();
		}
	}
}