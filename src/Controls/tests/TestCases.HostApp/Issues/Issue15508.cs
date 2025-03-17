using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 15508, "Scrollview.ScrollTo execution only returns after manual scroll", PlatformAffected.UWP)]
public class Issue15508 : ContentPage
{
	ScrollView scrollView;
	Label label1;
	public Issue15508()
	{

		var grid = new Grid()
		{
			ColumnDefinitions = new ColumnDefinitionCollection()
			{
				new ColumnDefinition() { Width = GridLength.Auto},
				new ColumnDefinition() { Width = GridLength.Star},
			},
			RowDefinitions = new RowDefinitionCollection()
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			},
			BackgroundColor = Colors.Gray
		};

		var verticalStack = new VerticalStackLayout();

		Button button = new Button()
		{
			Text = "Scroll activated through message",
			WidthRequest = 250,
			HorizontalOptions = LayoutOptions.Start,
			AutomationId = "ButtonToScroll",
		};

		button.Clicked += button_clicked;

		verticalStack.Children.Add(button);

		grid.Add(verticalStack, 1, 0);

		scrollView = new ScrollView
		{
			BackgroundColor = Colors.LightCoral,
			VerticalOptions = LayoutOptions.Start,
			VerticalScrollBarVisibility = ScrollBarVisibility.Always,
			MaximumHeightRequest = 70,
			WidthRequest = 150
		};

		label1 = new Label()
		{
			AutomationId = "ScrollLabel",
			Text =
			"a" + Environment.NewLine +
			"b" + Environment.NewLine +
			"c" + Environment.NewLine +
			"d" + Environment.NewLine +
			"e" + Environment.NewLine +
			"f" + Environment.NewLine +
			"g" + Environment.NewLine +
			"h" + Environment.NewLine +
			"i" + Environment.NewLine +
			"j" + Environment.NewLine +
			"k" + Environment.NewLine +
			"l" + Environment.NewLine +
			"m" + Environment.NewLine +
			"n" + Environment.NewLine +
			"o" + Environment.NewLine +
			"p" + Environment.NewLine +
			"q" + Environment.NewLine +
			"r" + Environment.NewLine +
			"s" + Environment.NewLine +
			"t" + Environment.NewLine +
			"u" + Environment.NewLine +
			"v" + Environment.NewLine +
			"w" + Environment.NewLine +
			"x" + Environment.NewLine +
			"y" + Environment.NewLine +
			"z" + Environment.NewLine +
			"a" + Environment.NewLine +
			"b" + Environment.NewLine +
			"c" + Environment.NewLine +
			"d" + Environment.NewLine +
			"e" + Environment.NewLine +
			"f" + Environment.NewLine +
			"g" + Environment.NewLine +
			"h" + Environment.NewLine +
			"i" + Environment.NewLine +
			"j" + Environment.NewLine +
			"k" + Environment.NewLine +
			"l" + Environment.NewLine +
			"m" + Environment.NewLine +
			"n" + Environment.NewLine +
			"o" + Environment.NewLine +
			"p" + Environment.NewLine +
			"q" + Environment.NewLine +
			"r" + Environment.NewLine +
			"s" + Environment.NewLine +
			"t" + Environment.NewLine +
			"u" + Environment.NewLine +
			"v" + Environment.NewLine +
			"w" + Environment.NewLine +
			"x" + Environment.NewLine +
			"y" + Environment.NewLine +
			"z" + Environment.NewLine
		};

		scrollView.Content = label1;
		grid.Add(scrollView, 0, 0);
		Content = grid;
	}

	void button_clicked(object sender, EventArgs e)
	{
		Application.Current?.Dispatcher.DispatchAsync(async () =>
		{
			await scrollView.ScrollToAsync(0, 0, true);
			label1.Text = "The text is successfully changed";
		});
	}
}

