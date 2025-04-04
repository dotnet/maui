namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27143, "Not trigger OnNavigatedTo method when hide the navi bar and using swipe", PlatformAffected.iOS)]
public class Issue27143NavigationPage : NavigationPage
{
	public Issue27143NavigationPage() : base(new Issue27143()) { }

	class Issue27143 : ContentPage
	{
		int _navigatedToEventTriggersCount;

		Label _label;

		public Issue27143()
		{
			ContentPage subpage = new ContentPage()
			{
				Content = new Label()
				{
					Text = "Hello from the other side!",
				}
			};

			SetHasNavigationBar(this, false);

			_label = new Label()
			{
				AutomationId = "navigatedToEventTriggersCountLabel",
			};

			Content = new VerticalStackLayout()
				{
					new Button()
					{
						Text = "Click to navigate",
						AutomationId = "button",
						Command = new Command(() => Window!.Page!.Navigation.PushAsync(subpage, false))
					},
					_label
				};
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			_label.Text = $"NavigatedTo event triggers count: {++_navigatedToEventTriggersCount}";
		}
	}
}