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
	/// Represents a button control that displays an image.
	/// </summary>
	public partial class ImageButton : View, IImageController, IElementConfiguration<ImageButton>, IBorderElement, IButtonController, IViewController, IPaddingElement, IButtonElement, ICommandElement, IImageElement, IImageButton
	{
		const int DefaultCornerRadius = -1;

		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = ButtonElement.CommandProperty;

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = ButtonElement.CommandParameterProperty;

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty = BorderElement.CornerRadiusProperty;

		/// <summary>Bindable property for <see cref="BorderWidth"/>.</summary>
		public static readonly BindableProperty BorderWidthProperty = BorderElement.BorderWidthProperty;

		/// <summary>Bindable property for <see cref="BorderColor"/>.</summary>
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		/// <summary>Bindable property for <see cref="Source"/>.</summary>
		public static readonly BindableProperty SourceProperty = ImageElement.SourceProperty;

		/// <summary>Bindable property for <see cref="Aspect"/>.</summary>
		public static readonly BindableProperty AspectProperty = ImageElement.AspectProperty;

		/// <summary>Bindable property for <see cref="IsOpaque"/>.</summary>
		public static readonly BindableProperty IsOpaqueProperty = ImageElement.IsOpaqueProperty;

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(ImageButton), default(bool));

		/// <summary>Bindable property for <see cref="IsLoading"/>.</summary>
		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		internal static readonly BindablePropertyKey IsPressedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(ImageButton), default(bool));

		/// <summary>Bindable property for <see cref="IsPressed"/>.</summary>
		public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		public event EventHandler Clicked;
		public event EventHandler Pressed;
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
		/// Gets or sets the border color of the button.
		/// </summary>
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the corner radius of the button.
		/// </summary>
		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		/// <summary>
		/// Gets or sets the border width of the button.
		/// </summary>
		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		/// <summary>
		/// Gets or sets the aspect ratio of the image.
		/// </summary>
		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		/// <summary>
		/// Gets a value indicating whether the image is currently loading.
		/// </summary>
		public bool IsLoading => (bool)GetValue(IsLoadingProperty);

		/// <summary>
		/// Gets a value indicating whether the button is currently pressed.
		/// </summary>
		public bool IsPressed => (bool)GetValue(IsPressedProperty);

		/// <summary>
		/// Gets or sets a value indicating whether the image is opaque.
		/// </summary>
		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}
		/// <summary>
		/// Gets or sets the command to invoke when the button is clicked.
		/// </summary>
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the command.
		/// </summary>
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>
		/// Gets or sets the image source for the button.
		/// </summary>
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
		/// Sets the loading state of the image.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsLoading(bool isLoading) => SetValue(IsLoadingPropertyKey, isLoading);

		/// <summary>
		/// Sets the pressed state of the button.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsPressed(bool isPressed) =>
			SetValue(IsPressedPropertyKey, isPressed);

		/// <summary>
		/// Sends the clicked event for the button.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() =>
			ButtonElement.ElementClicked(this, this);

		/// <summary>
		/// Sends the pressed event for the button.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() =>
			ButtonElement.ElementPressed(this, this);

		/// <summary>
		/// Sends the released event for the button.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() =>
			ButtonElement.ElementReleased(this, this);

		/// <summary>
		/// Propagates the clicked event up the visual tree.
		/// </summary>
		public void PropagateUpClicked() =>
			Clicked?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Propagates the pressed event up the visual tree.
		/// </summary>
		public void PropagateUpPressed() =>
			Pressed?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Propagates the released event up the visual tree.
		/// </summary>
		public void PropagateUpReleased() =>
			Released?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Raises the property changed event for the image source.
		/// </summary>
		public void RaiseImageSourcePropertyChanged() =>
			OnPropertyChanged(nameof(Source));

		/// <summary>
		/// Gets or sets the padding of the button.
		/// </summary>
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
			base.IsEnabledCore && CommandElement.GetCanExecute(this);

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
