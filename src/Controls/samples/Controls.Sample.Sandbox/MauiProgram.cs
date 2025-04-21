namespace Maui.Controls.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp() =>
		MauiApp
			.CreateBuilder()
#if __ANDROID__ || __IOS__
			.UseMauiMaps()
#endif
			.UseMauiApp<App>()
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

#if IOS || MACCATALYST
		 .ConfigureMauiHandlers(handlers =>
			 {
				 handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
				 handlers.AddHandler<Microsoft.Maui.Controls.CarouselView, Microsoft.Maui.Controls.Handlers.Items2.CarouselViewHandler2>();
			 })
#endif
			.Build();
}
