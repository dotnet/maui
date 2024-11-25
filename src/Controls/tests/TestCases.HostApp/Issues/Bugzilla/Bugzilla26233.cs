namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 26233, "Windows phone crashing when going back to page containing listview with Frame inside ViewCell")]
public class Bugzilla26233 : NavigationPage
{
	public Bugzilla26233()
	{
		Navigation.PushAsync(new MainPage());
	}

	public class MainPage : ContentPage
	{
		public MainPage()
		{
			var listview = new ListView();
			listview.ItemTemplate = new DataTemplate(typeof(ItemTemplate));
			listview.ItemsSource = new string[] { "item1", "item2", "item3", "item4", "item5", null, null };
			var btnBack = new Button { Text = "back", AutomationId = "btnBack", Command = new Command(() => Navigation.PopAsync()) };
			listview.ItemSelected += (s, e) => Navigation.PushAsync(new ContentPage { Content = btnBack });
			var btnPush = new Button
			{
				Text = "Next",
				AutomationId = "btnPush",
				Command = new Command(() => Navigation.PushAsync(new ContentPage { Content = btnBack }))
			};

			Content = new StackLayout { Children = { btnPush, listview } };
		}
	}

	internal class ItemTemplate : ViewCell
	{
		public ItemTemplate()
		{
			var frame = new Frame();
			frame.Content = new StackLayout { Children = { new Label { Text = "hello 1" } } };
			View = frame;
		}
	}
}
