using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.ControlGallery.GalleryPages.RadioButtonGalleries;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class App : Application
	{
		public const string AppName = "CompatibilityGalleryControls";

		// ReSharper disable once InconsistentNaming
		public static int IOSVersion = -1;

		public static List<string> AppearingMessages = new List<string>();

		static Dictionary<string, string> s_config;
		readonly ITestCloudService _testCloudService;

		public const string DefaultMainPageId = "ControlGalleryMainPage";

		public static bool PreloadTestCasesIssuesList { get; set; } = true;
		public App()
		{
			_testCloudService = DependencyService.Get<ITestCloudService>();

			//TestMainPageSwitches();

			SetMainPage(CreateDefaultMainPage());

			//TestMainPageSwitches();

			//SetMainPage(new ImageSourcesGallery());
		}

		protected override void OnStart()
		{
			//TestIssue2393();
		}

		async Task TestBugzilla44596()
		{
			await Task.Delay(50);
			// verify that there is no gray screen displayed between the blue splash and red FlyoutPage.
			SetMainPage(new Bugzilla44596SplashPage(async () =>
			{
				var newTabbedPage = new TabbedPage();
				newTabbedPage.Children.Add(new ContentPage { BackgroundColor = Colors.Red, Content = new Label { Text = "Success" } });
				MainPage = new FlyoutPage
				{
					Flyout = new ContentPage { Title = "Flyout", BackgroundColor = Colors.Red },
					Detail = newTabbedPage
				};

				await Task.Delay(50);
				SetMainPage(CreateDefaultMainPage());
			}));
		}

		async Task TestBugzilla45702()
		{
			await Task.Delay(50);
			// verify that there is no crash when switching MainPage from MDP inside NavPage
			SetMainPage(new Microsoft.Maui.Controls.ControlGallery.Issues.Bugzilla45702());
		}

		void TestIssue2393()
		{
			MainPage = new NavigationPage();

			// Hand off to website for sign in process
			var view = new WebView { Source = new Uri("http://google.com") };
			view.Navigated += (s, e) => MainPage.DisplayAlert("Navigated", $"If this popup appears multiple times, this test has failed", "ok");
			;

			MainPage.Navigation.PushAsync(new ContentPage { Content = view, Title = "Issue 2393" });
			//// Uncomment to verify that there is no gray screen displayed between the blue splash and red FlyoutPage.
			//SetMainPage(new Bugzilla44596SplashPage(() =>
			//{
			//	var newTabbedPage = new TabbedPage();
			//	newTabbedPage.Children.Add(new ContentPage { BackgroundColor = Colors.Red, Content = new Label { Text = "yay" } });
			//	MainPage = new FlyoutPage
			//	{
			//		Flyout = new ContentPage { Title = "Flyout", BackgroundColor = Colors.Red },
			//		Detail = newTabbedPage
			//	};
			//}));

			//// Uncomment to verify that there is no crash when switching MainPage from MDP inside NavPage
			//SetMainPage(new Bugzilla45702());

			//// Uncomment to verify that there is no crash when rapidly switching pages that contain lots of buttons
			//SetMainPage(new Issues.Issue2004());
		}

		async Task TestMainPageSwitches()
		{
			await TestBugzilla45702();

			await TestBugzilla44596();
		}

		public Page CreateDefaultMainPage()
		{
			var layout = new StackLayout { BackgroundColor = Colors.Red };
			layout.Children.Add(new Label { Text = "This is master Page" });
			var master = new ContentPage { Title = "Flyout", Content = layout, BackgroundColor = Colors.SkyBlue, IconImageSource = "menuIcon" };
			master.On<iOS>().SetUseSafeArea(true);
			var mdp = new FlyoutPage
			{
				AutomationId = DefaultMainPageId,
				Flyout = master,
				Detail = CoreGallery.GetMainPage()
			};
			master.IconImageSource.AutomationId = "btnMDPAutomationID";
			mdp.SetAutomationPropertiesName("Main page");
			mdp.SetAutomationPropertiesHelpText("Main page help text");
			mdp.Flyout.IconImageSource.SetAutomationPropertiesHelpText("This as MDP icon");
			mdp.Flyout.IconImageSource.SetAutomationPropertiesName("MDPICON");
			return mdp;
			//return new XamStore.StoreShell();
		}

		protected override void OnAppLinkRequestReceived(Uri uri)
		{
			var appDomain = "http://" + AppName.ToLowerInvariant() + "/";

			if (!uri.ToString().ToLowerInvariant().StartsWith(appDomain))
				return;

			var url = uri.ToString().Replace(appDomain, "");

			var parts = url.Split('/');
			if (parts.Length == 2)
			{
				var isPage = parts[0].Trim().ToLower() == "gallery";
				if (isPage)
				{
					string page = parts[1].Trim();
					var pageForms = Activator.CreateInstance(Type.GetType(page));

					if (pageForms is AppLinkPageGallery appLinkPageGallery)
					{
						appLinkPageGallery.ShowLabel = true;
						(MainPage as FlyoutPage)?.Detail.Navigation.PushAsync((pageForms as Page));
					}
				}
			}

			base.OnAppLinkRequestReceived(uri);
		}

		public static Dictionary<string, string> Config
		{
			get
			{
				if (s_config == null)
					LoadConfig();

				return s_config;
			}
		}

		public static ContentPage MenuPage { get; set; }

		public void SetMainPage(Page rootPage)
		{
			MainPage = rootPage;
		}

		static Assembly GetAssembly(out string assemblystring)
		{
			assemblystring = typeof(App).AssemblyQualifiedName.Split(',')[1].Trim();
			var assemblyname = new AssemblyName(assemblystring);
			return Assembly.Load(assemblyname);
		}

		static void LoadConfig()
		{
			s_config = new Dictionary<string, string>();

			string keyData = LoadResource("controlgallery.config").Result;
			string[] entries = keyData.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (string entry in entries)
			{
				string[] parts = entry.Split(':');
				if (parts.Length < 2)
					continue;

				s_config.Add(parts[0].Trim(), parts[1].Trim());
			}
		}

		static async Task<string> LoadResource(string filename)
		{
			Assembly assembly = GetAssembly(out string assemblystring);

			Stream stream = assembly.GetManifestResourceStream($"{assemblystring}.{filename}");
			string text;
			using (var reader = new StreamReader(stream))
				text = await reader.ReadToEndAsync();
			return text;
		}

		public bool NavigateToTestPage(string test)
		{
			try
			{
				// Create an instance of the main page
				var root = CreateDefaultMainPage();

				// Set up a delegate to handle the navigation to the test page
				EventHandler toTestPage = null;

				toTestPage = async delegate (object sender, EventArgs e)
				{
					await Current.MainPage.Navigation.PushModalAsync(TestCases.GetTestCases());
					TestCases.TestCaseScreen.PageToAction[test]();
					Current.MainPage.Appearing -= toTestPage;
				};

				// And set that delegate to run once the main page appears
				root.Appearing += toTestPage;

				SetMainPage(root);

				return true;
			}
			catch (Exception ex)
			{
				Application.Current.FindMauiContext()?.CreateLogger("UITests").LogWarning(ex, "Error attempting to navigate directly to {Test}", test);
			}

			return false;
		}

		public void Reset()
		{
			SetMainPage(CreateDefaultMainPage());
		}

		public void PlatformTest()
		{
			SetMainPage(new GalleryPages.PlatformTestsGallery.PlatformTestsConsole());
		}
	}
}
