namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Bugzilla, 29363, "PushModal followed immediate by PopModal crashes")]
	public class Bugzilla29363 : NavigationPage
	{
		public Bugzilla29363() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
#pragma warning disable CS0618 // Type or member is obsolete
				var layout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
#pragma warning restore CS0618 // Type or member is obsolete

				Button modal = new Button
				{
					AutomationId = "ModalPushPopTest",
					Text = "Modal Push Pop Test",
					FontAttributes = FontAttributes.Bold,
					FontSize = 25,
					HorizontalOptions = LayoutOptions.Center
				};
				modal.Clicked += async (object sender, EventArgs e) =>
				{
					var page = new ContentPage() { BackgroundColor = Colors.Red };

					await Navigation.PushModalAsync(page);

					await Navigation.PopModalAsync(true);
				};

				layout.Children.Add(modal);
				Content = layout;
			}
		}
	}
}