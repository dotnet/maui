namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25599, "OnNavigating wrong target when tapping the same tab", PlatformAffected.iOS)]
public partial class Issue25599 : Shell
{
	public Issue25599()
	{
		InitializeComponent();
	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new DetailsPage());
	}

	public class DetailsPage : ContentPage
	{
		public DetailsPage()
		{
			Title = "DetailsPage";

			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "Details Page",
						AutomationId = "DetailsPageLabel"
					}
				}
			};
		}
	}
}