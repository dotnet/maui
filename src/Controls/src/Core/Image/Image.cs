#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="Type[@FullName='Microsoft.Maui.Controls.Image']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Image()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Image>>(() => new PlatformConfigurationRegistry<Image>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='Aspect']/Docs/*" />
		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='IsLoading']/Docs/*" />
		public bool IsLoading
		{
			get => (bool)GetValue(IsLoadingProperty);
			private set => SetValue(IsLoadingPropertyKey, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='IsOpaque']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='Source']/Docs/*" />
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
	}
}