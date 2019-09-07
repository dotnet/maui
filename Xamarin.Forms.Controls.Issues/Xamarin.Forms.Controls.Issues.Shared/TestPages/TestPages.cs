using System;
using System.Reflection;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
using Xamarin.UITest;

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
[assembly: Category("Issues")]
#endif

namespace Xamarin.Forms.Controls
{
	internal static class AppPaths
	{
		public static string ApkPath = "../../../Xamarin.Forms.ControlGallery.Android/bin/Debug/AndroidControlGallery.AndroidControlGallery-Signed.apk";

		public static string MacOSPath = "../../../Xamarin.Forms.ControlGallery.MacOS/bin/Debug/Xamarin.Forms.ControlGallery.MacOS.app";

		// Have to continue using the old BundleId for now; Test Cloud doesn't like
		// when you change the BundleId
		public static string BundleId = "com.xamarin.quickui.controlgallery";

		// Have to continue using the old BundleId for now; Test Cloud doesn't like
		// when you change the BundleId
		public static string MacOSBundleId = "com.xamarin.xamarin-forms-controlgallery-macos";
	}

#if UITEST
	internal static class AppSetup
	{
		static IApp InitializeApp()
		{
			IApp app = null;
#if __ANDROID__

			app = InitializeAndroidApp();

#elif __IOS__

			app = InitializeiOSApp();
			
#elif __MACOS__
			Xamarin.UITest.Desktop.TestAgent.Start();
			app = InitializeMacOSApp();

#elif __WINDOWS__
			app = InitializeUWPApp();
#endif
			if (app == null)
				throw new NullReferenceException("App was not initialized.");

			// Wrap the app in ScreenshotConditional so it only takes screenshots if the SCREENSHOTS symbol is specified
			return new ScreenshotConditionalApp(app);
		}

#if __ANDROID__
		static IApp InitializeAndroidApp()
		{
			var app = ConfigureApp.Android.ApkFile(AppPaths.ApkPath).Debug().StartApp();

			if (bool.Parse((string)app.Invoke("IsPreAppCompat")))
			{
				IsFormsApplicationActivity = true;
			}

			return app;
		}

		public static bool IsFormsApplicationActivity { get; private set; }
#endif

#if __IOS__
		static IApp InitializeiOSApp() 
		{ 
			// Running on a device
			var app = ConfigureApp.iOS.InstalledApp(AppPaths.BundleId).Debug()
				//Uncomment to run from a specific iOS SIM, get the ID from XCode -> Devices
				.StartApp(Xamarin.UITest.Configuration.AppDataMode.DoNotClear);
			int _iosVersion;
			if (int.TryParse(app.Invoke("iOSVersion").ToString(), out _iosVersion))
			{
				iOSVersion = _iosVersion;
			}

			// Running on the simulator
			//var app = ConfigureApp.iOS
			//				  .PreferIdeSettings()
			//		  		  .AppBundle("../../../Xamarin.Forms.ControlGallery.iOS/bin/iPhoneSimulator/Debug/XamarinFormsControlGalleryiOS.app")
			//				  .Debug()
			//				  .StartApp();

			return app;
		}

		public static int iOSVersion { get; private set; }
#endif

#if __MACOS__
		static IApp InitializeMacOSApp()
		{
			// Running on a device
			var configurator = new Xamarin.UITest.Desktop.CocoaAppConfigurator();
			var app = configurator
							//.AppBundle("/Users/rmarinho/Xamarin/Xamarin.Forms/Xamarin.Forms.ControlGallery.MacOS/bin/Debug/Xamarin.Forms.ControlGallery.MacOS.app")
							.AppBundle(AppPaths.MacOSPath)
							.BundleId(AppPaths.MacOSBundleId)
							.StartApp();
			return new Xamarin.Forms.Core.macOS.UITests.MacOSApp(app);
		}
#endif

#if __WINDOWS__
		static IApp InitializeUWPApp()
		{
			return WindowsTestBase.ConfigureApp();
		}
#endif

