#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="Type[@FullName='Microsoft.Maui.Controls.Editor']/Docs/*" />
	public partial class Editor : InputView, IEditorController, IFontElement, ITextAlignmentElement, IElementConfiguration<Editor>, IEditor
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='TextProperty']/Docs/*" />
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		/// <summary>Bindable property for <see cref="FontFamily"/>.</summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>Bindable property for <see cref="FontSize"/>.</summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>Bindable property for <see cref="FontAttributes"/>.</summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='PlaceholderProperty']/Docs/*" />
		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='PlaceholderColorProperty']/Docs/*" />
		public new static readonly BindableProperty PlaceholderColorProperty = InputView.PlaceholderColorProperty;

		/// <summary>Bindable property for <see cref="IsTextPredictionEnabled"/>.</summary>
		public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(Editor), true, BindingMode.Default);

		/// <summary>Bindable property for <see cref="CursorPosition"/>.</summary>
		public static readonly BindableProperty CursorPositionProperty = BindableProperty.Create(nameof(CursorPosition), typeof(int), typeof(Editor), 0, validateValue: (b, v) => (int)v >= 0);

		/// <summary>Bindable property for <see cref="SelectionLength"/>.</summary>
		public static readonly BindableProperty SelectionLengthProperty = BindableProperty.Create(nameof(SelectionLength), typeof(int), typeof(Editor), 0, validateValue: (b, v) => (int)v >= 0);

		/// <summary>Bindable property for <see cref="AutoSize"/>.</summary>
		public static readonly BindableProperty AutoSizeProperty = BindableProperty.Create(nameof(AutoSize), typeof(EditorAutoSizeOption), typeof(Editor), defaultValue: EditorAutoSizeOption.Disabled, propertyChanged: (bindable, oldValue, newValue)
			=> ((Editor)bindable)?.UpdateAutoSizeOption());

		/// <summary>Bindable property for <see cref="HorizontalTextAlignment"/>.</summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <summary>Bindable property for <see cref="VerticalTextAlignment"/>.</summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(nameof(VerticalTextAlignment), typeof(TextAlignment), typeof(Editor), TextAlignment.Start);

		readonly Lazy<PlatformConfigurationRegistry<Editor>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='AutoSize']/Docs/*" />
		public EditorAutoSizeOption AutoSize
		{
			get { return (EditorAutoSizeOption)GetValue(AutoSizeProperty); }
			set { SetValue(AutoSizeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='IsTextPredictionEnabled']/Docs/*" />
		public bool IsTextPredictionEnabled
		{
			get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
			set { SetValue(IsTextPredictionEnabledProperty, value); }
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(HorizontalTextAlignmentProperty); }
			set { SetValue(HorizontalTextAlignmentProperty, value); }
		}

		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(VerticalTextAlignmentProperty); }
			set { SetValue(VerticalTextAlignmentProperty, value); }
		}

		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
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
			UpdateAutoSizeOption();
		}

		void UpdateAutoSizeOption()
		{
			if (AutoSize == EditorAutoSizeOption.TextChanges && this.IsShimmed())
				InvalidateMeasure();
		}

		public event EventHandler Completed;
		double _previousWidthConstraint;
		double _previousHeightConstraint;
		Rect _previousBounds;

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Editor()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Editor>>(() => new PlatformConfigurationRegistry<Editor>(this));
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Editor> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Editor.xml" path="//Member[@MemberName='SendCompleted']/Docs/*" />
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

		public void OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}

		Font ITextStyle.Font => this.ToFont();

		void IEditor.Completed()
		{
			(this as IEditorController).SendCompleted();
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			_previousBounds = bounds;
			return base.ArrangeOverride(bounds);
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (AutoSize == EditorAutoSizeOption.Disabled &&
				Width > 0 &&
				Height > 0)
			{
				if (TheSame(_previousHeightConstraint, heightConstraint) &&
					TheSame(_previousWidthConstraint, widthConstraint))
				{
					return new Size(Width, Height);
				}
				else if (TheSame(_previousHeightConstraint, _previousBounds.Height) &&
					TheSame(_previousWidthConstraint, _previousBounds.Width))
				{
					return new Size(Width, Height);
				}
			}

			_previousWidthConstraint = widthConstraint;
			_previousHeightConstraint = heightConstraint;
			return base.MeasureOverride(widthConstraint, heightConstraint);

			bool TheSame(double width, double otherWidth)
			{
				return width == otherWidth ||
					(width - otherWidth) < double.Epsilon;
			}
		}
	}
}
