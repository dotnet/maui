#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics;


namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A view control that displays an image.
	/// </summary>
	/// <remarks>
	/// The Image control supports various image sources including files, URIs, streams, and embedded resources.
	/// Use the <see cref="Source"/> property to specify the image, and the <see cref="Aspect"/> property to control how the image is scaled.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class Image : View, IImageController, IElementConfiguration<Image>, IViewController, IImageElement, IImage
	{
		/// <summary>Bindable property for <see cref="Source"/>.</summary>
		public static readonly BindableProperty SourceProperty = ImageElement.SourceProperty;

		/// <summary>Bindable property for <see cref="Aspect"/>.</summary>
		public static readonly BindableProperty AspectProperty = ImageElement.AspectProperty;

		/// <summary>Bindable property for <see cref="IsOpaque"/>.</summary>
		public static readonly BindableProperty IsOpaqueProperty = ImageElement.IsOpaqueProperty;

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(Image), default(bool));

		/// <summary>Bindable property for <see cref="IsLoading"/>.</summary>
		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="IsAnimationPlaying"/>.</summary>
		public static readonly BindableProperty IsAnimationPlayingProperty = ImageElement.IsAnimationPlayingProperty;

		readonly Lazy<PlatformConfigurationRegistry<Image>> _platformConfigurationRegistry;

		/// <summary>Initializes a new instance of the Image class.</summary>
		public Image()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Image>>(() => new PlatformConfigurationRegistry<Image>(this));
		}

		/// <summary>Gets or sets the scaling mode for the image. This is a bindable property.</summary>
		/// <value>An <see cref="Microsoft.Maui.Aspect"/> value that determines how the image is scaled. The default is <see cref="Microsoft.Maui.Aspect.AspectFit"/>.</value>
		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		/// <summary>Gets the loading status of the image. This is a bindable property.</summary>
		/// <value>A <see cref="bool"/> indicating whether the image is currently loading. The default is <see langword="false"/>.</value>
		/// <remarks>This property can be used to show loading indicators while images are being loaded from remote sources.</remarks>
		public bool IsLoading
		{
			get => (bool)GetValue(IsLoadingProperty);
			private set => SetValue(IsLoadingPropertyKey, value);
		}

		/// <summary>Gets or sets a Boolean value that hints to the rendering engine that it may safely omit drawing visual elements behind the image. This is a bindable property.</summary>
		/// <value>A <see cref="bool"/> that indicates whether the image is opaque. The default is <see langword="false"/>.</value>
		/// <remarks>Setting this property does not change the visual opacity of the image. Instead, it provides a hint to the rendering engine that may improve performance by skipping the rendering of elements behind the image.</remarks>
		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}

		/// <summary>Gets or sets a value that indicates whether animated images should play. This is a bindable property.</summary>
		/// <value>A <see cref="bool"/> that indicates whether animated images (such as GIFs) should play. The default is <see langword="true"/>.</value>
		public bool IsAnimationPlaying
		{
			get { return (bool)GetValue(IsAnimationPlayingProperty); }
			set { SetValue(IsAnimationPlayingProperty, value); }
		}

		/// <summary>Gets or sets the source of the image. This is a bindable property.</summary>
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

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			SizeRequest desiredSize = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
			return ImageElement.Measure(this, desiredSize, widthConstraint, heightConstraint);
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Image> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		bool IImageController.GetLoadAsAnimation() =>
			ImageElement.GetLoadAsAnimation(this);

		void IImageController.SetIsLoading(bool isLoading) =>
			IsLoading = isLoading;

		void IImageElement.OnImageSourceSourceChanged(object sender, EventArgs e) =>
			ImageElement.ImageSourceSourceChanged(this, e);

		void IImageElement.RaiseImageSourcePropertyChanged() =>
			OnPropertyChanged(nameof(Source));

		IImageSource IImageSourcePart.Source => Source;

		void IImageSourcePart.UpdateIsLoading(bool isLoading) =>
			IsLoading = isLoading;

		private protected override string GetDebuggerDisplay()
		{
			var sourceText = DebuggerDisplayHelpers.GetDebugText(nameof(Source), Source);
			return $"{base.GetDebuggerDisplay()}, {sourceText}";
		}
	}
}