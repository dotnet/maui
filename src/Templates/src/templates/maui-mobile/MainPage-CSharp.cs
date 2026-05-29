namespace MauiApp._1;

// Pattern for a C# MAUI page:
//
//   * The constructor stays small — it sets the page title and calls Build().
//   * Build() composes the visual tree and assigns Content. Keeping the tree
//     in one method makes the page shape easy to scan and gives you a clean
//     seam to extract helpers (BuildHeader(), BuildFooter(), ...) as the
//     page grows.
//   * State that should survive a page reconstruct (persistent data,
//     services) belongs in a ViewModel or service — not on the page.
//   * Controls the class talks to after construction (e.g. _counterButton)
//     are assigned inside Build() so they always reference the live tree.
//
// Hot Reload today:
//   * Method-body edits — e.g. changing OnCounterClicked — are patched live
//     by standard .NET Hot Reload. No extra work needed.
//   * Edits inside Build() do not re-render until the page is reconstructed
//     (navigate away and back, or relaunch). A [MetadataUpdateHandler] that
//     replaces window.Page on update is the usual trick, but on mono-based
//     iOS / MacCatalyst the runtime does not dispatch that callback to user
//     code today, so the handler sits dead. Tracking:
//       dotnet/sdk  #49975 — Hot Reload doesn't work on iOS from Mac
//       dotnet/maui #34714 — MAUI has no registered MetadataUpdateHandler
//     Once those land, the re-render plumbing belongs inside MAUI itself,
//     not in every user page — so the template intentionally does not
//     include a handler class.
public class MainPage : ContentPage
{
	Button _counterButton = null!;
	int _count;

	public MainPage()
	{
		Title = "Home";
		Build();
	}

	void Build()
	{
		var logo = new Image
		{
			Source = "dotnet_bot.png",
			HeightRequest = 185,
			Aspect = Aspect.AspectFit
		};
		SemanticProperties.SetDescription(logo, "dot net bot in a submarine number ten");

		var headline = new Label
		{
			Text = "Hello, World!",
			FontSize = 32,
			FontAttributes = FontAttributes.Bold,
			HorizontalTextAlignment = TextAlignment.Center
		};
		SemanticProperties.SetHeadingLevel(headline, SemanticHeadingLevel.Level1);

		var subtitle = new Label
		{
			Text = "Welcome to \n.NET Multi-platform App UI",
			FontSize = 18,
			HorizontalTextAlignment = TextAlignment.Center
		};
		SemanticProperties.SetHeadingLevel(subtitle, SemanticHeadingLevel.Level2);
		SemanticProperties.SetDescription(subtitle, "Welcome to dot net Multi platform App U I");

		_counterButton = new Button
		{
			Text = FormatCounterText(_count),
			HorizontalOptions = LayoutOptions.Fill
		};
		SemanticProperties.SetHint(_counterButton, "Counts the number of times you click");
		_counterButton.Clicked += OnCounterClicked;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children =
				{
					logo,
					headline,
					subtitle,
					_counterButton
				}
			}
		};
	}

	void OnCounterClicked(object? sender, EventArgs e)
	{
		_count++;

		_counterButton.Text = FormatCounterText(_count);

		SemanticScreenReader.Announce(_counterButton.Text);
	}

	static string FormatCounterText(int count) => count switch
	{
		0 => "Click me",
		1 => $"Clicked {count} time",
		_ => $"Clicked {count} times"
	};
}
