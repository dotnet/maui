namespace Maui.Controls.Sample;

using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using MPowerKit.VirtualizeListView;
using CommunityToolkit.Maui;
using RatingControlMaui;
using AlohaKit.Layouts.Hosting;
using Effects;
using The49.Maui.BottomSheet;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp() =>
		MauiApp
			.CreateBuilder()
#if __ANDROID__ || __IOS__
			.UseMauiMaps()
#endif
			.UseMauiApp<AllTheLists.App>()
			
			.UseMauiCommunityToolkit()
			.UseVirtualListView()
			.UseMPowerKitListView()
			.UseFFImageLoading()		
			.UseRatingControl()	
			.UseAlohaKitLayouts()
			.UseBottomSheet()
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
			.ConfigureEffects(effects =>
			{
                
				effects.Add<ContentInsetAdjustmentBehaviorRoutingEffect, ContentInsetAdjustmentBehaviorPlatformEffect>();
                
			})
			.Build();
}
