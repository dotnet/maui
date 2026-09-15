namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36298, "[Windows] ContentPresenter throws ArgumentException when dynamically switching RefreshView or ScrollView content.", PlatformAffected.UWP)]
public class Issue36298 : ContentPage
{
	ContentView _contentHolder;
	View _view1;
	View _view2;

	public Issue36298()
	{
		_view1 = new ContentView
		{
			Content = new RefreshView
			{
				Content = new Label { Text = "View 1 - RefreshView" }
			}
		};

		_view2 = new ContentView
		{
			Content = new ScrollView
			{
				Content = new Label { Text = "View 2 - ScrollView" }
			}
		};

		_contentHolder = new ContentView
		{
			Content = _view1
		};

		var switchToView2Button = new Button
		{
			Text = "Switch to View 2",
			AutomationId = "SwitchToView2"
		};
		switchToView2Button.Clicked += (_, _) => _contentHolder.Content = _view2;

		var switchToView1Button = new Button
		{
			Text = "Switch to View 1",
			AutomationId = "SwitchToView1"
		};
		switchToView1Button.Clicked += (_, _) => _contentHolder.Content = _view1;

		var successLabel = new Label
		{
			Text = "Waiting",
			AutomationId = "SuccessLabel"
		};

		switchToView2Button.Clicked += (_, _) => successLabel.Text = "View2";
		switchToView1Button.Clicked += (_, _) => successLabel.Text = "Success";

		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = new Thickness(20),
			Children =
			{
				switchToView2Button,
				switchToView1Button,
				_contentHolder,
				successLabel
			}
		};
	}
}
