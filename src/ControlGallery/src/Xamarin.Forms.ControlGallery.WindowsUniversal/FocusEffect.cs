using System;
using System.Diagnostics;
using Windows.UI;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Platform.UWP;
using WSolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

[assembly: ExportEffect(typeof(FocusEffect), "FocusEffect")]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class FocusEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			try
			{
				(Control as Windows.UI.Xaml.Controls.Control).Background = new WSolidColorBrush(Colors.Cyan);
				(Control as FormsTextBox).BackgroundFocusBrush = new WSolidColorBrush(Colors.White);
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