		public static void NavigateToIssue(Type type, IApp app)
		{
			var typeIssueAttribute = type.GetTypeInfo().GetCustomAttribute<IssueAttribute>();

			string cellName = "";
			if (typeIssueAttribute.IssueTracker.ToString() != "None" &&
				typeIssueAttribute.IssueNumber != 1461 &&
				typeIssueAttribute.IssueNumber != 342)
			{
				cellName = typeIssueAttribute.DisplayName;
			}
			else
			{
				cellName = typeIssueAttribute.Description;
			}

			int maxAttempts = 2;
			int attempts = 0;

			while (attempts < maxAttempts)
			{
				attempts += 1;

				try
				{
					// Attempt the direct way of navigating to the test page
#if __ANDROID__

					if (bool.Parse((string)app.Invoke("NavigateToTest", cellName)))
					{
						return;
					}
#endif
#if __IOS__
				if (bool.Parse(app.Invoke("navigateToTest:", cellName).ToString()))
				{
					return;
				}
#endif

#if __WINDOWS__
					// Windows doens't have an 'invoke' option right now for us to do the more direct navigation
					// we're using for Android/iOS
					// So we're just going to use the 'Reset' method to bounce the app to the opening screen
					// and then fall back to the old manual navigation
					WindowsTestBase.Reset();
#endif
				}
				catch (Exception ex)
				{
					var debugMessage = $"Could not directly invoke test; using UI navigation instead. {ex}";

					System.Diagnostics.Debug.WriteLine(debugMessage);
					Console.WriteLine(debugMessage);
				}

				try
				{
					// Fall back to the "manual" navigation method
					app.Tap(q => q.Button("Go to Test Cases"));
					app.WaitForElement(q => q.Raw("* marked:'TestCasesIssueList'"));

					app.EnterText(q => q.Raw("* marked:'SearchBarGo'"), cellName);

					app.WaitForElement(q => q.Raw("* marked:'SearchButton'"));
					app.Tap(q => q.Raw("* marked:'SearchButton'"));

					return;
				}
				catch (Exception ex)
				{
					var debugMessage = $"Both navigation methods failed. {ex}";

					System.Diagnostics.Debug.WriteLine(debugMessage);
					Console.WriteLine(debugMessage);

					if (attempts < maxAttempts)
					{
						// Something has failed and we're stuck in a place where we can't navigate
						// to the test. Usually this is because we're getting network/HTTP errors 
						// communicating with the server on the device. So we'll try restarting the app.
						RunningApp = InitializeApp();
					}
					else
					{
						// But if it's still not working after [maxAttempts], we've got assume this is a legit
						// problem that restarting won't fix
						throw;
					}
				}
			}
		}

		public static IApp Setup(Type pageType = null)
		{
			IApp runningApp = null;
			try
			{
				runningApp = InitializeApp();
			}
			catch (Exception e)
			{
				Assert.Inconclusive($"App did not start for some reason: {e}");
			}

			if (pageType != null)
				NavigateToIssue(pageType, runningApp);

			return runningApp;
		}

		// Make sure the server on the device is still up and running;
		// if not, restart the app
		public static void EnsureConnection()
		{
			if (RunningApp != null)
			{
				try
				{
					RunningApp.TestServer.Get("version");
					return;
				}
				catch
				{
				}

				RunningApp = InitializeApp();
			}
		}

		static int s_testsrun;
		const int ConsecutiveTestLimit = 20;

		// Until we get more of our memory leak issues worked out, restart the app 
		// after a specified number of tests so we don't get bogged down in GC
		// (or booted by jetsam)
		public static void EnsureMemory()
		{
			if (RunningApp != null)
			{
				s_testsrun += 1;

				if (s_testsrun >= ConsecutiveTestLimit)
				{
					s_testsrun = 0;
					RunningApp = InitializeApp();
				}
			}
		}

		// For tests which just don't play well with others, we can ensure
		// that they run in their own instance of the application
		public static void BeginIsolate()
		{
			if (RunningApp != null && s_testsrun > 0)
			{
				s_testsrun = 0;
				RunningApp = InitializeApp();
			}
		}

		public static void EndIsolate()
		{
			s_testsrun = ConsecutiveTestLimit;
		}

		public static IApp RunningApp { get; set; }
	}
#endif

	public abstract class TestPage : Page
	{
#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;

		protected virtual bool Isolate => false;
#endif

