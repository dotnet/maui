using System.Collections;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(ImageSource))]
	class ImageBrush : Brush, IEnumerable
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

		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
			nameof(ImageSource), typeof(ImageSource), typeof(ImageBrush), default(ImageSource));

		public virtual ImageSource? ImageSource
		{
			get => (ImageSource?)GetValue(ImageSourceProperty);
			set => SetValue(ImageSourceProperty, value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			yield return ImageSource;
		}

		public override bool Equals(object? obj) =>
			obj is ImageBrush dest && ImageSource == dest.ImageSource;

		public override int GetHashCode() =>
			-1234567890 + (ImageSource?.GetHashCode() ?? 0);
	}
}