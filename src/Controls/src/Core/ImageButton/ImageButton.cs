#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
/// <summary>
/// Represents a button that displays an image and reacts to touch events.
/// </summary>
/// <remarks>
/// <see cref="ImageButton"/> is similar to <see cref="Button"/> but displays an image instead of text.
/// It supports all standard button features including commands, events, borders, and visual states.
/// </remarks>
[ElementHandler<ImageButtonHandler>]
	public partial class ImageButton : View, IImageController, IElementConfiguration<ImageButton>, IBorderElement, IButtonController, IViewController, IPaddingElement, IButtonElement, ICommandElement, IImageElement, IImageButton
	{
		const int DefaultCornerRadius = -1;

		/// <summary>Bindable property for <see cref="Command"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CommandProperty = ButtonElement.CommandProperty;

		/// <summary>Bindable property for <see cref="CommandParameter"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CommandParameterProperty = ButtonElement.CommandParameterProperty;

		/// <summary>Bindable property for <see cref="CornerRadius"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CornerRadiusProperty = BorderElement.CornerRadiusProperty;

		/// <summary>Bindable property for <see cref="BorderWidth"/>. This is a bindable property.</summary>
		public static readonly BindableProperty BorderWidthProperty = BorderElement.BorderWidthProperty;

		/// <summary>Bindable property for <see cref="BorderColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		/// <summary>Bindable property for <see cref="Source"/>. This is a bindable property.</summary>
		public static readonly BindableProperty SourceProperty = ImageElement.SourceProperty;

		/// <summary>Bindable property for <see cref="Aspect"/>. This is a bindable property.</summary>
		public static readonly BindableProperty AspectProperty = ImageElement.AspectProperty;

		/// <summary>Bindable property for <see cref="IsOpaque"/>. This is a bindable property.</summary>
		public static readonly BindableProperty IsOpaqueProperty = ImageElement.IsOpaqueProperty;

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(ImageButton), default(bool));

		/// <summary>Bindable property for <see cref="IsLoading"/>. This is a bindable property.</summary>
		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		internal static readonly BindablePropertyKey IsPressedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(ImageButton), default(bool));

		/// <summary>Bindable property for <see cref="IsPressed"/>. This is a bindable property.</summary>
		public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="Padding"/>. This is a bindable property.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>
		/// Occurs when the <see cref="ImageButton"/> is clicked or tapped.
		/// </summary>
		public event EventHandler Clicked;

		/// <summary>
		/// Occurs when the <see cref="ImageButton"/> is pressed.
		/// </summary>
		public event EventHandler Pressed;

		/// <summary>
		/// Occurs when the <see cref="ImageButton"/> is released.
		/// </summary>
		public event EventHandler Released;

		readonly Lazy<PlatformConfigurationRegistry<ImageButton>> _platformConfigurationRegistry;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageButton"/> class.
		/// </summary>
		public ImageButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ImageButton>>(() => new PlatformConfigurationRegistry<ImageButton>(this));
		}

		/// <summary>
		/// Gets or sets the color of the border around the image button.
		/// This is a bindable property.
		/// </summary>
		/// <value>The color of the border. The default is <see langword="null"/>.</value>
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the corner radius for the image button border, in device-independent units.
		/// This is a bindable property.
		/// </summary>
		/// <value>The corner radius of the border. The default is -1, which indicates the platform default.</value>
		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		/// <summary>
		/// Gets or sets the width of the border around the image button, in device-independent units.
		/// This is a bindable property.
		/// </summary>
		/// <value>The width of the border. The default is 0.</value>
		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		/// <summary>
		/// Gets or sets the scaling mode for the image.
		/// This is a bindable property.
		/// </summary>
		/// <value>An <see cref="Microsoft.Maui.Aspect"/> value that determines how the image is scaled. The default is <see cref="Microsoft.Maui.Aspect.AspectFit"/>.</value>
		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		/// <summary>
		/// Gets a value indicating whether the image is currently being loaded.
		/// This is a bindable property.
		/// </summary>
		/// <value><see langword="true"/> if the image is loading; otherwise, <see langword="false"/>.</value>
		public bool IsLoading => (bool)GetValue(IsLoadingProperty);

		/// <summary>
		/// Gets a value indicating whether the image button is currently pressed.
		/// This is a bindable property.
		/// </summary>
		/// <value><see langword="true"/> if the button is pressed; otherwise, <see langword="false"/>.</value>
		public bool IsPressed => (bool)GetValue(IsPressedProperty);

		/// <summary>
		/// Gets or sets a value indicating whether the image should be rendered as opaque.
		/// This is a bindable property.
		/// </summary>
		/// <value><see langword="true"/> if the image should be opaque; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to invoke when the image button is clicked.
		/// This is a bindable property.
		/// </summary>
		/// <value>An <see cref="ICommand"/> to execute when the button is clicked. The default is <see langword="null"/>.</value>
		/// <remarks>
		/// This property is typically used in MVVM patterns to bind the button to a command in the view model.
		/// The button's <see cref="VisualElement.IsEnabled"/> property is controlled by <see cref="System.Windows.Input.ICommand.CanExecute(object)"/>.
		/// </remarks>
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="Command"/> when it is executed.
		/// This is a bindable property.
		/// </summary>
		/// <value>The parameter object. The default is <see langword="null"/>.</value>
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>
		/// Gets or sets the source of the image to display on the button.
		/// This is a bindable property.
		/// </summary>
		/// <value>An <see cref="ImageSource"/> that represents the image. The default is <see langword="null"/>.</value>
		[System.ComponentModel.TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			ImageElement.OnBindingContextChanged(this, this);
			base.OnBindingContextChanged();
		}

		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && IsPressed)
			{
				VisualStateManager.GoToState(this, ButtonElement.PressedVisualState);
			}
			else
			{
				base.ChangeVisualState();
			}
		}

