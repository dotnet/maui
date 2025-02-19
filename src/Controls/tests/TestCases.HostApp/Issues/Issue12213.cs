using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 12213, "[Windows] TapGestureRecognizer not working on Entry", PlatformAffected.UWP)]
	public class Issue12213 : TestContentPage
	{

		public Issue12213()
		{
		}

		protected override void Init()
		{
			var stackLayout = new StackLayout();

			var entry = new Entry();
			entry.Placeholder = "Enter Your Name";
			entry.AutomationId = "Entry";
			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
			tapGestureRecognizer.NumberOfTapsRequired = 1;
			entry.GestureRecognizers.Add(tapGestureRecognizer);
			stackLayout.Children.Add(entry);
			Content = stackLayout;
		}

		private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
		{
			DisplayAlert("Entry", "Tapped", "OK");
		}
	}
}
