using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 16978, "[Android]PanGestureRecognizer is not updated for Frame",
	PlatformAffected.Android)]
	public partial class Issue16978 : ContentPage
	{
		public Issue16978()
		{
			InitializeComponent();
		}

		private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
		{
			FrameLabel.Text = "Tap Gesture Recognized";
		}
	}
}