#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable CS0618 // Type or member is obsolete
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			SizeRequest desiredSize = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
#pragma warning restore CS0618 // Type or member is obsolete
			return ImageElement.Measure(this, desiredSize, widthConstraint, heightConstraint);
		}
#pragma warning restore CS0672 // Member overrides obsolete member

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ImageButton> On<T>() where T : IConfigPlatform => _platformConfigurationRegistry.Value.On<T>();

		int IBorderElement.CornerRadiusDefaultValue => (int)CornerRadiusProperty.DefaultValue;

		Color IBorderElement.BorderColorDefaultValue => (Color)BorderColorProperty.DefaultValue;

		double IBorderElement.BorderWidthDefaultValue => (double)BorderWidthProperty.DefaultValue;

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform. Sets the <see cref="IsLoading"/> property.
		/// </summary>
		/// <param name="isLoading">The loading state to set.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsLoading(bool isLoading) => SetValue(IsLoadingPropertyKey, isLoading);

		/// <summary>
		/// For internal use by the .NET MAUI platform. Sets the <see cref="IsPressed"/> property.
		/// </summary>
		/// <param name="isPressed">The pressed state to set.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsPressed(bool isPressed) =>
			SetValue(IsPressedPropertyKey, isPressed);

		/// <summary>
		/// For internal use by the .NET MAUI platform. Triggers the <see cref="Clicked"/> event.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() =>
			ButtonElement.ElementClicked(this, this);

		/// <summary>
		/// For internal use by the .NET MAUI platform. Triggers the <see cref="Pressed"/> event.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() =>
			ButtonElement.ElementPressed(this, this);

		/// <summary>
		/// For internal use by the .NET MAUI platform. Triggers the <see cref="Released"/> event.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() =>
			ButtonElement.ElementReleased(this, this);

		/// <summary>
		/// For internal use by the .NET MAUI platform. Propagates the clicked event up the visual tree.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void PropagateUpClicked() =>
			Clicked?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// For internal use by the .NET MAUI platform. Propagates the pressed event up the visual tree.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void PropagateUpPressed() =>
			Pressed?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// For internal use by the .NET MAUI platform. Propagates the released event up the visual tree.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void PropagateUpReleased() =>
			Released?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// For internal use by the .NET MAUI platform. Raises the property changed event for the image source.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void RaiseImageSourcePropertyChanged() =>
			OnPropertyChanged(nameof(Source));

		/// <summary>
		/// Gets or sets the padding inside the image button.
		/// This is a bindable property.
		/// </summary>
		/// <value>The padding around the image. The default is a <see cref="Thickness"/> with all values set to 0.</value>
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

		void IImageElement.OnImageSourceSourceChanged(object sender, EventArgs e) =>
			ImageElement.ImageSourceSourceChanged(this, e);

		bool IImageElement.IsAnimationPlaying
		{
			get => false;
		}

		bool IBorderElement.IsCornerRadiusSet() => IsSet(CornerRadiusProperty);
		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);
		bool IBorderElement.IsBackgroundSet() => IsSet(BackgroundProperty);
		bool IBorderElement.IsBorderColorSet() => IsSet(BorderColorProperty);
		bool IBorderElement.IsBorderWidthSet() => IsSet(BorderWidthProperty);

		bool IImageController.GetLoadAsAnimation() => false;

		protected override bool IsEnabledCore =>
			base.IsEnabledCore && CommandElement.GetCanExecute(this, CommandProperty);

		void ICommandElement.CanExecuteChanged(object sender, EventArgs e) =>
			RefreshIsEnabledProperty();

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == BorderWidthProperty.PropertyName)
				Handler?.UpdateValue(nameof(IImageButton.StrokeThickness));
			else if (propertyName == BorderColorProperty.PropertyName)
				Handler?.UpdateValue(nameof(IImageButton.StrokeColor));
		}

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
			((IImageController)this)?.SetIsLoading(isLoading);
		}

		bool IImageSourcePart.IsAnimationPlaying => false;

		IImageSource IImageSourcePart.Source => Source;

		void IButton.Clicked()
		{
			(this as IButtonController).SendClicked();
		}

		void IButton.Pressed()
		{
			(this as IButtonController).SendPressed();
		}

		void IButton.Released()
		{
			(this as IButtonController).SendReleased();
		}

		double IButtonStroke.StrokeThickness => (double)GetValue(BorderWidthProperty);

		Color IButtonStroke.StrokeColor => (Color)GetValue(BorderColorProperty);

		int IButtonStroke.CornerRadius => (int)GetValue(CornerRadiusProperty);


		WeakCommandSubscription ICommandElement.CleanupTracker
		{
			get;
			set;
		}

	}
}
