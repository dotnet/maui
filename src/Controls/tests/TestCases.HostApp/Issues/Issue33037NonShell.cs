using LargeTitleDisplayMode = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode;
using Microsoft.Maui.Layouts;
#if IOS
using UIKit;
#endif
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33037, "iOS Large Title display disappears when scrolling in non-Shell NavigationPage", PlatformAffected.iOS, issueTestNumber: 1)]
public class Issue33037NonShell : NavigationPage
{
	public Issue33037NonShell() : base(new Issue33037MenuPage())
	{
		BarBackgroundColor = Colors.Transparent;
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(this, true);
	}
}

class Issue33037MenuPage : ContentPage
{
	public Issue33037MenuPage()
	{
		Title = "Large title scenarios";
		var grid = new Grid { Padding = 16, RowSpacing = 8, ColumnSpacing = 8 };
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
		string[] scenarios = ["Reporter", "Nested", "Table", "Explicit", "NestedExplicit", "Header", "Multiple", "Never", "Opaque", "Hidden", "Margin", "Dynamic", "Transfer"];
		for (var i = 0; i < scenarios.Length; i++)
		{
			if (i % 2 == 0)
				grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

			var scenario = scenarios[i];
			var button = new Button { Text = scenario, AutomationId = scenario };
			button.Clicked += async (_, _) =>
			{
				var navigation = new NavigationPage(new Issue33037ListPage(scenario)) { BarBackgroundColor = Colors.Transparent };
				Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(navigation, true);
				if (scenario == "Opaque")
					Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetIsNavigationBarTranslucent(navigation, false);
				await Navigation.PushModalAsync(navigation);
			};
			grid.Add(button, i % 2, i / 2);
		}
		Content = grid;
	}
}

