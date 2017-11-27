using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Controls;

[assembly: Dependency (typeof (StringProvider))]

namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle {
			get { return "Windows Core Gallery"; }
		}
	}
}
