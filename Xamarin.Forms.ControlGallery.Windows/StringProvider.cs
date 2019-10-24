using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WinRT;
using Xamarin.Forms.Controls;

[assembly: Dependency (typeof (StringProvider))]

namespace Xamarin.Forms.ControlGallery.WinRT
{
	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle {
			get { return "Windows Core Gallery"; }
		}
	}
}