		protected TestPage()
		{
#if APP
			Init();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			if (Isolate)
			{
				AppSetup.BeginIsolate();
			}
			else
			{
				AppSetup.EnsureMemory();
				AppSetup.EnsureConnection();
			}

			AppSetup.NavigateToIssue(GetType(), RunningApp);
		}

		[TearDown]
		public void TearDown()
		{
			if (Isolate)
			{
				AppSetup.EndIsolate();
			}
		}
#endif

		protected abstract void Init();
	}


	public abstract class TestContentPage : ContentPage
	{
#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;

		protected virtual bool Isolate => false;
#endif

		protected TestContentPage()
		{
#if APP
			Init();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			if (Isolate)
			{
				AppSetup.BeginIsolate();
			}
			else
			{
				AppSetup.EnsureMemory();
				AppSetup.EnsureConnection();
			}

			AppSetup.NavigateToIssue(GetType(), RunningApp);
		}

		[TearDown]
		public virtual void TearDown()
		{
			if (Isolate)
			{
				AppSetup.EndIsolate();
			}
		}
#endif

		protected abstract void Init();
	}

	public abstract class TestCarouselPage : CarouselPage
	{
#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;

		protected virtual bool Isolate => false;
#endif

		protected TestCarouselPage()
		{
#if APP
			Init();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			if (Isolate)
			{
				AppSetup.BeginIsolate();
			}
			else
			{
				AppSetup.EnsureMemory();
				AppSetup.EnsureConnection();
			}

			AppSetup.NavigateToIssue(GetType(), RunningApp);
		}

		[TearDown]
		public void TearDown()
		{
			if (Isolate)
			{
				AppSetup.EndIsolate();
			}
		}
#endif

		protected abstract void Init();
	}

#if UITEST
	[Category(Core.UITests.UITestCategories.MasterDetailPage)]
#endif
	public abstract class TestMasterDetailPage : MasterDetailPage
	{
#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;

		protected virtual bool Isolate => false;
#endif

		protected TestMasterDetailPage()
		{
#if APP
			Init();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			if (Isolate)
			{
				AppSetup.BeginIsolate();
			}
			else
			{
				AppSetup.EnsureMemory();
				AppSetup.EnsureConnection();
			}

			AppSetup.NavigateToIssue(GetType(), RunningApp);
		}

		[TearDown]
		public virtual void TearDown()
		{
			if (Isolate)
			{
				AppSetup.EndIsolate();
			}
		}
#endif

		protected abstract void Init();
	}

	public abstract class TestNavigationPage : NavigationPage
	{
#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;

		protected virtual bool Isolate => false;
#endif

		protected TestNavigationPage()
		{
#if APP
			Init();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			if (Isolate)
			{
				AppSetup.BeginIsolate();
			}
			else
			{
				AppSetup.EnsureMemory();
				AppSetup.EnsureConnection();
			}

			AppSetup.NavigateToIssue(GetType(), RunningApp);
		}

		[TearDown]
		public void TearDown()
		{
			if (Isolate)
			{
				AppSetup.EndIsolate();
			}
		}
#endif

		protected abstract void Init();
	}

#if UITEST
	[Category(UITestCategories.TabbedPage)]
#endif
	public abstract class TestTabbedPage : TabbedPage
	{
#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;

		protected virtual bool Isolate => false;
#endif

		protected TestTabbedPage()
		{
#if APP
			Init();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			if (Isolate)
			{
				AppSetup.BeginIsolate();
			}
			else
			{
				AppSetup.EnsureMemory();
				AppSetup.EnsureConnection();
			}

			AppSetup.NavigateToIssue(GetType(), RunningApp);
		}

		[TearDown]
		public virtual void TearDown()
		{
			if (Isolate)
			{
				AppSetup.EndIsolate();
			}
		}
#endif

