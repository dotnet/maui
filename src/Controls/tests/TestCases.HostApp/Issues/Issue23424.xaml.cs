namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "23424", "BackButtonBehavior IsVisible=False does not hide the back button", PlatformAffected.All)]
public partial class Issue23424 : Shell
{
	public Issue23424()
	{
		Items.Add(new ContentPage());
		Navigation.PushAsync(new DetailPage());
	}

	public class DetailPage : ContentPage
	{
		public DetailPage()
		{
			Title = "Detail page";
			BackButtonBehavior backButtonBehavior = new BackButtonBehavior
			{
				IconOverride = "small_dotnet_bot.png"
			};

			SetBackButtonBehavior(this, backButtonBehavior);
			Content = new VerticalStackLayout()
			{
				new Button()
				{
					AutomationId = "button",
					Text = "Click to hide the back button",
					Command = new Command(()=>backButtonBehavior.IsVisible =! backButtonBehavior.IsVisible)
				}
			};
		}
	}
}