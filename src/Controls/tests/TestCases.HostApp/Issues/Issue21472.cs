namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21472, "Shell FlyoutBackgroundImage doesn't shown", PlatformAffected.UWP)]
public class Issue21472 : TestShell
{
	protected override void Init()
	{
		var page = new ContentPage();
		this.BindingContext = this;

		FlyoutBackgroundImage = "dotnet_bot.png";
		AddFlyoutItem(page, "Flyout Item");

		var button = new Button
		{
			Text = "RemoveFlyoutBackground",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "button"
		};

		button.Clicked += (sender, e) =>
		{
			Shell.Current.FlyoutBackgroundImage = null;
		};

		page.Content = button;

		FlyoutContentTemplate = new DataTemplate(() =>
		{
			var stackLayout = new StackLayout();

			var closeButton = new Button
			{
				Text = "Close Flyout",
				AutomationId = "CloseFlyoutButton",
				Margin = new Thickness(20),
				HorizontalOptions = LayoutOptions.Center
			};

			closeButton.Clicked += (sender, e) =>
			{
				Shell.Current.FlyoutIsPresented = false;
			};

			stackLayout.Add(closeButton);
			return stackLayout;
		});
	}
}