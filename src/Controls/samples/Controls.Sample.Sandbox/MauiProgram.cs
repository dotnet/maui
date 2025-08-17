using Microsoft.Maui.LifecycleEvents;

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
			.ConfigureLifecycleEvents(events =>
			{
#if ANDROID
				events.AddAndroid(android =>
				{
					android.OnKeyDown((activity, keyCode, keyEvent) =>
					{
						System.Diagnostics.Debug.WriteLine($"OnKeyDown: {keyCode}");
						// Handle volume keys specifically for demonstration
						if (keyCode == Android.Views.Keycode.VolumeUp)
						{
							System.Diagnostics.Debug.WriteLine("Volume Up key handled by lifecycle event!");
							return true; // Prevent default handling
						}
						return false; // Allow default handling for other keys
					});

					android.OnKeyUp((activity, keyCode, keyEvent) =>
					{
						System.Diagnostics.Debug.WriteLine($"OnKeyUp: {keyCode}");
						return false; // Allow default handling
					});

					android.OnKeyLongPress((activity, keyCode, keyEvent) =>
					{
						System.Diagnostics.Debug.WriteLine($"OnKeyLongPress: {keyCode}");
						// Handle back button long press
						if (keyCode == Android.Views.Keycode.Back)
						{
							System.Diagnostics.Debug.WriteLine("Back button long press handled!");
							return true; // Prevent default handling
						}
						return false;
					});

					android.OnKeyMultiple((activity, keyCode, repeatCount, keyEvent) =>
					{
						System.Diagnostics.Debug.WriteLine($"OnKeyMultiple: {keyCode}, repeat: {repeatCount}");
						return false;
					});

					android.OnKeyShortcut((activity, keyCode, keyEvent) =>
					{
						System.Diagnostics.Debug.WriteLine($"OnKeyShortcut: {keyCode}");
						return false;
					});
				});
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
