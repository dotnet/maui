using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class Editor : InputView, IEditorController, IFontElement, IElementConfiguration<Editor>
	{
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		public new static readonly BindableProperty PlaceholderColorProperty = InputView.PlaceholderColorProperty;

		public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(Editor), true, BindingMode.Default);

		public static readonly BindableProperty AutoSizeProperty = BindableProperty.Create(nameof(AutoSize), typeof(EditorAutoSizeOption), typeof(Editor), defaultValue: EditorAutoSizeOption.Disabled, propertyChanged: (bindable, oldValue, newValue)
			=> ((Editor)bindable)?.InvalidateMeasure());

		readonly Lazy<PlatformConfigurationRegistry<Editor>> _platformConfigurationRegistry;

		public EditorAutoSizeOption AutoSize
		{
			get { return (EditorAutoSizeOption)GetValue(AutoSizeProperty); }
			set { SetValue(AutoSizeProperty, value); }
		}

		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		public bool IsTextPredictionEnabled
		{
			get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
			set { SetValue(IsTextPredictionEnabledProperty, value); }
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

		protected void UpdateAutoSizeOption()
		{
			// Null out the Maui font value so it will be recreated next time it's accessed
			_font = null;

			if (AutoSize == EditorAutoSizeOption.TextChanges)
			{
				InvalidateMeasure();
			}
		}

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue)
		{
			UpdateAutoSizeOption();
		}

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue)
		{
			UpdateAutoSizeOption();
		}

		void IFontElement.OnFontChanged(Font oldValue, Font newValue)
		{
			UpdateAutoSizeOption();
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, (Editor)this);

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
		{
			UpdateAutoSizeOption();
		}

		public event EventHandler Completed;

		public Editor()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Editor>>(() => new PlatformConfigurationRegistry<Editor>(this));
		}

		public IPlatformElementConfiguration<T, Editor> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCompleted()
			=> Completed?.Invoke(this, EventArgs.Empty);

		protected override void OnTextChanged(string oldValue, string newValue)
		{
			base.OnTextChanged(oldValue, newValue);

			if (AutoSize == EditorAutoSizeOption.TextChanges)
			{
				InvalidateMeasure();
			}
		}
	}
}