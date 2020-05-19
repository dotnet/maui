using System.Threading;
using System.Threading.Tasks;
using Android.Widget;

namespace System.Maui.Platform.Android
{
	/// <summary>
	/// The successor to IImageSourceHandler, the goal being we can achieve better performance by never creating an Android.Graphics.Bitmap instance
	/// </summary>
	public interface IImageViewHandler : IRegisterable
	{
		Task LoadImageAsync(ImageSource imagesource, ImageSource placeholder, ImageView imageView, CancellationToken cancellationToken = default(CancellationToken));
	}
}