using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportEffect(typeof(FocusEffect), "FocusEffect")]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class FocusEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			try
			{
				(Control as Windows.UI.Xaml.Controls.Control).Background = new SolidColorBrush(Colors.Cyan);
				(Control as FormsTextBox).BackgroundFocusBrush = new SolidColorBrush(Colors.White);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
			}
		}

		protected override void OnDetached()
		{
		}
	}
}