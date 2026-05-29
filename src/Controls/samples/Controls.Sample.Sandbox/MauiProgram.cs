namespace Maui.Controls.Sample;

public static class MauiProgram
{
	// Toggle this to test different handlers
	// true = Use Handler2 (CURRENT DEFAULT - the one being fixed in PR)
	// false = Use Handler1 (LEGACY - to compare behavior)
	private static bool UseHandler2 = true;

	public static MauiApp CreateMauiApp() =>
		MauiApp
			.CreateBuilder()
#if __ANDROID__ || __IOS__
			.UseMauiMaps()
#endif
			.UseMauiApp<App>()
			.ConfigureMauiHandlers(handlers =>
			{
#if IOS || MACCATALYST
				if (!UseHandler2)
				{
					// Force use of legacy Handler1 by overriding default
					handlers.AddHandler<Microsoft.Maui.Controls.CarouselView, Microsoft.Maui.Controls.Handlers.Items.CarouselViewHandler>();
					Console.WriteLine("✅ Forcing CarouselViewHandler1 (legacy)");
				}
				else
				{
					Console.WriteLine("✅ Using CarouselViewHandler2 (current default)");
				}
#endif
			})
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
			.Build();
}
