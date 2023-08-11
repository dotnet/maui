#nullable disable
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="Type[@FullName='Microsoft.Maui.Controls.RadioButton']/Docs/*" />
	public partial class RadioButton : TemplatedView, IElementConfiguration<RadioButton>, ITextElement, IFontElement, IBorderElement, IRadioButton
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='CheckedVisualState']/Docs/*" />
		public const string CheckedVisualState = "Checked";
		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='UncheckedVisualState']/Docs/*" />
		public const string UncheckedVisualState = "Unchecked";

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='TemplateRootName']/Docs/*" />
		public const string TemplateRootName = "Root";
		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='CheckedIndicator']/Docs/*" />
		public const string CheckedIndicator = "CheckedIndicator";
		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='UncheckedButton']/Docs/*" />
		public const string UncheckedButton = "Button";

		internal const string GroupNameChangedMessage = "RadioButtonGroupNameChanged";
		internal const string ValueChangedMessage = "RadioButtonValueChanged";

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

		public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(nameof(Content), typeof(object), typeof(RadioButton), null);

		/// <summary>Bindable property for <see cref="Value"/>.</summary>
		public static readonly BindableProperty ValueProperty =
			BindableProperty.Create(nameof(Value), typeof(object), typeof(RadioButton), null,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnValuePropertyChanged());

		/// <summary>Bindable property for <see cref="IsChecked"/>.</summary>
		public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
			nameof(IsChecked), typeof(bool), typeof(RadioButton), false,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnIsCheckedPropertyChanged((bool)n),
			defaultBindingMode: BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="GroupName"/>.</summary>
		public static readonly BindableProperty GroupNameProperty = BindableProperty.Create(
			nameof(GroupName), typeof(string), typeof(RadioButton), null,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnGroupNamePropertyChanged((string)o, (string)n));

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Bindable property for <see cref="TextTransform"/>.</summary>
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <summary>Bindable property for <see cref="FontAttributes"/>.</summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>Bindable property for <see cref="FontFamily"/>.</summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>Bindable property for <see cref="FontSize"/>.</summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>Bindable property for <see cref="BorderColor"/>.</summary>
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty = BorderElement.CornerRadiusProperty;

		/// <summary>Bindable property for <see cref="BorderWidth"/>.</summary>
		public static readonly BindableProperty BorderWidthProperty = BorderElement.BorderWidthProperty;

		// If Content is set to a string, the string will be displayed using the native Text property
		// on platforms which support that; in a ControlTemplate it will be automatically converted
		// to a Label. If Content is set to a View, the View will be displayed on platforms which 
		// support Content natively or in the ContentPresenter of the ControlTemplate, if a ControlTemplate
		// is set. If a ControlTemplate is not set and the platform does not natively support arbitrary
		// Content, the ToString() representation of Content will be displayed.
		// For all types other than View and string, the ToString() representation of Content will be displayed.
		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public object Content
		{
			get => GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='Value']/Docs/*" />
		public object Value
		{
			get => GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='IsChecked']/Docs/*" />
		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='GroupName']/Docs/*" />
		public string GroupName
		{
			get { return (string)GetValue(GroupNameProperty); }
			set { SetValue(GroupNameProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(CharacterSpacingProperty); }
			set { SetValue(CharacterSpacingProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='TextTransform']/Docs/*" />
		public TextTransform TextTransform
		{
			get { return (TextTransform)GetValue(TextTransformProperty); }
			set { SetValue(TextTransformProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='BorderWidth']/Docs/*" />
		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='BorderColor']/Docs/*" />
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderColorProperty); }
			set { SetValue(BorderColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='CornerRadius']/Docs/*" />
		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='DefaultTemplate']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='UpdateFormsText']/Docs/*" />
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilites.GetTransformedText(source, textTransform);

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

		IPlatformSizeService _platformSizeService;

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (ControlTemplate == null)
			{
				if (Handler != null)
					return new SizeRequest(Handler.GetDesiredSize(widthConstraint, heightConstraint));

				_platformSizeService ??= DependencyService.Get<IPlatformSizeService>();
				return _platformSizeService.GetPlatformSize(this, widthConstraint, heightConstraint);
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
				SetValue(IsCheckedProperty, true, specificity: SetterSpecificity.FromHandler);
		}

		void OnIsCheckedPropertyChanged(bool isChecked)
		{
			if (isChecked)
				RadioButtonGroup.UpdateRadioButtonGroup(this);

			ChangeVisualState();
			CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(isChecked));
		}

		void OnValuePropertyChanged()
		{
			if (!IsChecked || string.IsNullOrEmpty(GroupName))
			{
				return;
			}
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			MessagingCenter.Send(this, ValueChangedMessage,
						new RadioButtonValueChanged(RadioButtonGroup.GetVisualRoot(this)));
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void OnGroupNamePropertyChanged(string oldGroupName, string newGroupName)
		{
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			if (!string.IsNullOrEmpty(newGroupName))
			{
				if (string.IsNullOrEmpty(oldGroupName))
				{
					MessagingCenter.Subscribe<RadioButton, RadioButtonGroupSelectionChanged>(this,
						RadioButtonGroup.GroupSelectionChangedMessage, HandleRadioButtonGroupSelectionChanged);
					MessagingCenter.Subscribe<Element, RadioButtonGroupValueChanged>(this,
						RadioButtonGroup.GroupValueChangedMessage, HandleRadioButtonGroupValueChanged);
				}

				MessagingCenter.Send(this, GroupNameChangedMessage,
					new RadioButtonGroupNameChanged(RadioButtonGroup.GetVisualRoot(this), oldGroupName));
			}
			else
			{
				if (!string.IsNullOrEmpty(oldGroupName))
				{
					MessagingCenter.Unsubscribe<RadioButton, RadioButtonGroupSelectionChanged>(this, RadioButtonGroup.GroupSelectionChangedMessage);
					MessagingCenter.Unsubscribe<Element, RadioButtonGroupValueChanged>(this, RadioButtonGroup.GroupValueChangedMessage);
				}
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}

		bool MatchesScope(RadioButtonScopeMessage message)
		{
			return RadioButtonGroup.GetVisualRoot(this) == message.Scope;
		}

		void HandleRadioButtonGroupSelectionChanged(RadioButton selected, RadioButtonGroupSelectionChanged args)
		{
			if (!IsChecked || selected == this || string.IsNullOrEmpty(GroupName) || GroupName != selected.GroupName || !MatchesScope(args))
			{
				return;
			}

			SetValue(IsCheckedProperty, false, specificity: SetterSpecificity.FromHandler);
		}

		void HandleRadioButtonGroupValueChanged(Element layout, RadioButtonGroupValueChanged args)
		{
			if (IsChecked || string.IsNullOrEmpty(GroupName) || GroupName != args.GroupName || !object.Equals(Value, args.Value) || !MatchesScope(args))
			{
				return;
			}

			SetValue(IsCheckedProperty, true, specificity: SetterSpecificity.FromHandler);
		}

		static void BindToTemplatedParent(BindableObject bindableObject, params BindableProperty[] properties)
		{
			foreach (var property in properties)
			{
				bindableObject.SetBinding(property, new Binding(property.PropertyName,
					source: RelativeBindingSource.TemplatedParent));
			}
		}

		static View BuildDefaultTemplate()
		{
			Border border = new Border()
			{
				Padding = 6
			};

			BindToTemplatedParent(border, BackgroundColorProperty, HorizontalOptionsProperty,
				MarginProperty, OpacityProperty, RotationProperty, ScaleProperty, ScaleXProperty, ScaleYProperty,
				TranslationYProperty, TranslationXProperty, VerticalOptionsProperty);

			border.SetBinding(Border.StrokeProperty,
				new Binding(BorderColorProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			border.SetBinding(Border.StrokeShapeProperty,
				new Binding(CornerRadiusProperty.PropertyName, converter: new CornerRadiusToShape(),
							source: RelativeBindingSource.TemplatedParent));

			border.SetBinding(Border.StrokeThicknessProperty,
				new Binding(BorderWidthProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

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

			contentPresenter.SetBinding(MarginProperty, new Binding("Padding", source: RelativeBindingSource.TemplatedParent));
			contentPresenter.SetBinding(BackgroundColorProperty, new Binding(BackgroundColorProperty.PropertyName,
				source: RelativeBindingSource.TemplatedParent));

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

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='ContentAsString']/Docs/*" />
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
		object IContentView.Content => ContentAsString();
#endif

		IView IContentView.PresentedContent => ((this as IControlTemplated).TemplateRoot as IView) ?? (Content as IView);

		double IButtonStroke.StrokeThickness => (double)GetValue(BorderWidthProperty);

		Color IButtonStroke.StrokeColor => (Color)GetValue(BorderColorProperty);

		int IButtonStroke.CornerRadius => (int)GetValue(CornerRadiusProperty);

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
	}
}
