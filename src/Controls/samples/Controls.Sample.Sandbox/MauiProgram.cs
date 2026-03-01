using Microsoft.Maui.LifecycleEvents;
using MauiDevFlow.Agent;

namespace Maui.Controls.Sample;

public static class MauiProgram
{
	const string WidgetAppGroup = "group.com.microsoft.maui.sandbox";
	const string WidgetKind = "MauiSandboxWidget";

	public static MauiApp CreateMauiApp() =>
		MauiApp
			.CreateBuilder()
#if __ANDROID__ || __IOS__
			.UseMauiMaps()
#endif
			.UseMauiApp<App>()
			.AddMauiDevFlowAgent()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Dokdo-Regular.ttf", "Dokdo");
				fonts.AddFont("LobsterTwo-Regular.ttf", "Lobster Two");
				fonts.AddFont("LobsterTwo-Bold.ttf", "Lobster Two Bold");
				fonts.AddFont("LobsterTwo-Italic.ttf", "Lobster Two Italic");
				fonts.AddFont("LobsterTwo-BoldItalic.ttf", "Lobster Two BoldItalic");
				fonts.AddFont("ionicons.ttf", "Ionicons");
				fonts.AddFont("SegoeUI.ttf", "Segoe UI");
				fonts.AddFont("SegoeUI-Bold.ttf", "Segoe UI Bold");
				fonts.AddFont("SegoeUI-Italic.ttf", "Segoe UI Italic");
				fonts.AddFont("SegoeUI-Bold-Italic.ttf", "Segoe UI Bold Italic");
			})
			.ConfigureLifecycleEvents(events =>
			{
#if IOS || MACCATALYST
#pragma warning disable CA1416
				events.AddiOS(ios => ios
					.FinishedLaunching((app, options) =>
					{
						UpdateWidgetData("MAUI Sandbox", "Hello from .NET MAUI!", "🟣");
						return true;
					}));
#pragma warning restore CA1416
#endif
#if IOS && !MACCATALYST
#pragma warning disable CA1416
				events.AddiOS(ios => ios
					.CarPlayDidConnect((scene, interfaceController) =>
					{
						var favorites = new CarPlay.ICPListTemplateItem[]
						{
							CreateSong("Bohemian Rhapsody", "Queen"),
							CreateSong("Hotel California", "Eagles"),
							CreateSong("Stairway to Heaven", "Led Zeppelin"),
							CreateSong("Imagine", "John Lennon"),
							CreateSong("Smells Like Teen Spirit", "Nirvana"),
						};

						var recent = new CarPlay.ICPListTemplateItem[]
						{
							CreateSong("Blinding Lights", "The Weeknd"),
							CreateSong("Shape of You", "Ed Sheeran"),
							CreateSong("Levitating", "Dua Lipa"),
						};

						var favSection = new CarPlay.CPListSection(favorites, "⭐ Favorites", "");
						var recentSection = new CarPlay.CPListSection(recent, "🕐 Recently Played", "");

						var template = new CarPlay.CPListTemplate(
							"MAUI Music", new[] { favSection, recentSection });

						interfaceController.SetRootTemplate(template, true, null);
					})
					.CarPlayDidDisconnect((scene, controller) =>
					{
						System.Diagnostics.Debug.WriteLine("[CarPlay] Disconnected");
					}));
#pragma warning restore CA1416
#endif
			})
			.Build();

#if IOS || MACCATALYST
#pragma warning disable CA1416
	public static void UpdateWidgetData(string title, string message, string emoji)
	{
		// Write to UserDefaults (works on device with provisioning)
		var defaults = Microsoft.Maui.MauiWidgetCenter.GetSharedDefaults(WidgetAppGroup);
		if (defaults is not null)
		{
			defaults.SetString(title, "widget_title");
			defaults.SetString(message, "widget_message");
			defaults.SetString(emoji, "widget_emoji");
			defaults.Synchronize();
		}
		
		// Also write JSON file to App Group container (reliable on simulator)
		var containerUrl = Foundation.NSFileManager.DefaultManager.GetContainerUrl(WidgetAppGroup);
		if (containerUrl is not null)
		{
			var json = $"{{\"title\":\"{title}\",\"message\":\"{message}\",\"emoji\":\"{emoji}\"}}";
			var filePath = System.IO.Path.Combine(containerUrl.Path!, "widget_data.json");
			System.IO.File.WriteAllText(filePath, json);
		}
		
		Microsoft.Maui.MauiWidgetCenter.ReloadTimelines(WidgetKind);
	}
#pragma warning restore CA1416
#endif

#if IOS && !MACCATALYST
#pragma warning disable CA1416
	static CarPlay.CPListItem CreateSong(string title, string artist)
	{
		var item = new CarPlay.CPListItem(title, artist);
		item.Handler = (listItem, completion) =>
		{
			System.Diagnostics.Debug.WriteLine($"[CarPlay] Now playing: {title} by {artist}");
			completion();
		};
		return item;
	}
#pragma warning restore CA1416
#endif
}
