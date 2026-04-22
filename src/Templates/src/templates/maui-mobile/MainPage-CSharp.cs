//-:cnd:noEmit
#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(MauiApp._1.HotReloadHandler))]
#endif
//+:cnd:noEmit

namespace MauiApp._1;

// Pattern for a C# MAUI page:
//
//   * The constructor stays small — it sets the page title and calls Build().
//   * Build() composes the visual tree and assigns Content. Keeping the tree
//     in one method makes the page shape easy to scan and gives you a clean
//     seam to extract helpers (BuildHeader(), BuildFooter(), ...) as the
//     page grows.
//   * State that should survive the page being reconstructed (persistent
//     data, services) belongs in a ViewModel or service — not on the page —
//     because Hot Reload replaces the whole page instance (see below).
//   * Controls the class talks to after construction (e.g. _counterButton)
//     are assigned inside Build() so they always reference the live tree.
//
// Hot Reload:
//   * Method-body edits (e.g. changing OnCounterClicked) are patched live
//     by standard .NET Hot Reload. No extra work needed.
//   * When you edit the Build() method, the HotReloadHandler below replaces
//     the current root Page with a fresh MainPage instance so your layout
//     changes render immediately. Press the 🔥 "Apply Code Changes" button
//     in VS / VS Code to trigger it.
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

//-:cnd:noEmit
#if DEBUG
// Hot Reload handler for C# UI edits.
//
// .NET Hot Reload patches the running IL when you press 🔥 "Apply Code
// Changes", but it does not tell MAUI to redraw. This handler listens for
// the update notification and replaces each window's Page with a fresh
// instance of the same type, so edits inside Build() take effect
// immediately.
//
// State held on the Page instance (like the counter value in MainPage) is
// reset by this because the whole page is reconstructed. Move anything
// that should outlive a Hot Reload into a ViewModel or service.
//
// To use this pattern on another page, add the page's type to the
// 'if' check in UpdateApplication (or compare against a common base class).
// Pattern adapted from CommunityToolkit.Maui.Markup's HotReloadHandler:
// https://learn.microsoft.com/dotnet/communitytoolkit/maui/markup/dotnet-hot-reload
internal static class HotReloadHandler
{
	public static void UpdateApplication(System.Type[]? types)
	{
		if (types is null || Application.Current?.Windows is null)
		{
			return;
		}

		Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
		{
			foreach (var window in Application.Current.Windows)
			{
				if (window.Page is null)
				{
					continue;
				}

				var currentPageType = window.Page.GetType();
				foreach (var type in types)
				{
					if (type == currentPageType || currentPageType.IsSubclassOf(type))
					{
						if (System.Activator.CreateInstance(currentPageType) is Page fresh)
						{
							window.Page = fresh;
						}
						break;
					}
				}
			}
		});
	}
}
#endif
//+:cnd:noEmit
