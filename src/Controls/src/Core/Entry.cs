using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class Entry : InputView, IFontElement, ITextAlignmentElement, IEntryController, IElementConfiguration<Entry>
	{
		public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(Entry), ReturnType.Default);

		public static readonly BindableProperty ReturnCommandProperty = BindableProperty.Create(nameof(ReturnCommand), typeof(ICommand), typeof(Entry), default(ICommand));

		public static readonly BindableProperty ReturnCommandParameterProperty = BindableProperty.Create(nameof(ReturnCommandParameter), typeof(object), typeof(Entry), default(object));

		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		public new static readonly BindableProperty PlaceholderColorProperty = InputView.PlaceholderColorProperty;

		public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create("IsPassword", typeof(bool), typeof(Entry), default(bool));

		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(Entry), true, BindingMode.OneTime);

		public static readonly BindableProperty CursorPositionProperty = BindableProperty.Create(nameof(CursorPosition), typeof(int), typeof(Entry), 0, validateValue: (b, v) => (int)v >= 0);

		public static readonly BindableProperty SelectionLengthProperty = BindableProperty.Create(nameof(SelectionLength), typeof(int), typeof(Entry), 0, validateValue: (b, v) => (int)v >= 0);

		public static readonly BindableProperty ClearButtonVisibilityProperty = BindableProperty.Create(nameof(ClearButtonVisibility), typeof(ClearButtonVisibility), typeof(Entry), ClearButtonVisibility.Never);

		readonly Lazy<PlatformConfigurationRegistry<Entry>> _platformConfigurationRegistry;

		public Entry()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Entry>>(() => new PlatformConfigurationRegistry<Entry>(this));
		}

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		public bool IsPassword
		{
			get { return (bool)GetValue(IsPasswordProperty); }
			set { SetValue(IsPasswordProperty, value); }
		}

		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		[TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public bool IsTextPredictionEnabled
		{
			get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
			set { SetValue(IsTextPredictionEnabledProperty, value); }
		}

		public ReturnType ReturnType
		{
			get => (ReturnType)GetValue(ReturnTypeProperty);
			set => SetValue(ReturnTypeProperty, value);
		}

		public int CursorPosition
		{
			get { return (int)GetValue(CursorPositionProperty); }
			set { SetValue(CursorPositionProperty, value); }
		}

		public int SelectionLength
		{
			get { return (int)GetValue(SelectionLengthProperty); }
			set { SetValue(SelectionLengthProperty, value); }
		}

		public ICommand ReturnCommand
		{
			get => (ICommand)GetValue(ReturnCommandProperty);
			set => SetValue(ReturnCommandProperty, value);
		}

		public object ReturnCommandParameter
		{
			get => GetValue(ReturnCommandParameterProperty);
			set => SetValue(ReturnCommandParameterProperty, value);
		}

		public ClearButtonVisibility ClearButtonVisibility
		{
			get => (ClearButtonVisibility)GetValue(ClearButtonVisibilityProperty);
			set => SetValue(ClearButtonVisibilityProperty, value);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, (Entry)this);

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontChanged(Font oldValue, Font newValue) =>
			 HandleFontChanged();

		void HandleFontChanged()
		{
			// Null out the Maui font value so it will be recreated next time it's accessed
			_font = null;
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		public event EventHandler Completed;

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

		public IPlatformElementConfiguration<T, Entry> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}


		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}
	}
}