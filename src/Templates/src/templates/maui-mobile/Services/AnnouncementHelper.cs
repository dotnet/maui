namespace MauiApp._1.Services;

public static class AnnouncementHelper
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public static async Task Announce(string recipeName)
	{
#if IOS
		var serviceProvider = IPlatformApplication.Current?.Services;
		var _semanticScreenReader = serviceProvider?.GetService<IAsyncAnnouncement>();
		if (_semanticScreenReader is not null)
		{
			await _semanticScreenReader.AnnounceAsync(recipeName);
		}
		else
		{
			SemanticScreenReader.Announce(recipeName);
		}
#else
		SemanticScreenReader.Announce(recipeName);
#endif
	}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

}
