using Microsoft.Maui.LifecycleEvents;
#if __IOS__
using System.Runtime.InteropServices;
using UIKit;
using Foundation;
#endif

namespace Maui.Controls.Sample;

public static class MauiProgram
{
#if __IOS__
	[DllImport(ObjCRuntime.Constants.FoundationLibrary)]
	static extern void NSLog(System.IntPtr format);

	static void Log(string message)
	{
		using var str = new NSString(message);
		NSLog(str.Handle);
	}

#endif

	public static MauiApp CreateMauiApp() =>
		MauiApp
			.CreateBuilder()
#if __ANDROID__ || __IOS__
			.UseMauiMaps()
#endif
			.UseMauiApp<App>()
#if __IOS__
#pragma warning disable CA1416, CA1422
			.ConfigureLifecycleEvents(events =>
			{
				events.AddiOS(ios => ios
					.CarPlayDidConnect((scene, interfaceController) =>
					{
						Log("CARPLAY: CarPlay scene connected! Setting root template...");
						try
						{
							var item = new CarPlay.CPListItem("hello MauiCarkit", ".NET MAUI + CarPlay");
							var section = new CarPlay.CPListSection(
								new CarPlay.ICPListTemplateItem[] { item }, "MAUI CarPlay", "");
							var listTemplate = new CarPlay.CPListTemplate(
								"MAUI CarPlay", new CarPlay.CPListSection[] { section });
							interfaceController.SetRootTemplate(listTemplate, true, (success, error) =>
							{
								if (error != null)
									Log($"CARPLAY: SetRootTemplate error: {error.LocalizedDescription}");
								else
									Log("CARPLAY: Root template set successfully!");
							});
						}
						catch (System.Exception ex)
						{
							Log($"CARPLAY: Exception: {ex.Message}");
						}
					})
					.CarPlayDidDisconnect((scene, interfaceController) =>
					{
						Log("CARPLAY: Disconnected");
					})
					.FinishedLaunching((app, options) =>
					{
						Log("CARPLAY: FinishedLaunching fired!");
						return true;
					}));
			})
#pragma warning restore CA1416, CA1422
#endif
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
