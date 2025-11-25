#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="Type[@FullName='Microsoft.Maui.Controls.TimePicker']/Docs/*" />
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<TimePickerHandler>]
	public partial class TimePicker : View, IFontElement, ITextElement, IElementConfiguration<TimePicker>, ITimePicker
	{
		/// <summary>Bindable property for <see cref="Format"/>.</summary>
		public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(TimePicker), "t");

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Bindable property for <see cref="Time"/>.</summary>
		public static readonly BindableProperty TimeProperty = BindableProperty.Create(nameof(Time), typeof(TimeSpan?), typeof(TimePicker), new TimeSpan(0), BindingMode.TwoWay,
			validateValue: (bindable, value) =>
			{
				var time = (TimeSpan?)value;
				return time is null || (time?.TotalHours < 24 && time?.TotalMilliseconds >= 0);
			},
			propertyChanged: TimePropertyChanged);

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
			BindableProperty.Create(nameof(ITimePicker.IsOpen), typeof(bool), typeof(TimePicker), default, BindingMode.TwoWay,
				propertyChanged: OnIsOpenPropertyChanged);

		readonly Lazy<PlatformConfigurationRegistry<TimePicker>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public TimePicker()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<TimePicker>>(() => new PlatformConfigurationRegistry<TimePicker>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='Format']/Docs/*" />
		public string Format
		{
			get { return (string)GetValue(FormatProperty); }
			set { SetValue(FormatProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='Time']/Docs/*" />
		public TimeSpan? Time
		{
			get { return (TimeSpan?)GetValue(TimeProperty); }
			set { SetValue(TimeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

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
			((TimePicker)bindable).OnIsOpenPropertyChanged((bool)oldValue, (bool)newValue);
		}

		TextTransform ITextElement.TextTransform
		{
			get => TextTransform.Default;
			set { }
		}

		public event EventHandler<TimeChangedEventArgs> TimeSelected;
		public event EventHandler<TimePickerOpenedEventArgs> Opened;
		public event EventHandler<TimePickerClosedEventArgs> Closed;

		/// <include file="../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="//Member[@MemberName='UpdateFormsText']/Docs/*" />
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilities.GetTransformedText(source, textTransform);

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

		readonly Queue<Action> _pendingIsOpenActions = new Queue<Action>();

		void OnIsOpenPropertyChanged(bool oldValue, bool newValue)
		{
			if (Handler?.VirtualView is TimePicker)
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
			if (Handler?.VirtualView is not TimePicker timePicker)
				return;

			if (timePicker.IsOpen)
				timePicker.Opened?.Invoke(timePicker, TimePickerOpenedEventArgs.Empty);
			else
				timePicker.Closed?.Invoke(timePicker, TimePickerClosedEventArgs.Empty);
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, TimePicker> On<T>() where T : IConfigPlatform
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

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
			=> InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		Font ITextStyle.Font => this.ToFont();

		TimeSpan? ITimePicker.Time
		{
			get => Time;
			set => SetValue(TimeProperty, value, SetterSpecificity.FromHandler);
		}

		static void TimePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is TimePicker timePicker)
				timePicker.TimeSelected?.Invoke(timePicker, new TimeChangedEventArgs((TimeSpan)oldValue, (TimeSpan)newValue));
		}

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, Time = {Time}";
		}

		internal override bool TrySetValue(string text)
		{
			if (TimeSpan.TryParse(text, out TimeSpan tpResult))
			{
				Time = tpResult;
				return true;
			}

			return false;
		}
	}
}
