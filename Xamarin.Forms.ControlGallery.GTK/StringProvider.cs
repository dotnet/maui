using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.GTK;

[assembly: Dependency(typeof(StringProvider))]
namespace Xamarin.Forms.ControlGallery.GTK
{
    public class StringProvider : Controls.IStringProvider
    {
        public string CoreGalleryTitle
        {
            get
            {
                return "GTK# CoreGallery";
            }
        }
    }
}