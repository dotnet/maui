using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_DatePickerRenderer))]
	public class DatePicker : View, ITextElement,IElementConfiguration<DatePicker>
	{
		public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(DatePicker), "d");

		public static readonly BindableProperty DateProperty = BindableProperty.Create(nameof(Date), typeof(DateTime), typeof(DatePicker), default(DateTime), BindingMode.TwoWay,
			coerceValue: CoerceDate,
			propertyChanged: DatePropertyChanged,
			defaultValueCreator: (bindable) => DateTime.Today);

		public static readonly BindableProperty MinimumDateProperty = BindableProperty.Create(nameof(MinimumDate), typeof(DateTime), typeof(DatePicker), new DateTime(1900, 1, 1),
			validateValue: ValidateMinimumDate, coerceValue: CoerceMinimumDate);

		public static readonly BindableProperty MaximumDateProperty = BindableProperty.Create(nameof(MaximumDate), typeof(DateTime), typeof(DatePicker), new DateTime(2100, 12, 31),
			validateValue: ValidateMaximumDate, coerceValue: CoerceMaximumDate);

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		readonly Lazy<PlatformConfigurationRegistry<DatePicker>> _platformConfigurationRegistry;

		public DatePicker()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<DatePicker>>(() => new PlatformConfigurationRegistry<DatePicker>(this));
		}

		public DateTime Date
		{
			get { return (DateTime)GetValue(DateProperty); }
			set { SetValue(DateProperty, value); }
		}

		public string Format
		{
			get { return (string)GetValue(FormatProperty); }
			set { SetValue(FormatProperty, value); }
		}

		public DateTime MaximumDate
		{
			get { return (DateTime)GetValue(MaximumDateProperty); }
			set { SetValue(MaximumDateProperty, value); }
		}

		public DateTime MinimumDate
		{
			get { return (DateTime)GetValue(MinimumDateProperty); }
			set { SetValue(MinimumDateProperty, value); }
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		public event EventHandler<DateChangedEventArgs> DateSelected;

		static object CoerceDate(BindableObject bindable, object value)
		{
			var picker = (DatePicker)bindable;
			DateTime dateValue = ((DateTime)value).Date;

			if (dateValue > picker.MaximumDate)
				dateValue = picker.MaximumDate;

			if (dateValue < picker.MinimumDate)
				dateValue = picker.MinimumDate;

			return dateValue;
		}

		static object CoerceMaximumDate(BindableObject bindable, object value)
		{
			DateTime dateValue = ((DateTime)value).Date;
			var picker = (DatePicker)bindable;
			if (picker.Date > dateValue)
				picker.Date = dateValue;

			return dateValue;
		}

		static object CoerceMinimumDate(BindableObject bindable, object value)
		{
			DateTime dateValue = ((DateTime)value).Date;
			var picker = (DatePicker)bindable;
			if (picker.Date < dateValue)
				picker.Date = dateValue;

			return dateValue;
		}

		static void DatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var datePicker = (DatePicker)bindable;
			EventHandler<DateChangedEventArgs> selected = datePicker.DateSelected;

			if (selected != null)
				selected(datePicker, new DateChangedEventArgs((DateTime)oldValue, (DateTime)newValue));
		}

		static bool ValidateMaximumDate(BindableObject bindable, object value)
		{
			return (DateTime)value >= ((DatePicker)bindable).MinimumDate;
		}

		static bool ValidateMinimumDate(BindableObject bindable, object value)
		{
			return (DateTime)value <= ((DatePicker)bindable).MaximumDate;
		}

		public IPlatformElementConfiguration<T, DatePicker> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}
	}
}