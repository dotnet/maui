using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
				.UseMauiMaps()
				.UseMauiApp<App>()
				.Build();
	}

	class App : Application
	{
		public const string AppName = "CompatibilityGalleryControls";
		public const string DefaultMainPageId = "ControlGalleryMainPage";

		public App()
		{
			SetMainPage(CreateDefaultMainPage());
		}

		public static bool PreloadTestCasesIssuesList { get; set; } = true;

		public void SetMainPage(Page rootPage)
		{
			MainPage = rootPage;
		}

		public Page CreateDefaultMainPage()
		{
			return new CoreNavigationPage();
		}

		protected override void OnAppLinkRequestReceived(Uri uri)
		{
			base.OnAppLinkRequestReceived(uri);
		}
	}
}