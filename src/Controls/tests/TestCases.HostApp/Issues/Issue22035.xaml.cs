namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22035, "[Android] CarouselView: VirtualView cannot be null here, when clearing and adding items on second navigation", PlatformAffected.Android)]
	public class Issue22035 : NavigationPage
	{
		public Issue22035() : base(new Issue22035Page())
		{

		}
	}

	public partial class Issue22035Page : ContentPage
	{
		Issue22035Page2 _Issue22035Page2 = new Issue22035Page2();
		public Issue22035Page()
		{
			InitializeComponent();
		}

		async void OnNavigateClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(_Issue22035Page2);
		}
	}
}