namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21814, "Add better parameters for navigation args", PlatformAffected.All)]
	public class Issue21814 : NavigationPage
	{
		public Issue21814()
		{
			Navigation.PushAsync(new Issue21814FirstPage());
		}
	}

	public partial class Issue21814FirstPage : TestContentPage
	{
		public Issue21814FirstPage()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);

			var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
			OnNavigatedToLabel.Text = $"PreviousPage: {previousPage}, NavigationType: {args.NavigationType}";
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
		{
			base.OnNavigatingFrom(args);

			var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
			OnNavigatingFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
		}

		protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
		{
			base.OnNavigatedFrom(args);

			var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
			OnNavigatedFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
		}

		void OnNavigateButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Issue21814SecondPage());
		}
	}
}