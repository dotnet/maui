using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Windows.UI;
using global::Windows.UI.Xaml.Media;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Platform.UWP;

[assembly: ExportEffect(typeof(FocusEffect), "FocusEffect")]
namespace System.Maui.ControlGallery.WindowsUniversal
{
	public class FocusEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			try
			{
				(Control as global::Windows.UI.Xaml.Controls.Control).Background = new SolidColorBrush(Colors.Cyan);
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