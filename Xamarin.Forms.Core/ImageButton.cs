using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ImageButtonRenderer))]
	public class ImageButton : View, IImageController, IElementConfiguration<ImageButton>, IBorderElement, IButtonController, IBorderController, IViewController, IPaddingElement
	{
		const int DefaultCornerRadius = -1;

		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Button), null, propertyChanging: OnCommandChanging, propertyChanged: OnCommandChanged);

		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(Button), defaultValue: DefaultCornerRadius);

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ImageButton), null,
			propertyChanged: (bindable, oldvalue, newvalue) => ButtonElementManager.CommandCanExecuteChanged(bindable, EventArgs.Empty));

		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(Button), -1d);

		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(ImageButton), default(ImageSource),
			propertyChanging: OnImageSourceChanging, propertyChanged: OnImageSourceChanged);

		public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(ImageButton), Aspect.AspectFit);

		public static readonly BindableProperty IsOpaqueProperty = BindableProperty.Create(nameof(IsOpaque), typeof(bool), typeof(ImageButton), false);

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(ImageButton), default(bool));

		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		internal static readonly BindablePropertyKey IsPressedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(ImageButton), default(bool));

		public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;

		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;
		public event EventHandler Clicked;
		public event EventHandler Pressed;
		public event EventHandler Released;

		readonly Lazy<PlatformConfigurationRegistry<ImageButton>> _platformConfigurationRegistry;


		public ImageButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ImageButton>>(() => new PlatformConfigurationRegistry<ImageButton>(this));
		}

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		public bool IsLoading => (bool)GetValue(IsLoadingProperty);

		public bool IsPressed => (bool)GetValue(IsPressedProperty);

		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		bool IButtonController.IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			ImageElementManager.OnBindingContextChanged(this, this);
			base.OnBindingContextChanged();
		}

		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && IsPressed)
			{
				VisualStateManager.GoToState(this, ButtonElementManager.PressedVisualState);
			}
			else
			{
				base.ChangeVisualState();
			}
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			SizeRequest desiredSize = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
			return ImageElementManager.Measure(this, desiredSize, widthConstraint, heightConstraint);
		}

		public IPlatformElementConfiguration<T, ImageButton> On<T>() where T : IConfigPlatform => _platformConfigurationRegistry.Value.On<T>();

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsLoading(bool isLoading) => SetValue(IsLoadingPropertyKey, isLoading);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsPressed(bool isPressed) =>
			SetValue(IsPressedPropertyKey, isPressed);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() =>
			ButtonElementManager.ElementClicked(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() =>
			ButtonElementManager.ElementPressed(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() =>
			ButtonElementManager.ElementReleased(this, this);

		public void PropagateUpClicked() =>
			Clicked?.Invoke(this, EventArgs.Empty);

		public void PropagateUpPressed() =>
			Pressed?.Invoke(this, EventArgs.Empty);

		public void PropagateUpReleased() =>
			Released?.Invoke(this, EventArgs.Empty);

		public void RaiseImageSourcePropertyChanged() =>
			OnPropertyChanged(nameof(Source));

		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingElement.PaddingProperty); }
			set { SetValue(PaddingElement.PaddingProperty, value); }
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return default(Thickness);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}
		BindableProperty IBorderController.CornerRadiusProperty => ImageButton.CornerRadiusProperty;

		BindableProperty IBorderController.BorderColorProperty => ImageButton.BorderColorProperty;

		BindableProperty IBorderController.BorderWidthProperty => ImageButton.BorderWidthProperty;

		BindableProperty IImageController.SourceProperty => SourceProperty;

		BindableProperty IImageController.AspectProperty => AspectProperty;

		BindableProperty IImageController.IsOpaqueProperty => IsOpaqueProperty;

		void OnImageSourcesSourceChanged(object sender, EventArgs e) =>
			ImageElementManager.ImageSourcesSourceChanged(this, EventArgs.Empty);

		static void OnImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ImageSource newSource = (ImageSource)newValue;
			ImageButton button = (ImageButton)bindable;
			if (newSource != null)
			{
				newSource.SourceChanged += button.OnImageSourcesSourceChanged;
			}
			ImageElementManager.ImageSourceChanged(bindable, newSource);
		}

		static void OnImageSourceChanging(BindableObject bindable, object oldValue, object newValue)
		{
			ImageSource oldSource = (ImageSource)oldValue;
			ImageButton button = (ImageButton)bindable;

			if (oldSource != null)
			{
				oldSource.SourceChanged -= button.OnImageSourcesSourceChanged;
			}
			ImageElementManager.ImageSourceChanging(oldSource);
		}

		void OnCommandCanExecuteChanged(object sender, EventArgs e) =>
			ButtonElementManager.CommandCanExecuteChanged(this, EventArgs.Empty);

		static void OnCommandChanged(BindableObject bo, object o, object n)
		{
			var button = (ImageButton)bo;
			if (n is ICommand newCommand)
				newCommand.CanExecuteChanged += button.OnCommandCanExecuteChanged;

			ButtonElementManager.CommandChanged(button);
		}

		static void OnCommandChanging(BindableObject bo, object o, object n)
		{
			var button = (ImageButton)bo;
			if (o != null)
			{
				(o as ICommand).CanExecuteChanged -= button.OnCommandCanExecuteChanged;
			}
		}
	}
}