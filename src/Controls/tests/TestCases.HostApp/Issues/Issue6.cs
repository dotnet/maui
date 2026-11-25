using Microsoft.Maui.Controls.Animations;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6, "Page transitions: a shared element lands where it belongs and pages return to rest", PlatformAffected.All, NavigationBehavior.SetApplicationRoot)]
public class Issue6 : NavigationPage
{
	public Issue6() : base(new Issue6ListPage())
	{
	}
}

public class Issue6ListPage : ContentPage
{
	readonly Label _status;

	public Issue6ListPage()
	{
		Title = "Page transitions";
		_status = new Label { AutomationId = "Status", Text = "Ready" };

		var tiles = new VerticalStackLayout { Spacing = 12, Padding = 16 };
		tiles.Add(_status);
		for (int i = 1; i <= 3; i++)
		{
			var id = i.ToString(System.Globalization.CultureInfo.InvariantCulture);
			var tile = new Button
			{
				AutomationId = "Tile" + id,
				Text = "Tile " + id,
				HeightRequest = 80,
				BackgroundColor = Colors.SteelBlue,
				TextColor = Colors.White,
			};
			SharedTransition.SetName(tile, "tile");
			SharedTransition.SetGroup(tile, id);
			tile.Clicked += async (_, _) =>
				await Navigation.PushAsync(new Issue6DetailPage(id), new SlideTransition { Duration = TimeSpan.FromMilliseconds(350) });
			tiles.Add(tile);
		}

		Content = tiles;
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		if (args.PreviousPage is Issue6DetailPage detail)
		{
			_status.Text = "Returned from " + detail.Id;
		}
	}
}

public class Issue6DetailPage : ContentPage
{
	readonly Label _status;

	public Issue6DetailPage(string id)
	{
		Id = id;
		Title = "Detail " + id;
		SharedTransition.SetGroup(this, id);

		// A Border, so Appium finds it on every platform (it has its own automation peer on WinUI, unlike a textless
		// BoxView) and reports its laid-out size; a Label's WinUI TextBlock is only as tall as its text.
		var hero = new Border
		{
			AutomationId = "DetailTile",
			BackgroundColor = Colors.SteelBlue,
			StrokeThickness = 0,
			HeightRequest = 240,
			Content = new Label { Text = "Detail", TextColor = Colors.White, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center },
		};
		SharedTransition.SetName(hero, "tile");

		_status = new Label { AutomationId = "DetailStatus", Text = "Arriving" };
		var back = new Button { AutomationId = "BackButton", Text = "Back" };
		back.Clicked += async (_, _) => await Navigation.PopAsync();

		Content = new VerticalStackLayout { Spacing = 12, Padding = 16, Children = { hero, _status, back } };
	}

	public new string Id { get; }

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		_status.Text = "Arrived at " + Id;
	}
}
