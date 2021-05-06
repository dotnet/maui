using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

[assembly: ExportEffect(typeof(FocusEffect), "FocusEffect")]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	public class FocusEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			try
			{
				(Control as Microsoft.UI.Xaml.Controls.Control).Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Cyan);
				(Control as FormsTextBox).BackgroundFocusBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.White);
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