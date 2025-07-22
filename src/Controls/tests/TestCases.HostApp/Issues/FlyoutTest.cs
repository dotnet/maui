namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 26593, "FlyoutTest", PlatformAffected.iOS)]
public partial class FlyoutTest : TestShell
{

	Tab tabOne = new Tab { Title = "TabOne" };
	Tab tabTwo = new Tab { Title = "TabTwo " };
	protected override void Init()
	{
		var tabOnePage = new ContentPage();
		var stackLayout = new StackLayout();
		stackLayout.Children.Add(new Label { Text = "Tab 1 Page", AutomationId = "Tab1Page", HorizontalOptions = LayoutOptions.Center });
		tabOnePage.Content = stackLayout;
		tabOne.Items.Add(tabOnePage);

		var button = new Button();
		button.Text = "Change Flyout Background";
		button.AutomationId = "ChangeFlyoutBackground";
		button.Clicked += ButtonClicked;
		stackLayout.Children.Add(button);

		var tabTwoPage = new ContentPage();
		var stackLayout2 = new StackLayout();
		stackLayout2.Children.Add(new Label { Text = "Tab 2 Page", HorizontalOptions = LayoutOptions.Center });
		tabTwoPage.Content = stackLayout2;
		tabTwo.Items.Add(tabTwoPage);

		var flyoutItem = new FlyoutItem
		{
			Title = "Tab 1",
			Icon = "bank.png",
			Items = { tabOne }
		};

		var flyoutItem2 = new FlyoutItem
		{
			Title = "Tab 2",
			Icon = "coffee.png",
			Items = { tabTwo }
		};
		Items.Add(flyoutItem);
		Items.Add(flyoutItem2);
	}

	private void ButtonClicked(object sender, EventArgs e)
	{
		SetValue(Shell.FlyoutBackgroundProperty, Colors.LightBlue);
	}

}