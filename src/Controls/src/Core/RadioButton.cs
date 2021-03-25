using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls
{
	public class RadioButton : TemplatedView, IElementConfiguration<RadioButton>, ITextElement, IFontElement, IBorderElement
	{
		public const string CheckedVisualState = "Checked";
		public const string UncheckedVisualState = "Unchecked";

		public const string TemplateRootName = "Root";
		public const string CheckedIndicator = "CheckedIndicator";
		public const string UncheckedButton = "Button";

		internal const string GroupNameChangedMessage = "RadioButtonGroupNameChanged";
		internal const string ValueChangedMessage = "RadioButtonValueChanged";

		// Template Parts
		TapGestureRecognizer _tapGestureRecognizer;
		View _templateRoot;

		static readonly Brush RadioButtonCheckMarkThemeColor = ResolveThemeColor("RadioButtonCheckMarkThemeColor");
		static readonly Brush RadioButtonThemeColor = ResolveThemeColor("RadioButtonThemeColor");
		static ControlTemplate s_defaultTemplate;

		readonly Lazy<PlatformConfigurationRegistry<RadioButton>> _platformConfigurationRegistry;

		static bool? s_rendererAvailable;

		public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(nameof(Content), typeof(object), typeof(RadioButton), null);

		public static readonly BindableProperty ValueProperty =
			BindableProperty.Create(nameof(Value), typeof(object), typeof(RadioButton), null,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnValuePropertyChanged());

		public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
			nameof(IsChecked), typeof(bool), typeof(RadioButton), false,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnIsCheckedPropertyChanged((bool)n),
			defaultBindingMode: BindingMode.TwoWay);

		public static readonly BindableProperty GroupNameProperty = BindableProperty.Create(
			nameof(GroupName), typeof(string), typeof(RadioButton), null,
			propertyChanged: (b, o, n) => ((RadioButton)b).OnGroupNamePropertyChanged((string)o, (string)n));

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		public static readonly BindableProperty CornerRadiusProperty = BorderElement.CornerRadiusProperty;

		public static readonly BindableProperty BorderWidthProperty = BorderElement.BorderWidthProperty;

		// If Content is set to a string, the string will be displayed using the native Text property
		// on platforms which support that; in a ControlTemplate it will be automatically converted
		// to a Label. If Content is set to a View, the View will be displayed on platforms which 
		// support Content natively or in the ContentPresenter of the ControlTemplate, if a ControlTemplate
		// is set. If a ControlTemplate is not set and the platform does not natively support arbitrary
		// Content, the ToString() representation of Content will be displayed.
		// For all types other than View and string, the ToString() representation of Content will be displayed.
		public object Content
		{
			get => GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}

		public object Value
		{
			get => GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		public string GroupName
		{
			get { return (string)GetValue(GroupNameProperty); }
			set { SetValue(GroupNameProperty, value); }
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		public double CharacterSpacing
		{
			get { return (double)GetValue(CharacterSpacingProperty); }
			set { SetValue(CharacterSpacingProperty, value); }
		}

		public TextTransform TextTransform
		{
			get { return (TextTransform)GetValue(TextTransformProperty); }
			set { SetValue(TextTransformProperty, value); }
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

		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderColorProperty); }
			set { SetValue(BorderColorProperty, value); }
		}

		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		public RadioButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<RadioButton>>(() =>
				new PlatformConfigurationRegistry<RadioButton>(this));
		}

		public IPlatformElementConfiguration<T, RadioButton> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

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
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontChanged(Font oldValue, Font newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, this);

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

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (UsingRenderer)
			{
				return Device.PlatformServices.GetNativeSize(this, widthConstraint, heightConstraint);
			}

			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		public override ControlTemplate ResolveControlTemplate()
		{
			var template = base.ResolveControlTemplate();

			if (template == null)
			{
				if (!RendererAvailable)
				{
					ControlTemplate = DefaultTemplate;
				}
			}

			return ControlTemplate;
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

		bool UsingRenderer => ControlTemplate == null;

		void UpdateIsEnabled()
		{
			if (UsingRenderer)
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

		static bool RendererAvailable
		{
			get
			{
				if (!s_rendererAvailable.HasValue)
				{
					s_rendererAvailable = Internals.Registrar.Registered.GetHandlerType(typeof(RadioButton)) != null;
				}

				return s_rendererAvailable.Value;
			}
		}

		static Brush ResolveThemeColor(string key)
		{
			if (Application.Current.TryGetResource(key, out object color))
			{
				return (Brush)color;
			}

			if (Application.Current?.RequestedTheme == OSAppTheme.Dark)
			{
				return Brush.White;
			}

			return Brush.Black;
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
				IsChecked = true;
			}
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

			MessagingCenter.Send(this, ValueChangedMessage,
						new RadioButtonValueChanged(RadioButtonGroup.GetVisualRoot(this)));
		}

		void OnGroupNamePropertyChanged(string oldGroupName, string newGroupName)
		{
			if (!string.IsNullOrEmpty(newGroupName))
			{
				if (string.IsNullOrEmpty(oldGroupName))
				{
					MessagingCenter.Subscribe<RadioButton, RadioButtonGroupSelectionChanged>(this,
						RadioButtonGroup.GroupSelectionChangedMessage, HandleRadioButtonGroupSelectionChanged);
					MessagingCenter.Subscribe<Layout<View>, RadioButtonGroupValueChanged>(this,
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
					MessagingCenter.Unsubscribe<Layout<View>, RadioButtonGroupValueChanged>(this, RadioButtonGroup.GroupValueChangedMessage);
				}
			}
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

			IsChecked = false;
		}

		void HandleRadioButtonGroupValueChanged(Layout<View> layout, RadioButtonGroupValueChanged args)
		{
			if (IsChecked || string.IsNullOrEmpty(GroupName) || GroupName != args.GroupName || Value != args.Value || !MatchesScope(args))
			{
				return;
			}

			IsChecked = true;
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
			var frame = new Frame
			{
				HasShadow = false,
				Padding = 6
			};

			BindToTemplatedParent(frame, BackgroundColorProperty, Microsoft.Maui.Controls.Frame.BorderColorProperty, HorizontalOptionsProperty,
				MarginProperty, OpacityProperty, RotationProperty, ScaleProperty, ScaleXProperty, ScaleYProperty,
				TranslationYProperty, TranslationXProperty, VerticalOptionsProperty);

			var grid = new Grid
			{
				RowSpacing = 0,
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
				Stroke = RadioButtonThemeColor,
				InputTransparent = true
			};

			var checkMark = new Ellipse
			{
				Fill = RadioButtonCheckMarkThemeColor,
				Aspect = Stretch.Uniform,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = 11,
				WidthRequest = 11,
				Opacity = 0,
				InputTransparent = true
			};

			var contentPresenter = new ContentPresenter
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			contentPresenter.SetBinding(MarginProperty, new Binding("Padding", source: RelativeBindingSource.TemplatedParent));
			contentPresenter.SetBinding(BackgroundColorProperty, new Binding(BackgroundColorProperty.PropertyName,
				source: RelativeBindingSource.TemplatedParent));

			grid.Children.Add(normalEllipse);
			grid.Children.Add(checkMark);
			grid.Children.Add(contentPresenter, 1, 0);

			frame.Content = grid;

			INameScope nameScope = new NameScope();
			NameScope.SetNameScope(frame, nameScope);
			nameScope.RegisterName(TemplateRootName, frame);
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
			checkedVisualState.Setters.Add(new Setter() { Property = Shape.StrokeProperty, TargetName = UncheckedButton, Value = RadioButtonCheckMarkThemeColor });
			checkedStates.States.Add(checkedVisualState);

			VisualState uncheckedVisualState = new VisualState() { Name = UncheckedVisualState };
			uncheckedVisualState.Setters.Add(new Setter() { Property = OpacityProperty, TargetName = CheckedIndicator, Value = 0 });
			uncheckedVisualState.Setters.Add(new Setter() { Property = Shape.StrokeProperty, TargetName = UncheckedButton, Value = RadioButtonThemeColor });
			checkedStates.States.Add(uncheckedVisualState);

			visualStateGroups.Add(checkedStates);

			VisualStateManager.SetVisualStateGroups(frame, visualStateGroups);

			return frame;
		}

		public string ContentAsString()
		{
			var content = Content;
			if (content is View)
			{
				Log.Warning("RadioButton", $"Warning - {Device.RuntimePlatform} does not support {nameof(View)} as the {ContentProperty.PropertyName} property of {nameof(RadioButton)}; the return value of the ToString() method will be displayed instead.");
			}

			return content?.ToString();
		}
	}
}