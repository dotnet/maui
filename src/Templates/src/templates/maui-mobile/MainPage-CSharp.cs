//-:cnd:noEmit
#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(MauiApp._1.MainPage.HotReloadHandler))]
#endif
//+:cnd:noEmit

namespace MauiApp._1;

// Pattern:
//   * Keep the constructor small — it sets the page title and calls Build().
//   * Build() composes the visual tree and assigns Content. It can be called
//     again to rebuild the page (used by the Hot Reload hook below).
//   * State that should survive a rebuild (e.g. _count) lives on the instance;
//     Build() reads it so the new tree reflects current state.
//   * Controls the class talks to after construction (e.g. _counterButton)
//     are assigned inside Build() so they always reference the live tree.
//   * Event handler bodies (like OnCounterClicked) are patched live by
//     standard .NET Hot Reload — no extra plumbing needed for those.
//   * Edits inside Build() (layout, properties, adding views) require a
//     rebuild. Both the .NET [MetadataUpdateHandler] and MAUI's
//     [OnHotReload] hooks below call back into RebuildAllLiveInstances()
//     so the UI refreshes on whichever runtime is active.
public class MainPage : ContentPage
{
	Button _counterButton = null!;
	int _count;

//-:cnd:noEmit
#if DEBUG
	static readonly List<WeakReference<MainPage>> s_liveInstances = new();
#endif
//+:cnd:noEmit

	public MainPage()
	{
		Title = "Home";
		Build();
//-:cnd:noEmit
#if DEBUG
		lock (s_liveInstances)
		{
			s_liveInstances.RemoveAll(static wr => !wr.TryGetTarget(out _));
			s_liveInstances.Add(new WeakReference<MainPage>(this));
		}
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
	// Two hooks into the same rebuild so this works on both runtimes:
	//
	//   * [OnHotReload] is invoked by MAUI's MauiHotReloadHelper when the
	//     Visual Studio / VS Code MAUI agent applies a delta (mono on
	//     iOS/MacCatalyst, Android).
	//   * [MetadataUpdateHandler] (the assembly attribute at the top of the
	//     file) is invoked by CoreCLR's built-in Hot Reload — e.g.
	//     CoreCLR-on-Apple (.NET 10 preview / .NET 11), Windows, and
	//     `dotnet watch` when StartupHookSupport is enabled.
	//
	// To use this pattern on another page, copy the assembly attribute at
	// the top of this file, the s_liveInstances field, the registration
	// block in the constructor, and the two hook methods below — then
	// rename MainPage/HotReloadHandler to your page type.
	[Microsoft.Maui.HotReload.OnHotReload]
	static void OnHotReload() => RebuildAllLiveInstances("OnHotReload");

	internal static class HotReloadHandler
	{
		public static void UpdateApplication(System.Type[]? types) =>
			RebuildAllLiveInstances("UpdateApplication");

		public static void ClearCache(System.Type[]? types) =>
			RebuildAllLiveInstances("ClearCache");
	}

	static void RebuildAllLiveInstances(string source)
	{
		System.Diagnostics.Debug.WriteLine($"[HotReload] MainPage rebuild via {source}");
		lock (s_liveInstances)
		{
			s_liveInstances.RemoveAll(static wr => !wr.TryGetTarget(out _));
			foreach (var wr in s_liveInstances)
			{
				if (wr.TryGetTarget(out var page))
				{
					Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(page.Build);
				}
			}
		}
	}
#endif
//+:cnd:noEmit
}
