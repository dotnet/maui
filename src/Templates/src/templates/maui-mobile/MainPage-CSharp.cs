//-:cnd:noEmit
#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(MauiApp._1.MainPageHotReloadHandler))]
#endif
//+:cnd:noEmit

namespace MauiApp._1;

// Pattern:
//   * Keep the constructor small — it sets the page title and calls Build().
//   * Build() composes the visual tree and assigns Content. It can be called
//     again to rebuild the page (used by the Hot Reload handler below).
//   * State that should survive a rebuild (e.g. _count) lives on the instance;
//     Build() reads it so the new tree reflects current state.
//   * Controls the class talks to after construction (e.g. _counterButton)
//     are assigned inside Build() so they always reference the live tree.
//   * Event handler bodies (like OnCounterClicked) are patched live by
//     standard .NET Hot Reload — no extra plumbing needed for those.
public class MainPage : ContentPage
{
	Button _counterButton = null!;
	int _count;

	public MainPage()
	{
		Title = "Home";
		Build();
//-:cnd:noEmit
#if DEBUG
		MainPageHotReloadHandler.Register(this);
#endif
//+:cnd:noEmit
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

//-:cnd:noEmit
#if DEBUG
	internal void HotReloadRebuild() =>
		Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(Build);
#endif
//+:cnd:noEmit
}

//-:cnd:noEmit
#if DEBUG
// Hot Reload wiring (DEBUG only — compiled out of Release builds).
//
// .NET Hot Reload calls UpdateApplication/ClearCache whenever types in the
// app are updated. For C#-only UI we rebuild the page by calling Build()
// again, which re-creates the visual tree in place. The page instance,
// BindingContext, and fields are preserved — so per-page state (e.g. the
// click counter) survives the rebuild.
//
// To use this pattern on another page, copy the [assembly: ...] attribute
// at the top of this file, the handler class below, and the Register call
// in the constructor, adjusting the type names.
static class MainPageHotReloadHandler
{
	static readonly List<WeakReference<MainPage>> s_pages = new();

	public static void Register(MainPage page)
	{
		lock (s_pages)
		{
			s_pages.RemoveAll(static wr => !wr.TryGetTarget(out _));
			s_pages.Add(new WeakReference<MainPage>(page));
		}
	}

	public static void UpdateApplication(Type[]? types)
	{
		if (types is not null && Array.IndexOf(types, typeof(MainPage)) < 0)
			return;

		lock (s_pages)
		{
			s_pages.RemoveAll(static wr => !wr.TryGetTarget(out _));
			foreach (var wr in s_pages)
				if (wr.TryGetTarget(out var page))
					page.HotReloadRebuild();
		}
	}

	public static void ClearCache(Type[]? types) => UpdateApplication(types);
}
#endif
//+:cnd:noEmit
