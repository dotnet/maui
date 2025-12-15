#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	/// <summary><see cref="Microsoft.Maui.Controls.View"/> that holds an image.</summary>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler(typeof(ImageHandler))]
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
		/// <remarks>The following example creates a new image from a file</remarks>
		public Image()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Image>>(() => new PlatformConfigurationRegistry<Image>(this));
		}

		/// <summary>Gets or sets the scaling mode for the image. This is a bindable property.</summary>
		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		/// <summary>Gets the loading status of the image. This is a bindable property.</summary>
		/// <remarks>The following example illustrates running a</remarks>
		public bool IsLoading
		{
			get => (bool)GetValue(IsLoadingProperty);
			private set => SetValue(IsLoadingPropertyKey, value);
		}

		/// <summary>Gets or sets a Boolean value that, if <see langword="true"/> hints to the rendering engine that it may safely omit drawing visual elements behind the image.</summary>
		/// <remarks>When this property is
		/// Setting this property does not change the opacity of the image. Instead, it indicates whether the rendering engine may treat the image as opaque while rendering.</remarks>
		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='IsAnimationPlaying']/Docs/*" />
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