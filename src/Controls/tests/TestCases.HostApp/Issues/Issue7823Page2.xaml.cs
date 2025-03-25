namespace Maui.Controls.Sample.Issues
{
	public partial class Issue7823Page2 : ContentPage
	{
		public Issue7823Page2()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);
			grid.Add(new Label()
			{
				AutomationId = "SecondPageLoaded",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Issue 7823"
			});
		}

		async void OnToolbarItemClicked(object sender, EventArgs e)
		{
			await Navigation.PopAsync();
		}
	}
}