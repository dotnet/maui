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
	/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="Type[@FullName='Microsoft.Maui.Controls.ImageButton']/Docs/*" />
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


		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ImageButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ImageButton>>(() => new PlatformConfigurationRegistry<ImageButton>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='BorderColor']/Docs/*" />
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='CornerRadius']/Docs/*" />
		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='BorderWidth']/Docs/*" />
		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='Aspect']/Docs/*" />
		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='IsLoading']/Docs/*" />
		public bool IsLoading => (bool)GetValue(IsLoadingProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='IsPressed']/Docs/*" />
		public bool IsPressed => (bool)GetValue(IsPressedProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='IsOpaque']/Docs/*" />
		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}
		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='Source']/Docs/*" />
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

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			SizeRequest desiredSize = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
			return ImageElement.Measure(this, desiredSize, widthConstraint, heightConstraint);
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ImageButton> On<T>() where T : IConfigPlatform => _platformConfigurationRegistry.Value.On<T>();

		int IBorderElement.CornerRadiusDefaultValue => (int)CornerRadiusProperty.DefaultValue;

		Color IBorderElement.BorderColorDefaultValue => (Color)BorderColorProperty.DefaultValue;

		double IBorderElement.BorderWidthDefaultValue => (double)BorderWidthProperty.DefaultValue;

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='SetIsLoading']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsLoading(bool isLoading) => SetValue(IsLoadingPropertyKey, isLoading);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='SetIsPressed']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsPressed(bool isPressed) =>
			SetValue(IsPressedPropertyKey, isPressed);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='SendClicked']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() =>
			ButtonElement.ElementClicked(this, this);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='SendPressed']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() =>
			ButtonElement.ElementPressed(this, this);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='SendReleased']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() =>
			ButtonElement.ElementReleased(this, this);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='PropagateUpClicked']/Docs/*" />
		public void PropagateUpClicked() =>
			Clicked?.Invoke(this, EventArgs.Empty);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='PropagateUpPressed']/Docs/*" />
		public void PropagateUpPressed() =>
			Pressed?.Invoke(this, EventArgs.Empty);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='PropagateUpReleased']/Docs/*" />
		public void PropagateUpReleased() =>
			Released?.Invoke(this, EventArgs.Empty);

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='RaiseImageSourcePropertyChanged']/Docs/*" />
		public void RaiseImageSourcePropertyChanged() =>
			OnPropertyChanged(nameof(Source));

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="//Member[@MemberName='Padding']/Docs/*" />
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
	}
}
