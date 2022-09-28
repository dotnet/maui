using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Entry']/Docs/*" />
	public partial class Entry : InputView, IFontElement, ITextAlignmentElement, IEntryController, IElementConfiguration<Entry>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='ReturnTypeProperty']/Docs/*" />
		public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(Entry), ReturnType.Default);

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='ReturnCommandProperty']/Docs/*" />
		public static readonly BindableProperty ReturnCommandProperty = BindableProperty.Create(nameof(ReturnCommand), typeof(ICommand), typeof(Entry), default(ICommand));

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='ReturnCommandParameterProperty']/Docs/*" />
		public static readonly BindableProperty ReturnCommandParameterProperty = BindableProperty.Create(nameof(ReturnCommandParameter), typeof(object), typeof(Entry), default(object));

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='PlaceholderProperty']/Docs/*" />
		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='PlaceholderColorProperty']/Docs/*" />
		public new static readonly BindableProperty PlaceholderColorProperty = InputView.PlaceholderColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='IsPasswordProperty']/Docs/*" />
		public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(Entry), default(bool));

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='TextProperty']/Docs/*" />
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='KeyboardProperty']/Docs/*" />
		public new static readonly BindableProperty KeyboardProperty = InputView.KeyboardProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='HorizontalTextAlignmentProperty']/Docs/*" />
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='VerticalTextAlignmentProperty']/Docs/*" />
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='FontFamilyProperty']/Docs/*" />
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='FontSizeProperty']/Docs/*" />
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='FontAttributesProperty']/Docs/*" />
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='FontAutoScalingEnabledProperty']/Docs/*" />
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='IsTextPredictionEnabledProperty']/Docs/*" />
		public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(Entry), true, BindingMode.OneTime);

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='CursorPositionProperty']/Docs/*" />
		public static readonly BindableProperty CursorPositionProperty = BindableProperty.Create(nameof(CursorPosition), typeof(int), typeof(Entry), 0, validateValue: (b, v) => (int)v >= 0);

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='SelectionLengthProperty']/Docs/*" />
		public static readonly BindableProperty SelectionLengthProperty = BindableProperty.Create(nameof(SelectionLength), typeof(int), typeof(Entry), 0, validateValue: (b, v) => (int)v >= 0);

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='ClearButtonVisibilityProperty']/Docs/*" />
		public static readonly BindableProperty ClearButtonVisibilityProperty = BindableProperty.Create(nameof(ClearButtonVisibility), typeof(ClearButtonVisibility), typeof(Entry), ClearButtonVisibility.Never);

		readonly Lazy<PlatformConfigurationRegistry<Entry>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Entry()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Entry>>(() => new PlatformConfigurationRegistry<Entry>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='HorizontalTextAlignment']/Docs/*" />
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='VerticalTextAlignment']/Docs/*" />
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='IsPassword']/Docs/*" />
		public bool IsPassword
		{
			get { return (bool)GetValue(IsPasswordProperty); }
			set { SetValue(IsPasswordProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='FontAutoScalingEnabled']/Docs/*" />
		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='IsTextPredictionEnabled']/Docs/*" />
		public bool IsTextPredictionEnabled
		{
			get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
			set { SetValue(IsTextPredictionEnabledProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='ReturnType']/Docs/*" />
		public ReturnType ReturnType
		{
			get => (ReturnType)GetValue(ReturnTypeProperty);
			set => SetValue(ReturnTypeProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='CursorPosition']/Docs/*" />
		public int CursorPosition
		{
			get { return (int)GetValue(CursorPositionProperty); }
			set { SetValue(CursorPositionProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='SelectionLength']/Docs/*" />
		public int SelectionLength
		{
			get { return (int)GetValue(SelectionLengthProperty); }
			set { SetValue(SelectionLengthProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='ReturnCommand']/Docs/*" />
		public ICommand ReturnCommand
		{
			get => (ICommand)GetValue(ReturnCommandProperty);
			set => SetValue(ReturnCommandProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='ReturnCommandParameter']/Docs/*" />
		public object ReturnCommandParameter
		{
			get => GetValue(ReturnCommandParameterProperty);
			set => SetValue(ReturnCommandParameterProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='ClearButtonVisibility']/Docs/*" />
		public ClearButtonVisibility ClearButtonVisibility
		{
			get => (ClearButtonVisibility)GetValue(ClearButtonVisibilityProperty);
			set => SetValue(ClearButtonVisibilityProperty, value);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		public event EventHandler Completed;

		/// <include file="../../docs/Microsoft.Maui.Controls/Entry.xml" path="//Member[@MemberName='SendCompleted']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCompleted()
		{
			if (IsEnabled)
			{
				Completed?.Invoke(this, EventArgs.Empty);

				if (ReturnCommand != null && ReturnCommand.CanExecute(ReturnCommandParameter))
				{
					ReturnCommand.Execute(ReturnCommandParameter);
				}
			}
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Entry> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}
	}
}
