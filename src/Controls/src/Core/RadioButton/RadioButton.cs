#nullable disable
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;

using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a button that can be selected from a group of radio buttons, where only one button can be selected at a time.
	/// </summary>
	/// <remarks>
	/// <see cref="RadioButton"/> controls are typically used in groups where users need to select one option from multiple choices.
	/// Radio buttons in the same group are mutually exclusive - selecting one will automatically deselect the others.
	/// Use the <see cref="GroupName"/> property or <see cref="RadioButtonGroup"/> to group radio buttons together.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<RadioButtonHandler>]
	public partial class RadioButton : TemplatedView, IElementConfiguration<RadioButton>, ITextElement, IFontElement, IBorderElement, IRadioButton
	{
		/// <summary>
		/// The visual state name for when the radio button is checked.
		/// </summary>
		/// <value>The string "Checked".</value>
		public const string CheckedVisualState = "Checked";
		
		/// <summary>
		/// The visual state name for when the radio button is unchecked.
		/// </summary>
		/// <value>The string "Unchecked".</value>
		public const string UncheckedVisualState = "Unchecked";

		/// <summary>
		/// The name of the template root element in the control template.
		/// </summary>
		/// <value>The string "Root".</value>
		public const string TemplateRootName = "Root";
		
		/// <summary>
		/// The name of the checked indicator element in the control template.
		/// </summary>
		/// <value>The string "CheckedIndicator".</value>
		public const string CheckedIndicator = "CheckedIndicator";
		
		/// <summary>
		/// The name of the unchecked button element in the control template.
		/// </summary>
		/// <value>The string "Button".</value>
		public const string UncheckedButton = "Button";

		// App Theme string constants for Light/Dark modes
		internal const string RadioButtonOuterEllipseStrokeLight = "RadioButtonOuterEllipseStrokeLight";
		internal const string RadioButtonOuterEllipseStrokeDark = "RadioButtonOuterEllipseStrokeDark";
		internal const string RadioButtonCheckGlyphStrokeLight = "RadioButtonCheckGlyphStrokeLight";
		internal const string RadioButtonCheckGlyphStrokeDark = "RadioButtonCheckGlyphStrokeDark";
		internal const string RadioButtonCheckGlyphFillLight = "RadioButtonCheckGlyphFillLight";
		internal const string RadioButtonCheckGlyphFillDark = "RadioButtonCheckGlyphFillDark";

		// Older App Theme constants
		internal const string RadioButtonThemeColor = "RadioButtonThemeColor";
		internal const string RadioButtonCheckMarkThemeColor = "RadioButtonCheckMarkThemeColor";


		// Template Parts
		TapGestureRecognizer _tapGestureRecognizer;
		View _templateRoot;

		static ControlTemplate s_defaultTemplate;

		readonly Lazy<PlatformConfigurationRegistry<RadioButton>> _platformConfigurationRegistry;

		/// <summary>
		/// Occurs when the <see cref="IsChecked"/> property changes.
		/// </summary>
		public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

		/// <summary>Bindable property for <see cref="Content"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(nameof(Content), typeof(object), typeof(RadioButton), null);

		/// <summary>Bindable property for <see cref="Value"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ValueProperty =
			BindableProperty.Create(nameof(Value), typeof(object), typeof(RadioButton), null,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnValuePropertyChanged());

		/// <summary>Bindable property for <see cref="IsChecked"/>. This is a bindable property.</summary>
		public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
			nameof(IsChecked), typeof(bool), typeof(RadioButton), false,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnIsCheckedPropertyChanged((bool)n),
			defaultBindingMode: BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="GroupName"/>. This is a bindable property.</summary>
		public static readonly BindableProperty GroupNameProperty = BindableProperty.Create(
			nameof(GroupName), typeof(string), typeof(RadioButton), null,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnGroupNamePropertyChanged((string)o, (string)n));

		/// <summary>Bindable property for <see cref="TextColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Bindable property for <see cref="TextTransform"/>. This is a bindable property.</summary>
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <summary>Bindable property for <see cref="FontAttributes"/>. This is a bindable property.</summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>Bindable property for <see cref="FontFamily"/>. This is a bindable property.</summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>Bindable property for <see cref="FontSize"/>. This is a bindable property.</summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>. This is a bindable property.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>Bindable property for <see cref="BorderColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		/// <summary>Bindable property for <see cref="CornerRadius"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CornerRadiusProperty = BorderElement.CornerRadiusProperty;

		/// <summary>Bindable property for <see cref="BorderWidth"/>. This is a bindable property.</summary>
		public static readonly BindableProperty BorderWidthProperty = BorderElement.BorderWidthProperty;

		// If Content is set to a string, the string will be displayed using the native Text property
		// on platforms which support that; in a ControlTemplate it will be automatically converted
		// to a Label. If Content is set to a View, the View will be displayed on platforms which 
		// support Content natively or in the ContentPresenter of the ControlTemplate, if a ControlTemplate
		// is set. If a ControlTemplate is not set and the platform does not natively support arbitrary
		// Content, the ToString() representation of Content will be displayed.
		// For all types other than View and string, the ToString() representation of Content will be displayed.
		/// <summary>
		/// Gets or sets the content to display within the radio button.
		/// This is a bindable property.
		/// </summary>
		/// <value>The content object. Can be a string, <see cref="View"/>, or any object. For non-View types, the <c>ToString()</c> representation is displayed.</value>
		public object Content
		{
			get => GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}

		/// <summary>
		/// Gets or sets the value associated with this radio button.
		/// This is a bindable property.
		/// </summary>
		/// <value>The value object. This is typically used to identify which option was selected in a group of radio buttons.</value>
		public object Value
		{
			get => GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the radio button is checked.
		/// This is a bindable property.
		/// </summary>
		/// <value><see langword="true"/> if the radio button is checked; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		/// <summary>
		/// Gets or sets the name that identifies which radio buttons are mutually exclusive.
		/// This is a bindable property.
		/// </summary>
		/// <value>The group name. Radio buttons with the same group name are mutually exclusive. The default is <see langword="null"/>.</value>
		public string GroupName
		{
			get { return (string)GetValue(GroupNameProperty); }
			set { SetValue(GroupNameProperty, value); }
		}

		/// <summary>
		/// Gets or sets the color of the text displayed in the radio button.
		/// This is a bindable property.
		/// </summary>
		/// <value>The text <see cref="Color"/>. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the spacing between characters in the text.
		/// This is a bindable property.
		/// </summary>
		/// <value>The character spacing value. The default is 0.0.</value>
		public double CharacterSpacing
		{
			get { return (double)GetValue(CharacterSpacingProperty); }
			set { SetValue(CharacterSpacingProperty, value); }
		}

		/// <summary>
		/// Gets or sets the text transformation to apply to the text.
		/// This is a bindable property.
		/// </summary>
		/// <value>A <see cref="Microsoft.Maui.TextTransform"/> value. The default is <see cref="TextTransform.None"/>.</value>
		public TextTransform TextTransform
		{
			get { return (TextTransform)GetValue(TextTransformProperty); }
			set { SetValue(TextTransformProperty, value); }
		}

		/// <summary>
		/// Gets or sets the font attributes (bold, italic) for the text.
		/// This is a bindable property.
		/// </summary>
		/// <value>A <see cref="Microsoft.Maui.Controls.FontAttributes"/> value. The default is <see cref="FontAttributes.None"/>.</value>
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <summary>
		/// Gets or sets the font family for the text.
		/// This is a bindable property.
		/// </summary>
		/// <value>The font family name. The default is <see langword="null"/>, which uses the platform default font.</value>
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the size of the font.
		/// This is a bindable property.
		/// </summary>
		/// <value>The font size. The default is the platform default font size.</value>
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the font size should scale automatically based on user accessibility settings.
		/// This is a bindable property.
		/// </summary>
		/// <value><see langword="true"/> if font auto-scaling is enabled; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		/// <summary>
		/// Gets or sets the width of the border around the radio button.
		/// This is a bindable property.
		/// </summary>
		/// <value>The border width in device-independent units. The default is 0.</value>
		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		/// <summary>
		/// Gets or sets the color of the border around the radio button.
		/// This is a bindable property.
		/// </summary>
		/// <value>The border <see cref="Color"/>. The default is <see langword="null"/>.</value>
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderColorProperty); }
			set { SetValue(BorderColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the corner radius of the radio button border.
		/// This is a bindable property.
		/// </summary>
		/// <value>The corner radius in device-independent units. The default is -1, which indicates the platform default.</value>
		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RadioButton"/> class.
		/// </summary>
		public RadioButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<RadioButton>>(() =>
				new PlatformConfigurationRegistry<RadioButton>(this));
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, RadioButton> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		/// <summary>
		/// Gets the default control template for the <see cref="RadioButton"/>.
		/// </summary>
		/// <value>The default <see cref="ControlTemplate"/> that defines the visual structure of a radio button.</value>
		public static ControlTemplate DefaultTemplate
		{
			get
			{
				if (s_defaultTemplate == null)
				{
					s_defaultTemplate = new ControlTemplate(() => BuildDefaultTemplate());
				}

				return s_defaultTemplate;
			}
		}

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
			=> InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
			=> InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		/// <summary>
		/// Applies the specified text transformation to the source text.
		/// </summary>
		/// <param name="source">The source text to transform.</param>
		/// <param name="textTransform">The text transformation to apply.</param>
		/// <returns>The transformed text.</returns>
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilities.GetTransformedText(source, textTransform);

		int IBorderElement.CornerRadiusDefaultValue => (int)BorderElement.CornerRadiusProperty.DefaultValue;

		Color IBorderElement.BorderColorDefaultValue => (Color)BorderElement.BorderColorProperty.DefaultValue;

		double IBorderElement.BorderWidthDefaultValue => (double)BorderElement.BorderWidthProperty.DefaultValue;

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		bool IBorderElement.IsCornerRadiusSet() => IsSet(BorderElement.CornerRadiusProperty);
		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);
		bool IBorderElement.IsBackgroundSet() => IsSet(BackgroundProperty);
		bool IBorderElement.IsBorderColorSet() => IsSet(BorderElement.BorderColorProperty);
		bool IBorderElement.IsBorderWidthSet() => IsSet(BorderElement.BorderWidthProperty);

		protected internal override void ChangeVisualState()
		{
			ApplyIsCheckedState();

			base.ChangeVisualState();
		}

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (ControlTemplate == null)
			{
				return Handler?.GetDesiredSize(widthConstraint, heightConstraint) ?? new();
			}

			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_templateRoot = (this as IControlTemplated)?.TemplateRoot as View;

			ApplyIsCheckedState();
			UpdateIsEnabled();
		}

		internal override void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
		{
			base.OnControlTemplateChanged(oldValue, newValue);
		}

		void UpdateIsEnabled()
		{
			if (ControlTemplate == null)
			{
				return;
			}

			if (_tapGestureRecognizer == null)
			{
				_tapGestureRecognizer = new TapGestureRecognizer();
			}

			if (IsEnabled)
			{
				_tapGestureRecognizer.Tapped += SelectRadioButton;
				GestureRecognizers.Add(_tapGestureRecognizer);
			}
			else
			{
				_tapGestureRecognizer.Tapped -= SelectRadioButton;
				GestureRecognizers.Remove(_tapGestureRecognizer);
			}
		}

		void ApplyIsCheckedState()
		{
			if (IsChecked)
			{
				VisualStateManager.GoToState(this, CheckedVisualState);
				if (_templateRoot != null)
				{
					VisualStateManager.GoToState(_templateRoot, CheckedVisualState);
				}
			}
			else
			{
				VisualStateManager.GoToState(this, UncheckedVisualState);
				if (_templateRoot != null)
				{
					VisualStateManager.GoToState(_templateRoot, UncheckedVisualState);
				}
			}
		}

		void SelectRadioButton(object sender, EventArgs e)
		{
			if (IsEnabled)
			{
				SetValue(IsCheckedProperty, true, specificity: SetterSpecificity.FromHandler);
			}
		}

		void OnIsCheckedPropertyChanged(bool isChecked)
		{
			if (isChecked)
			{
				RadioButtonGroup.UpdateRadioButtonGroup(this);
			}

			ChangeVisualState();
			CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(isChecked));
		}

		void OnValuePropertyChanged()
		{
			if (!IsChecked || string.IsNullOrEmpty(GroupName))
			{
				return;
			}

			var controller = RadioButtonGroupController.GetGroupController(this);
			controller?.HandleRadioButtonValueChanged(this);
		}

		void OnGroupNamePropertyChanged(string oldGroupName, string newGroupName)
		{
			if (!string.IsNullOrEmpty(oldGroupName) && !string.IsNullOrEmpty(newGroupName) && newGroupName != oldGroupName)
			{
				var controller = RadioButtonGroupController.GetGroupController(this);
				controller?.HandleRadioButtonGroupNameChanged(oldGroupName);
			}
		}

		internal void OnGroupSelectionChanged(RadioButton radioButton)
		{
			var controller = RadioButtonGroupController.GetGroupController(this);
			controller?.HandleRadioButtonGroupSelectionChanged(radioButton);
		}

		static View BuildDefaultTemplate()
		{
			Border border = new Border()
			{
				Padding = 6
			};

			border.SetBinding(HorizontalOptionsProperty, static (RadioButton rb) => rb.HorizontalOptions, source: RelativeBindingSource.TemplatedParent);
			border.SetBinding(VerticalOptionsProperty, static (RadioButton rb) => rb.VerticalOptions, source: RelativeBindingSource.TemplatedParent);

			border.SetBinding(Border.StrokeProperty, static (RadioButton rb) => rb.BorderColor, source: RelativeBindingSource.TemplatedParent);
			border.SetBinding(Border.StrokeShapeProperty, static (RadioButton rb) => rb.CornerRadius, source: RelativeBindingSource.TemplatedParent, converter: new CornerRadiusToShape());
			border.SetBinding(Border.StrokeThicknessProperty, static (RadioButton rb) => rb.BorderWidth, source: RelativeBindingSource.TemplatedParent);

			var grid = new Grid
			{
				Padding = 2,
				RowSpacing = 0,
				ColumnSpacing = 6,
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star }
				},
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			var normalEllipse = new Ellipse
			{
				Fill = Brush.Transparent,
				Aspect = Stretch.Uniform,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = 21,
				WidthRequest = 21,
				StrokeThickness = 2,
				InputTransparent = false
			};

			var checkMark = new Ellipse
			{
				Aspect = Stretch.Uniform,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = 11,
				WidthRequest = 11,
				Opacity = 0,
				InputTransparent = false
			};

			var contentPresenter = new ContentPresenter
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			object dynamicOuterEllipseThemeColor = null;
			object dynamicCheckMarkThemeColor = null;
			object outerEllipseVisualStateLight = null;
			object outerEllipseVisualStateDark = null;
			object checkMarkVisualStateLight = null;
			object checkMarkVisualStateDark = null;
			object checkMarkFillVisualStateLight = null;
			object checkMarkFillVisualStateDark = null;

			if (!normalEllipse.TrySetDynamicThemeColor(
				RadioButtonThemeColor,
				Ellipse.StrokeProperty,
				out dynamicOuterEllipseThemeColor))
			{
				normalEllipse.TrySetAppTheme(
					RadioButtonOuterEllipseStrokeLight,
					RadioButtonOuterEllipseStrokeDark,
					Ellipse.StrokeProperty,
					SolidColorBrush.White,
					SolidColorBrush.Black,
					out outerEllipseVisualStateLight,
					out outerEllipseVisualStateDark);
			}

			if (!checkMark.TrySetDynamicThemeColor(
				RadioButtonCheckMarkThemeColor,
				Ellipse.StrokeProperty,
				out dynamicCheckMarkThemeColor))
			{
				checkMark.TrySetAppTheme(
					RadioButtonCheckGlyphStrokeLight,
					RadioButtonCheckGlyphStrokeDark,
					Ellipse.StrokeProperty,
					SolidColorBrush.White,
					SolidColorBrush.Black,
					out checkMarkVisualStateLight,
					out checkMarkVisualStateDark);
			}

			if (!checkMark.TrySetDynamicThemeColor(
				RadioButtonCheckMarkThemeColor,
				Ellipse.FillProperty,
				out dynamicCheckMarkThemeColor))
			{
				checkMark.TrySetAppTheme(
					RadioButtonCheckGlyphFillLight,
					RadioButtonCheckGlyphFillDark,
					Ellipse.FillProperty,
					SolidColorBrush.White,
					SolidColorBrush.Black,
					out checkMarkFillVisualStateLight,
					out checkMarkFillVisualStateDark);
			}

			contentPresenter.SetBinding(MarginProperty, static (RadioButton radio) => radio.Padding, BindingMode.OneWay, source: RelativeBindingSource.TemplatedParent);
			contentPresenter.SetBinding(BackgroundColorProperty, static (RadioButton radio) => radio.BackgroundColor, BindingMode.OneWay, source: RelativeBindingSource.TemplatedParent);

			grid.Add(normalEllipse);
			grid.Add(checkMark);
			grid.Add(contentPresenter, 1, 0);

			border.Content = grid;

			INameScope nameScope = new NameScope();
			NameScope.SetNameScope(border, nameScope);
			nameScope.RegisterName(TemplateRootName, border);
			nameScope.RegisterName(UncheckedButton, normalEllipse);
			nameScope.RegisterName(CheckedIndicator, checkMark);
			nameScope.RegisterName("ContentPresenter", contentPresenter);

			VisualStateGroupList visualStateGroups = new VisualStateGroupList();

			var common = new VisualStateGroup() { Name = "Common" };
			common.States.Add(new VisualState() { Name = VisualStateManager.CommonStates.Normal });
			common.States.Add(new VisualState() { Name = VisualStateManager.CommonStates.Disabled });

			visualStateGroups.Add(common);

			var checkedStates = new VisualStateGroup() { Name = "CheckedStates" };

			VisualState checkedVisualState = new VisualState() { Name = CheckedVisualState };
			checkedVisualState.Setters.Add(new Setter() { Property = OpacityProperty, TargetName = CheckedIndicator, Value = 1 });
			checkedVisualState.Setters.Add(
				new Setter()
				{
					Property = Shape.StrokeProperty,
					TargetName = UncheckedButton,
					Value = dynamicOuterEllipseThemeColor is not null ? dynamicOuterEllipseThemeColor : new AppThemeBinding() { Light = outerEllipseVisualStateLight, Dark = outerEllipseVisualStateDark }
				});
			checkedVisualState.Setters.Add(
				new Setter()
				{
					Property = Shape.StrokeProperty,
					TargetName = CheckedIndicator,
					Value = dynamicCheckMarkThemeColor is not null ? dynamicCheckMarkThemeColor : new AppThemeBinding() { Light = checkMarkVisualStateLight, Dark = checkMarkVisualStateDark }
				});
			checkedVisualState.Setters.Add(
				new Setter()
				{
					Property = Shape.FillProperty,
					TargetName = CheckedIndicator,
					Value = dynamicCheckMarkThemeColor is not null ? dynamicCheckMarkThemeColor : new AppThemeBinding() { Light = checkMarkFillVisualStateLight, Dark = checkMarkFillVisualStateDark }
				});
			checkedStates.States.Add(checkedVisualState);

			VisualState uncheckedVisualState = new VisualState() { Name = UncheckedVisualState };
			uncheckedVisualState.Setters.Add(new Setter() { Property = OpacityProperty, TargetName = CheckedIndicator, Value = 0 });

			uncheckedVisualState.Setters.Add(
				new Setter()
				{
					Property = Shape.StrokeProperty,
					TargetName = UncheckedButton,
					Value = dynamicOuterEllipseThemeColor is not null ? dynamicOuterEllipseThemeColor : new AppThemeBinding() { Light = outerEllipseVisualStateLight, Dark = outerEllipseVisualStateDark }
				});

			checkedStates.States.Add(uncheckedVisualState);

			visualStateGroups.Add(checkedStates);

			VisualStateManager.SetVisualStateGroups(border, visualStateGroups);

			return border;
		}

		/// <summary>
		/// Converts the <see cref="Content"/> to a string representation.
		/// </summary>
		/// <returns>The string representation of the content, or the result of <c>ToString()</c> if content is not a string.</returns>
		/// <remarks>
		/// If <see cref="Content"/> is a <see cref="View"/>, a warning is logged and the <c>ToString()</c> representation is used instead.
		/// </remarks>
		public string ContentAsString()
		{
			var content = Content;
			if (content is View)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<RadioButton>()?.LogWarning("Warning - {RuntimePlatform} does not support View as the {PropertyName} property of RadioButton; the return value of the ToString() method will be displayed instead.", DeviceInfo.Platform, ContentProperty.PropertyName);
			}

			return content?.ToString();
		}

		Font ITextStyle.Font => this.ToFont();

