namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1323, "tabbed page BarTextColor is not pervasive and can't be applied after instantiation", PlatformAffected.iOS)]
public class Issue1323 : TestTabbedPage
{
	protected override void Init()
	{
		BarBackgroundColor = Color.FromArgb("#61a60e");
		BarTextColor = Color.FromArgb("#ffffff");
		BackgroundColor = Color.FromArgb("#61a60e");

		var page = new ContentPage { SafeAreaEdges = new(SafeAreaRegions.Container), Title = "Page 1", Content = new Button { Text = "Pop", Command = new Command(async () => await Navigation.PopModalAsync()) } };
		var page2 = new ContentPage { Title = "Page 2" };
		var page3 = new ContentPage { Title = "Page 3" };
		var page4 = new ContentPage { Title = "Page 4" };

		Children.Add(page);
		Children.Add(page2);
		Children.Add(page3);
		Children.Add(page4);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		BarTextColor = Colors.White;
		Children.RemoveAt(1);
		Children.Insert(1, new ContentPage { Title = "Page5", IconImageSource = "coffee.png" }); // Need to replace the image as Loyalty.png once it got available

		Children.RemoveAt(3);
		Children.Insert(2, new ContentPage { Title = "Page6", IconImageSource = "star_flyout.png" }); // Need to replace the image as Gift.png once it got available
		BarTextColor = Colors.White;
	}
}