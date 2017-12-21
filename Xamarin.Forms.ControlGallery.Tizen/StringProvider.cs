using Xamarin.Forms;
using Xamarin.Forms.Controls;
using Xamarin.Forms.ControlGallery.Tizen;

[assembly: Dependency(typeof(StringProvider))]
namespace Xamarin.Forms.ControlGallery.Tizen
{
	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle
		{
			get
			{
				return "Tizen CoreGallery";
			}
		}
	}
}