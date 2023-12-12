using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 4734, "Gestures in Label Spans not working", PlatformAffected.All)]
	public partial class Issue4734 : ContentPage
	{
		public Issue4734()
		{
			InitializeComponent();
		}

		void OnSpanTapped(object sender, TappedEventArgs e)
		{
			TapResultLabel.Text = $"Span tapped!";
		}
	}
}