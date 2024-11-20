namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 32462, "Crash after a page disappeared if a ScrollView is in the HeaderTemplate property of a ListView", PlatformAffected.Android)]
public class Bugzilla32462 : NavigationPage // or TestFlyoutPage, etc ...
{
	public Bugzilla32462()
	{
		Navigation.PushAsync(new MainPage());
	}

	public class MainPage : ContentPage
	{
		public MainPage()
		{
			var button = new Button
			{
				Text = "Click!",
			};
			button.Clicked += (object sender, EventArgs e) => Navigation.PushAsync(new ListViewPage());
			Content = button;
		}
	}

	public class ListViewPage : ContentPage
	{
		public ListViewPage()
		{
			var scrollview = new ScrollView
			{
				Orientation = ScrollOrientation.Horizontal,
				Content = new Label { Text = "some looooooooooooooooooooooooooooooooooooooooooooooooooooooog text" }
			};
			var stacklayout = new StackLayout();
			stacklayout.Children.Add(scrollview);
			string[] list = Enumerable.Range(0, 40).Select(c => $"some text {c}").ToArray();
			var listview = new ListView { AutomationId = "listview", Header = stacklayout, ItemsSource = list };
			Content = listview;

			listview.ScrollTo(list[39], ScrollToPosition.Center, false);
		}
	}
}