		protected abstract void Init();
	}



#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public abstract class TestShell : Shell
	{
		protected const string FlyoutIconAutomationId = "OK";
#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;
		protected virtual bool Isolate => true;
#endif

		protected TestShell() : base()
		{
#if APP
			Init();
#endif
		}

		public ContentPage AddTopTab(string title)
		{
			var page = new ContentPage();
			AddTopTab(page, title);
			return page;
		}


		public void AddTopTab(ContentPage page, string title = null)
		{
			if(Items.Count == 0)
			{
				var item = AddContentPage(page);
				item.Items[0].Items[0].Title = title ?? page.Title;
				return;
			}

			Items[0].Items[0].Items.Add(new ShellContent()
			{
				Title = title ?? page.Title,
				Content = page
			});
		}

		public ContentPage AddBottomTab(string title)
		{

			ContentPage page = new ContentPage();
			Items[0].Items.Add(new ShellSection()
			{
				Items =
				{
					new ShellContent()
					{
						Content = page,
						Title = title
					}
				}
			});
			return page;
		}

		public TabBar CreateTabBar(string shellItemTitle)
		{
			shellItemTitle = shellItemTitle ?? $"Item: {Items.Count}";
			ContentPage page = new ContentPage();
			TabBar item = new TabBar()
			{
				Title = shellItemTitle,
				Items =
				{
					new ShellSection()
					{
						Items =
						{
							new ShellContent()
							{
								Content = page
							}
						}
					}
				}
			};

			Items.Add(item);
			return item;
		}

		public ContentPage CreateContentPage(string shellItemTitle = null) 
			=> CreateContentPage<ShellItem, ShellSection>(shellItemTitle);

		public ContentPage CreateContentPage<TShellItem, TShellSection>(string shellItemTitle = null)
			where TShellItem : ShellItem
			where TShellSection : ShellSection
		{
			shellItemTitle = shellItemTitle ?? $"Item: {Items.Count}";
			ContentPage page = new ContentPage();

			TShellItem item = Activator.CreateInstance<TShellItem>();
			item.Title = shellItemTitle;

			TShellSection shellSection = Activator.CreateInstance<TShellSection>();

			shellSection.Items.Add(new ShellContent()
			{
				Content = page
			});

			item.Items.Add(shellSection);

			Items.Add(item);
			return page;
		}

		public ShellItem AddContentPage(ContentPage contentPage = null)
			=> AddContentPage<ShellItem, ShellSection>(contentPage);

		public TShellItem AddContentPage<TShellItem, TShellSection>(ContentPage contentPage = null)
			where TShellItem : ShellItem
			where TShellSection : ShellSection
		{
			contentPage = contentPage ?? new ContentPage();
			TShellItem item = Activator.CreateInstance<TShellItem>();
			item.Title = contentPage.Title;
			TShellSection shellSection = Activator.CreateInstance<TShellSection>();
			Items.Add(item);
			item.Items.Add(shellSection);

			shellSection.Items.Add(new ShellContent()
			{
				Content = contentPage
			});

			return item;
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			if (Isolate)
			{
				AppSetup.BeginIsolate();
			}
			else
			{
				AppSetup.EnsureMemory();
				AppSetup.EnsureConnection();
			}

			AppSetup.NavigateToIssue(GetType(), RunningApp);
		}

		[TearDown]
		public virtual void TearDown()
		{
			if (Isolate)
			{
				AppSetup.EndIsolate();
			}
		}
		public void ShowFlyout(string flyoutIcon = FlyoutIconAutomationId, bool usingSwipe = false, bool testForFlyoutIcon = true)
		{			
			if(testForFlyoutIcon)
				RunningApp.WaitForElement(flyoutIcon);

			if (usingSwipe)
			{
				var rect = RunningApp.ScreenBounds();
				RunningApp.DragCoordinates(10, rect.CenterY, rect.CenterX, rect.CenterY);
			}
			else
			{
				RunningApp.Tap(flyoutIcon);
			}
		}


		public void TapInFlyout(string text, string flyoutIcon = FlyoutIconAutomationId, bool usingSwipe = false)
		{
			ShowFlyout(flyoutIcon, usingSwipe);
			RunningApp.WaitForElement(text);
			RunningApp.Tap(text);
		}

#endif

		protected abstract void Init();

	}
}

#if UITEST
namespace Xamarin.Forms.Controls.Issues
{
	using System;
	using NUnit.Framework;

	// Run setup once for all tests in the Xamarin.Forms.Controls.Issues namespace
	// (instead of once for each test)
	[SetUpFixture]
	public class IssuesSetup
	{
		[SetUp]
		public void RunBeforeAnyTests()
		{
			AppSetup.RunningApp = AppSetup.Setup(null);
		}
	}
}
#endif
