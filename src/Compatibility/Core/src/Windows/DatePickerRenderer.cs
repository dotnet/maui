using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class DatePickerRenderer : ViewRenderer<DatePicker, Microsoft.UI.Xaml.Controls.DatePicker>
	{
		WBrush _defaultBrush;
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
					var picker = new Microsoft.UI.Xaml.Controls.DatePicker();
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
			UpdateBackground();
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
			var button = Control.GetDescendantsByName<Microsoft.UI.Xaml.Controls.Button>("FlyoutButton").FirstOrDefault();

			if (button != null)
				InterceptVisualStateManager.Hook(button.GetFirstDescendant<Microsoft.UI.Xaml.Controls.Grid>(), button, Element);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.IsOneOf(DatePicker.DateProperty, DatePicker.FormatProperty))
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

		[PortHandler]
		void OnControlDateChanged(object sender, DatePickerValueChangedEventArgs e)
		{
			if (Element == null)
				return;

			if (Element.Date == null || Element.Date?.CompareTo(e.NewDate.Date) != 0)
			{
				var date = e.NewDate.Date.Clamp(Element.MinimumDate ?? DateTime.MinValue, Element.MaximumDate ?? DateTime.MaxValue);
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

		bool CheckDateFormat()
		{
			return String.IsNullOrWhiteSpace(Element.Format) || Element.Format.Equals("d", StringComparison.Ordinal);
		}

		[PortHandler]
		void UpdateDate(DateTime? date)
		{
			Control?.Date = new DateTimeOffset(new DateTime(date?.Ticks ?? 0, DateTimeKind.Unspecified));

			UpdateDay();
			UpdateMonth();
			UpdateYear();
		}

		[PortHandler]
		void UpdateMonth()
		{
			Control.MonthVisible = true;
			if (CheckDateFormat())
			{
				Control.MonthFormat = "month";
			}
			else if (Element.Format.Equals("D", StringComparison.Ordinal))
			{
				Control.MonthFormat = "month.full";
			}
			else
			{
				var month = Element.Format.Count(x => x == 'M');
				if (month == 0)
					Control.MonthVisible = false;
				else if (month <= 2)
					Control.MonthFormat = "month.numeric";
				else if (month == 3)
					Control.MonthFormat = "month.abbreviated";
				else
					Control.MonthFormat = "month.full";
			}
		}

		[PortHandler]
		void UpdateDay()
		{
			Control.DayVisible = true;
			if (CheckDateFormat())
			{
				Control.DayFormat = "day";
			}
			else if (Element.Format.Equals("D", StringComparison.Ordinal))
			{
				Control.DayFormat = "dayofweek.full";
			}
			else
			{
				var day = Element.Format.Count(x => x == 'd');
				if (day == 0)
					Control.DayVisible = false;
				else if (day == 3)
					Control.DayFormat = "dayofweek.abbreviated";
				else if (day == 4)
					Control.DayFormat = "dayofweek.full";
				else
					Control.DayFormat = "day";
			}
		}

		[PortHandler]
		void UpdateYear()
		{
			Control.YearVisible = true;
			if (CheckDateFormat())
			{
				Control.YearFormat = "year";
			}
			else if (Element.Format.Equals("D", StringComparison.Ordinal))
			{
				Control.YearFormat = "year.full";
			}
			else
			{
				var year = Element.Format.Count(x => x == 'y');
				if (year == 0)
					Control.YearVisible = false;
				else if (year <= 2)
					Control.YearFormat = "year.abbreviated";
				else
					Control.YearFormat = "year.full";
			}
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		[PortHandler]
		void UpdateCharacterSpacing()
		{
			Control.CharacterSpacing = Element.CharacterSpacing.ToEm();
		}

		[PortHandler]
		void UpdateFont()
		{
			if (Control == null)
				return;

			DatePicker datePicker = Element;

			if (datePicker == null)
				return;

			bool datePickerIsDefault =
				datePicker.FontFamily == null &&
#pragma warning disable CS0612 // Type or member is obsolete
				datePicker.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(DatePicker), true) &&
#pragma warning restore CS0612 // Type or member is obsolete
				datePicker.FontAttributes == FontAttributes.None;

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

		[PortHandler]
		void UpdateMaximumDate()
		{
			if (Element != null && Control != null)
				Control.MaxYear = new DateTimeOffset(new DateTime(Element.MaximumDate?.Ticks ?? DateTime.MaxValue.Ticks, DateTimeKind.Unspecified));
		}

		[PortHandler]
		void UpdateMinimumDate()
		{
			DateTime mindate = Element.MinimumDate ?? DateTime.MinValue;

			try
			{
				if (Element != null && Control != null)
					Control.MinYear = new DateTimeOffset(new DateTime(Element.MinimumDate?.Ticks ?? DateTime.MinValue.Ticks, DateTimeKind.Unspecified));
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
			Control.Foreground = color.IsDefault() ? (_defaultBrush ?? color.ToPlatform()) : color.ToPlatform();
		}
	}
}