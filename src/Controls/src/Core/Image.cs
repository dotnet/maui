using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="Type[@FullName='Microsoft.Maui.Controls.Image']/Docs/*" />
	public partial class Image : View, IImageController, IElementConfiguration<Image>, IViewController, IImageElement
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='SourceProperty']/Docs/*" />
		public static readonly BindableProperty SourceProperty = ImageElement.SourceProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='AspectProperty']/Docs/*" />
		public static readonly BindableProperty AspectProperty = ImageElement.AspectProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='IsOpaqueProperty']/Docs/*" />
		public static readonly BindableProperty IsOpaqueProperty = ImageElement.IsOpaqueProperty;

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(Image), default(bool));

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='IsLoadingProperty']/Docs/*" />
		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Image.xml" path="//Member[@MemberName='IsAnimationPlayingProperty']/Docs/*" />
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
	}
}