#if ANDROID
		object IContentView.Content 
		{
			get
			{
				if (ResolveControlTemplate() == null)
					return ContentAsString();

				return Content;
			}
		}
#endif

		IView IContentView.PresentedContent => ((this as IControlTemplated).TemplateRoot as IView) ?? (Content as IView);

		double IButtonStroke.StrokeThickness => (double)GetValue(BorderWidthProperty);

		Color IButtonStroke.StrokeColor => (Color)GetValue(BorderColorProperty);

		int IButtonStroke.CornerRadius => (int)GetValue(CornerRadiusProperty);

		bool IRadioButton.IsChecked
		{
			get => IsChecked;
			set => SetValue(IsCheckedProperty, value, SetterSpecificity.FromHandler);
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(Value), Value, nameof(IsChecked), IsChecked);
			return $"{base.GetDebuggerDisplay()},{debugText}";
		}

		private protected override Semantics UpdateSemantics()
		{
			var semantics = base.UpdateSemantics();

			if (ControlTemplate != null)
			{
				string contentAsString = ContentAsString();

				if (!string.IsNullOrWhiteSpace(contentAsString) && string.IsNullOrWhiteSpace(semantics?.Description))
				{
					semantics ??= new Semantics();
					semantics.Description = contentAsString;
				}
			}

			return semantics;
		}

		class CornerRadiusToShape : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				return new RoundRectangle
				{
					CornerRadius = (int)value,
				};
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}

		internal override bool TrySetValue(string text)
		{
			if (bool.TryParse(text, out bool rbResult))
			{
				IsChecked = rbResult;
				return true;
			}

			return false;
		}
	}
}
