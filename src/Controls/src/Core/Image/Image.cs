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
	public partial class Image : View, IImageController, IElementConfiguration<Image>, IViewController, IImageElement, IImage, IImageSourcePartEvents, IImageSourcePartFallbacks
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

		/// <summary>Bindable property for <see cref="PlaceholderSource"/>.</summary>
		public static readonly BindableProperty PlaceholderSourceProperty = BindableProperty.Create(nameof(PlaceholderSource), typeof(ImageSource), typeof(Image), default(ImageSource),
			propertyChanged: OnFallbackSourceChanged);

		/// <summary>Bindable property for <see cref="ErrorSource"/>.</summary>
		public static readonly BindableProperty ErrorSourceProperty = BindableProperty.Create(nameof(ErrorSource), typeof(ImageSource), typeof(Image), default(ImageSource),
			propertyChanged: OnFallbackSourceChanged);

		/// <summary>Bindable property for <see cref="KeepPreviousImageWhileLoading"/>.</summary>
		public static readonly BindableProperty KeepPreviousImageWhileLoadingProperty = BindableProperty.Create(nameof(KeepPreviousImageWhileLoading), typeof(bool), typeof(Image), false);

		static readonly BindablePropertyKey LoadingStatePropertyKey = BindableProperty.CreateReadOnly(nameof(LoadingState), typeof(ImageLoadingState), typeof(Image), ImageLoadingState.Idle);

		/// <summary>Bindable property for <see cref="LoadingState"/>.</summary>
		public static readonly BindableProperty LoadingStateProperty = LoadingStatePropertyKey.BindableProperty;

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

		/// <summary>Gets or sets the image shown while <see cref="Source"/> is loading. This is a bindable property.</summary>
		/// <value>The placeholder image, or <see langword="null"/> (the default) to show nothing while loading.</value>
		/// <remarks>
		/// The placeholder is only shown when <see cref="Source"/> is not available immediately, for example while it is downloaded.
		/// It does not change <see cref="IsLoading"/> or <see cref="LoadingState"/>. Changes take effect the next time <see cref="Source"/> loads.
		/// </remarks>
		[System.ComponentModel.TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource PlaceholderSource
		{
			get { return (ImageSource)GetValue(PlaceholderSourceProperty); }
			set { SetValue(PlaceholderSourceProperty, value); }
		}

		/// <summary>Gets or sets the image shown when <see cref="Source"/> fails to load. This is a bindable property.</summary>
		/// <value>The error image, or <see langword="null"/> (the default) to show nothing when loading fails.</value>
		/// <remarks>Changes take effect the next time <see cref="Source"/> fails to load.</remarks>
		[System.ComponentModel.TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource ErrorSource
		{
			get { return (ImageSource)GetValue(ErrorSourceProperty); }
			set { SetValue(ErrorSourceProperty, value); }
		}

		/// <summary>Gets or sets a value indicating whether the current image stays visible while a new <see cref="Source"/> loads. This is a bindable property.</summary>
		/// <value><see langword="true"/> to keep showing the current image until the new one is ready; <see langword="false"/> (the default) otherwise.</value>
		/// <remarks>
		/// <para>Use this, for example, to switch between images or to <see cref="Reload"/> without the image disappearing.
		/// While the current image is kept, <see cref="PlaceholderSource"/> is not shown. Only an image that loaded successfully is kept:
		/// when the new source fails to load, <see cref="ErrorSource"/> (or nothing) is shown instead.</para>
		/// <para>Do not enable this for images in a <see cref="CollectionView"/> or <see cref="ListView"/>: a reused cell would keep showing the previous item's image while the new one loads.</para>
		/// <para>When <see langword="false"/>, Android clears the image as soon as a new source starts loading, while iOS, Mac Catalyst and Windows keep it until the new image is ready.</para>
		/// </remarks>
		public bool KeepPreviousImageWhileLoading
		{
			get => (bool)GetValue(KeepPreviousImageWhileLoadingProperty);
			set => SetValue(KeepPreviousImageWhileLoadingProperty, value);
		}

		/// <summary>Gets the loading state of <see cref="Source"/>. This is a bindable property.</summary>
		/// <value>An <see cref="ImageLoadingState"/>. The default is <see cref="ImageLoadingState.Idle"/>.</value>
		/// <remarks>Use this property, for example in a trigger, to show a loading indicator or a retry button.</remarks>
		public ImageLoadingState LoadingState
		{
			get => (ImageLoadingState)GetValue(LoadingStateProperty);
			private set => SetValue(LoadingStatePropertyKey, value);
		}

		/// <summary>Occurs when <see cref="Source"/> has loaded and is displayed.</summary>
		public event EventHandler ImageLoaded;

		/// <summary>Occurs when <see cref="Source"/> fails to load.</summary>
		/// <remarks>Call <see cref="Reload"/> to try again.</remarks>
		public event EventHandler<ImageFailedEventArgs> ImageFailed;

		/// <summary>Loads <see cref="Source"/> again, for example after it failed to load.</summary>
		/// <remarks>Images that loaded successfully are reloaded from the cache when they are still valid.</remarks>
		public void Reload() =>
			Handler?.Invoke(Maui.Handlers.ImageHandler.ReloadCommand);

		protected override void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			// Clearing the source does not raise loading events.
			if (propertyName == SourceProperty.PropertyName && ImageSource.IsNullOrEmpty(Source))
				LoadingState = ImageLoadingState.Idle;
		}

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			// A load that was in progress when the handler was removed never completes.
			if (Handler is null && LoadingState == ImageLoadingState.Loading)
				LoadingState = ImageLoadingState.Idle;
		}

		static void OnFallbackSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (newValue is ImageSource source)
				SetInheritedBindingContext(source, bindable.BindingContext);
		}

		protected override void OnBindingContextChanged()
		{
			ImageElement.OnBindingContextChanged(this, this);
			if (PlaceholderSource is ImageSource placeholderSource)
				SetInheritedBindingContext(placeholderSource, BindingContext);
			if (ErrorSource is ImageSource errorSource)
				SetInheritedBindingContext(errorSource, BindingContext);
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

		IImageSource IImageSourcePartFallbacks.PlaceholderSource => PlaceholderSource;

		IImageSource IImageSourcePartFallbacks.ErrorSource => ErrorSource;

		bool IImageSourcePartFallbacks.KeepPreviousImageWhileLoading => KeepPreviousImageWhileLoading;

		void IImageSourcePartEvents.LoadingStarted() =>
			LoadingState = ImageLoadingState.Loading;

		void IImageSourcePartEvents.LoadingCompleted(bool successful)
		{
			// Unsuccessful completions are cancelled or replaced loads; the newer load reports its own state.
			if (!successful)
				return;

			LoadingState = ImageLoadingState.Loaded;
			ImageLoaded?.Invoke(this, EventArgs.Empty);
		}

		void IImageSourcePartEvents.LoadingFailed(Exception exception)
		{
			LoadingState = ImageLoadingState.Failed;
			ImageFailed?.Invoke(this, new ImageFailedEventArgs(exception));
		}

		void IImageSourcePart.UpdateIsLoading(bool isLoading) =>
			IsLoading = isLoading;

		private protected override string GetDebuggerDisplay()
		{
			var sourceText = DebuggerDisplayHelpers.GetDebugText(nameof(Source), Source);
			return $"{base.GetDebuggerDisplay()}, {sourceText}";
		}
	}
}