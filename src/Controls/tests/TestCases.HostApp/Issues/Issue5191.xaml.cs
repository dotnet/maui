using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 5191, "PanGesture notify Completed event moving outside View limits", PlatformAffected.All)]
	public partial class Issue5191 : ContentPage
	{
		public Issue5191()
		{
			InitializeComponent();
		}

		void OnPanGestureRecognizerUpdated(object sender, PanUpdatedEventArgs e)
		{
			InfoLabel.Text = $"StatusType: {e.StatusType}, TotalX: {e.TotalX}, TotalY: {e.TotalY}";
		}
	}
}