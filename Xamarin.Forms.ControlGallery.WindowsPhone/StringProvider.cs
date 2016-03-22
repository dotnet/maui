using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Windows;
using Xamarin.Forms.Controls;

[assembly: Dependency (typeof (StringProvider))]

namespace Xamarin.Forms.ControlGallery.Windows
{
	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle {
			get { return "Windows Core Gallery"; }
		}
	}
}
