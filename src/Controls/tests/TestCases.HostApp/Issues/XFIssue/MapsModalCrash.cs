using Microsoft.Maui.Controls.Maps;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 1701, "Modal Page over Map crashes application", PlatformAffected.Android)]
public class MapsModalCrash : TestContentPage
{
	const string StartTest = "Start Test";
	const string DisplayModal = "Click Me";
	const string Success = "SuccessLabel";

	protected override void Init()
	{
		var button = new Button { Text = StartTest };
		button.Clicked += (sender, args) =>
		{
			Application.Current.MainPage = MapPage();
		};

		var stackLayout = new StackLayout
		{
			button
		};

		stackLayout.Padding = new Thickness(0, 20, 0, 0);

		Content = stackLayout;
	}

	static ContentPage MapPage()
	{
		var map = new Microsoft.Maui.Controls.Maps.Map();

		var button = new Button { Text = DisplayModal };
		button.Clicked += (sender, args) => button.Navigation.PushModalAsync(new NavigationPage(SuccessPage()));

		return new ContentPage
		{
			Content = new StackLayout
			{
				map,
				button
			}
		};
	}

	static ContentPage SuccessPage()
	{
		return new ContentPage
		{
			BackgroundColor = Colors.LightBlue,
			Content = new Label { Text = "If you're seeing this, then the test was a success.", AutomationId = Success }
		};
	}
}