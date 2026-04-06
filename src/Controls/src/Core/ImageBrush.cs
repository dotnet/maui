namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(ImageSource))]
	class ImageBrush : Brush
	{
		public ImageBrush()
		{
		}

		public ImageBrush(ImageSource imageSource)
		{
			ImageSource = imageSource;
		}

		public override bool IsEmpty =>
			ImageSource?.IsEmpty ?? true;

		/// <summary>Bindable property for <see cref="ImageSource"/>.</summary>
		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
			nameof(ImageSource), typeof(ImageSource), typeof(ImageBrush), default(ImageSource));

		public virtual ImageSource? ImageSource
		{
			get => (ImageSource?)GetValue(ImageSourceProperty);
			set => SetValue(ImageSourceProperty, value);
		}

		public override bool Equals(object? obj) =>
			obj is ImageBrush dest && ImageSource == dest.ImageSource;

		public override int GetHashCode() => base.GetHashCode();
	}
}