#pragma warning disable CS0618 // ListView, TableView and ViewCell reproduce the compatibility-control regression.
class Issue33037ListPage : ContentPage
{
	public Issue33037ListPage(string scenario)
	{
		Title = "Large Title Demo";
		BackgroundColor = Color.FromArgb("#F4F7FB");
		if (scenario == "Never")
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(this, LargeTitleDisplayMode.Never);
		if (scenario == "Hidden")
			NavigationPage.SetHasNavigationBar(this, false);

		View scroller;
		if (scenario == "Table")
		{
			var section = new TableSection();
			for (var i = 0; i < 30; i++)
				section.Add(new TextCell { Text = $"Item {i}" });
			scroller = new TableView { Root = new TableRoot { section }, Intent = TableIntent.Menu, RowHeight = 50 };
		}
		else
		{
			scroller = CreateList(scenario == "Nested" ? 60 : 19);
		}
		scroller.AutomationId = "Scroller";
		if (scenario == "Margin")
			scroller.Margin = new Thickness(0, 20, 0, 0);

		var close = new Button
		{
			AutomationId = "CloseScenario",
			Text = "Close",
			BackgroundColor = Color.FromArgb("#0A84FF"),
			TextColor = Colors.White,
			CornerRadius = 12,
			FontAttributes = FontAttributes.Bold,
			ZIndex = 1
		};
		close.Clicked += async (_, _) => await Navigation.PopModalAsync();

		var content = new AbsoluteLayout();
		AbsoluteLayout.SetLayoutFlags(scroller, AbsoluteLayoutFlags.All);
		AbsoluteLayout.SetLayoutBounds(scroller, new Rect(0, 0, 1, 1));
		content.Add(scroller);
		AbsoluteLayout.SetLayoutFlags(close, AbsoluteLayoutFlags.PositionProportional);
		AbsoluteLayout.SetLayoutBounds(close, new Rect(0.5, 1, 200, 40));
		content.Add(close);

		if (scenario == "Nested")
		{
			content.Remove(close);
			var cover = new Button { Text = "Cover", AutomationId = "Cover" };
#if IOS
			cover.Clicked += (_, _) =>
			{
				if (Handler is IPlatformViewHandler { ViewController: { } controller })
					controller.PresentViewController(new Issue33037CoverController { ModalPresentationStyle = UIModalPresentationStyle.FullScreen }, true, null);
			};
#endif
			var footer = new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Spacing = 12, Children = { cover, close } };
			var grid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
			grid.Add(content);
			grid.Add(footer, 0, 1);
			Content = grid;
		}
		else if (scenario is "Header" or "Multiple")
		{
			content.Remove(close);
			var grid = new Grid
			{
				RowDefinitions = { new RowDefinition(scenario == "Header" ? GridLength.Auto : GridLength.Star), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) }
			};
			grid.Add(scenario == "Header" ? new Label { Text = "Fixed header", HeightRequest = 60, AutomationId = "Header" } : CreateList(19));
			grid.Add(content, 0, 1);
			grid.Add(close, 0, 2);
			Content = grid;
		}
		else if (scenario == "NestedExplicit")
		{
			content.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			Content = new Grid { Children = { content } };
		}
		else
		{
			if (scenario == "Explicit")
				content.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			if (scenario == "Dynamic")
			{
				var toggle = new Button { Text = "Use explicit safe area", AutomationId = "ToggleSafeArea" };
				toggle.Clicked += (_, _) =>
				{
					content.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
					toggle.Text = "Explicit safe area";
				};
				AbsoluteLayout.SetLayoutFlags(toggle, AbsoluteLayoutFlags.PositionProportional);
				AbsoluteLayout.SetLayoutBounds(toggle, new Rect(0.5, 0.85, 260, 44));
				content.Add(toggle);
			}
			if (scenario == "Transfer")
			{
				var transfer = new Button { Text = "Transfer list", AutomationId = "TransferList" };
				transfer.Clicked += async (_, _) =>
				{
					content.Remove(scroller);
					await Navigation.PushAsync(new ContentPage
					{
						Title = "Transferred list",
						Content = new Grid { Children = { scroller } }
					});
				};
				AbsoluteLayout.SetLayoutFlags(transfer, AbsoluteLayoutFlags.PositionProportional);
				AbsoluteLayout.SetLayoutBounds(transfer, new Rect(0.5, 0.85, 240, 44));
				content.Add(transfer);
			}
			Content = content;
		}
	}

	static ListView CreateList(int count)
	{
		var items = new string[count];
		for (var i = 0; i < count; i++)
			items[i] = $"Item {i}";
		return new ListView
		{
			RowHeight = 50,
			SeparatorColor = Color.FromArgb("#D5DBE3"),
			BackgroundColor = Colors.White,
			ItemsSource = items,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = new Thickness(20, 0), TextColor = Color.FromArgb("#1D2733"), VerticalTextAlignment = TextAlignment.Center };
				label.SetBinding(Label.TextProperty, ".");
				return new ViewCell { View = label };
			})
		};
	}
}
#pragma warning restore CS0618

#if IOS
sealed class Issue33037CoverController : UIViewController
{
	UIButton _dismiss;

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		View.BackgroundColor = UIColor.SystemBackground;
		_dismiss = UIButton.FromType(UIButtonType.System);
		_dismiss.SetTitle("Dismiss cover", UIControlState.Normal);
		_dismiss.AccessibilityIdentifier = "DismissCover";
		_dismiss.TranslatesAutoresizingMaskIntoConstraints = false;
		_dismiss.TouchUpInside += DismissCover;
		View.AddSubview(_dismiss);
		NSLayoutConstraint.ActivateConstraints(
		[
			_dismiss.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
			_dismiss.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
			_dismiss.WidthAnchor.ConstraintEqualTo(200),
			_dismiss.HeightAnchor.ConstraintEqualTo(50)
		]);
	}

	void DismissCover(object sender, EventArgs args) => DismissViewController(true, null);

	protected override void Dispose(bool disposing)
	{
		if (disposing && _dismiss is not null)
		{
			_dismiss.TouchUpInside -= DismissCover;
			_dismiss.Dispose();
			_dismiss = null;
		}
		base.Dispose(disposing);
	}
}
#endif
