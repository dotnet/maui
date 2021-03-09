using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Bitmap = Microsoft.UI.Xaml.Media.ImageSource;

[assembly: Dependency(typeof(_13109IssueHelper))]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class _13109IssueHelper : IIssue13109Helper
	{
		public void SetImage(ImageSource imageSource)
		{
			var bitmap = imageSource.GetImage().ConfigureAwait(true);
		}
	}

	internal static class ImageHandler
	{
		internal static IImageSourceHandler GetHandler(this ImageSource source)
		{
			IImageSourceHandler returnValue = new FileImageSourceHandler();
			return returnValue;
		}

		internal static Task<Bitmap> GetImage(this ImageSource source)
		{
			return source.GetHandler().LoadImageAsync(source);
		}
	}
}