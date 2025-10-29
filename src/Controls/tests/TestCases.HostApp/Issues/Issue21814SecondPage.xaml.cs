namespace Maui.Controls.Sample.Issues
{
	public partial class Issue21814SecondPage : TestContentPage
	{
		public Issue21814SecondPage()
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

		void OnNavigateBackButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}