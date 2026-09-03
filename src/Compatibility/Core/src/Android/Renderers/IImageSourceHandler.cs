using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IImageSourceHandler : IRegisterable
	{
		Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken));
	}
}