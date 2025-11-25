#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.View"/> that allows date picking.</summary>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<DatePickerHandler>]
	public partial class DatePicker : View, IFontElement, ITextElement, IElementConfiguration<DatePicker>, IDatePicker
	{
		/// <summary>Bindable property for <see cref="Format"/>.</summary>
		public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(DatePicker), "d");

		/// <summary>Bindable property for <see cref="Date"/>.</summary>
		public static readonly BindableProperty DateProperty = BindableProperty.Create(nameof(Date), typeof(DateTime?), typeof(DatePicker), null, BindingMode.TwoWay,
			coerceValue: CoerceDate,
			propertyChanged: DatePropertyChanged,
			defaultValueCreator: (bindable) => DateTime.Today);

		/// <summary>Bindable property for <see cref="MinimumDate"/>.</summary>
		public static readonly BindableProperty MinimumDateProperty = BindableProperty.Create(nameof(MinimumDate), typeof(DateTime?), typeof(DatePicker), new DateTime(1900, 1, 1),
			validateValue: ValidateMinimumDate, coerceValue: CoerceMinimumDate);

		/// <summary>Bindable property for <see cref="MaximumDate"/>.</summary>
		public static readonly BindableProperty MaximumDateProperty = BindableProperty.Create(nameof(MaximumDate), typeof(DateTime?), typeof(DatePicker), new DateTime(2100, 12, 31),
			validateValue: ValidateMaximumDate, coerceValue: CoerceMaximumDate);

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Bindable property for <see cref="FontFamily"/>.</summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>Bindable property for <see cref="FontSize"/>.</summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>Bindable property for <see cref="FontAttributes"/>.</summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
		public static readonly BindableProperty IsOpenProperty =
			BindableProperty.Create(nameof(IDatePicker.IsOpen), typeof(bool), typeof(DatePicker), default, BindingMode.TwoWay,
				propertyChanged: OnIsOpenPropertyChanged);

		readonly Lazy<PlatformConfigurationRegistry<DatePicker>> _platformConfigurationRegistry;

		/// <summary>Initializes a new instance of the DatePicker class.</summary>
		public DatePicker()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<DatePicker>>(() => new PlatformConfigurationRegistry<DatePicker>(this));
		}

		/// <summary>Gets or sets the displayed date. This is a bindable property.</summary>
		public DateTime? Date
		{
			get { return (DateTime?)GetValue(DateProperty); }
			set { SetValue(DateProperty, value); }
		}

		/// <summary>The format of the date to display to the user. This is a dependency property.</summary>
		/// <remarks>Format string is the same is passed to DateTime.ToString (string format).</remarks>
		public string Format
		{
			get { return (string)GetValue(FormatProperty); }
			set { SetValue(FormatProperty, value); }
		}

		TextTransform ITextElement.TextTransform
		{
			get => TextTransform.Default;
			set { }
		}

		/// <summary>The highest date selectable for this DatePicker. This is a bindable property.</summary>
		public DateTime? MaximumDate
		{
			get { return (DateTime?)GetValue(MaximumDateProperty); }
			set { SetValue(MaximumDateProperty, value); }
		}

		/// <summary>The lowest date selectable for this DatePicker. This is a bindable property.</summary>
		public DateTime? MinimumDate
		{
			get { return (DateTime?)GetValue(MinimumDateProperty); }
			set { SetValue(MinimumDateProperty, value); }
		}

		/// <summary>Gets or sets the text color for the date picker. This is a bindable property.</summary>
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DatePicker.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <summary>Gets a value that indicates whether the font for the date picker text is bold, italic, or neither. This is a bindable property.</summary>
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <summary>Gets or sets the font family for the picker text. This is a bindable property.</summary>
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <summary>Gets or sets the size of the font for the text in the picker.</summary>
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		/// <summary>
		/// Gets or sets whether the font size is automatically scaled based on the operating system's accessibility settings.
		/// This is a bindable property.
		/// </summary>
		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		public bool IsOpen
		{
			get => (bool)GetValue(IsOpenProperty);
			set => SetValue(IsOpenProperty, value);
		}

		static void OnIsOpenPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((DatePicker)bindable).OnIsOpenPropertyChanged((bool)oldValue, (bool)newValue);
		}

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
		{
		}

		readonly Queue<Action> _pendingIsOpenActions = new Queue<Action>();

		void OnIsOpenPropertyChanged(bool oldValue, bool newValue)
		{
			if (Handler?.VirtualView is DatePicker)
			{
				HandleIsOpenChanged();
			}
			else
			{
				_pendingIsOpenActions.Enqueue(HandleIsOpenChanged);
			}
		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();

			// Process any pending actions when handler becomes available
			while (_pendingIsOpenActions.Count > 0 && Handler != null)
			{
				var action = _pendingIsOpenActions.Dequeue();
				action.Invoke();
			}
		}

		void HandleIsOpenChanged()
		{
			if (Handler?.VirtualView is not DatePicker datePicker)
				return;

			if (datePicker.IsOpen)
				datePicker.Opened?.Invoke(datePicker, DatePickerOpenedEventArgs.Empty);
			else
				datePicker.Closed?.Invoke(datePicker, DatePickerClosedEventArgs.Empty);
		}

		/// <param name="source">The source parameter.</param>
		/// <param name="textTransform">The textTransform parameter.</param>
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilities.GetTransformedText(source, textTransform);

		public event EventHandler<DateChangedEventArgs> DateSelected;
		public event EventHandler<DatePickerOpenedEventArgs> Opened;
		public event EventHandler<DatePickerClosedEventArgs> Closed;

		static object CoerceDate(BindableObject bindable, object value)
		{
			var picker = (DatePicker)bindable;
			DateTime? dateValue = ((DateTime?)value)?.Date;

			if (dateValue > picker.MaximumDate)
			{
				dateValue = picker.MaximumDate;
			}

			if (dateValue < picker.MinimumDate)
			{
				dateValue = picker.MinimumDate;
			}

			return dateValue;
		}

		static object CoerceMaximumDate(BindableObject bindable, object value)
		{
			DateTime? dateValue = ((DateTime?)value)?.Date;
			var picker = (DatePicker)bindable;

			if (picker.Date > dateValue)
			{
				picker.Date = dateValue;
			}

			return dateValue;
		}

		static object CoerceMinimumDate(BindableObject bindable, object value)
		{
			DateTime? dateValue = ((DateTime?)value)?.Date;
			var picker = (DatePicker)bindable;

			if (picker.Date < dateValue)
			{
				picker.Date = dateValue;
			}

			return dateValue;
		}

		static void DatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var datePicker = (DatePicker)bindable;
			EventHandler<DateChangedEventArgs> selected = datePicker.DateSelected;

			if (selected is not null)
			{
				selected(datePicker, new DateChangedEventArgs((DateTime?)oldValue, (DateTime?)newValue));
			}
		}

		static bool ValidateMaximumDate(BindableObject bindable, object value)
		{
			var newDate = (DateTime?)value;
			var minimumDate = ((DatePicker)bindable).MinimumDate?.Date;

			if (newDate is null || minimumDate is null)
			{
				return true;
			}

			return newDate.Value.Date >= minimumDate;
		}

		static bool ValidateMinimumDate(BindableObject bindable, object value)
		{
			var newDate = (DateTime?)value;
			var maximumDate = ((DatePicker)bindable).MaximumDate?.Date;

			if (newDate is null || maximumDate is null)
			{
				return true;
			}

			return newDate.Value.Date <= maximumDate;
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, DatePicker> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
		{
			InvalidateMeasure();
		}

		Font ITextStyle.Font => this.ToFont();

		DateTime? IDatePicker.Date
		{
			get => Date;
			set => SetValue(DateProperty, value, SetterSpecificity.FromHandler);
		}

		string IDatePicker.Format
		{
			get => Format;
			set => SetValue(FormatProperty, value, SetterSpecificity.FromHandler);
		}

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, Date = {Date}";
		}

		internal override bool TrySetValue(string text)
		{
			if (DateTime.TryParse(text, out DateTime dpResult))
			{
				Date = dpResult;
				return true;
			}

			return false;
		}
	}
}
