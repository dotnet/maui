using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Platform;
using Bitmap = Microsoft.UI.Xaml.Media.ImageSource;

[assembly: Dependency(typeof(_13109IssueHelper))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
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
