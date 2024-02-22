using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Dispatching;
using NUnit.Framework.Interfaces;
using IOPath = System.IO.Path;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
using Xamarin.UITest;

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
[assembly: Category("Issues")]
#endif

namespace Microsoft.Maui.Controls.ControlGallery
{
#if UITEST
	using IApp = Xamarin.UITest.IApp;
#endif
	internal static class AppPaths
	{
		public static string ApkPath
		{
			get
			{
				var directories = Directory.GetDirectories("../../../../../src/Android/bin/Debug/", "*android*");

				if (directories.Length == 0)
					throw new Exception("Unable to locate apk path");

				return $"{directories[0]}/com.microsoft.mauicompatibilitygallery-Signed.apk";
			}
		}

		public static string iOSPath
		{
			get
			{
				var directories = Directory.GetDirectories("../../../../../src/iOS/bin/Debug/", "*ios*");

				if (directories.Length == 0)
					throw new Exception("Unable to locate ios path");

				return $"{directories[0]}/iossimulator-x64/Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.app";
			}
		}

		public static string MacOSPath = "../../../../src/MacOS/bin/Debug/Microsoft.Maui.Controls.ControlGallery.MacOS.app";

		// Have to continue using the old BundleId for now; Test Cloud doesn't like
		// when you change the BundleId
		public static string BundleId = "com.microsoft.mauicompatibilitygallery";

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

#elif WINDOWS
			app = InitializeUWPApp();
#endif
			if (app == null)
				throw new NullReferenceException("App was not initialized.");

			Application.Current ??= new TestApp(app);

			// Wrap the app in ScreenshotConditional so it only takes screenshots if the SCREENSHOTS symbol is specified
			return new ScreenshotConditionalApp(app);
		}

#if __ANDROID__
		static IApp InitializeAndroidApp()
		{
			var envApkPath = Environment.GetEnvironmentVariable("APP_APK");


			var fullApkPath = string.IsNullOrEmpty(envApkPath) ? IOPath.Combine(TestContext.CurrentContext.TestDirectory, AppPaths.ApkPath)
																: envApkPath;

			System.Diagnostics.Debug.WriteLine($"FullAppPath: {fullApkPath}");
			var appConfiguration = ConfigureApp.Android.ApkFile(fullApkPath).Debug();

			if (TestContext.Parameters.Exists("IncludeScreenShots") &&
				Convert.ToBoolean(TestContext.Parameters["IncludeScreenShots"]))
			{
				appConfiguration = appConfiguration.EnableLocalScreenshots();
			}

			var app = appConfiguration.StartApp(Xamarin.UITest.Configuration.AppDataMode.DoNotClear);

			return app;
		}

#endif

#if __IOS__
		static IApp InitializeiOSApp()
		{
			/*
			string UDID = "";

			if (TestContext.Parameters.Exists("UDID"))
			{
				UDID = TestContext.Parameters["UDID"];
			}

			// Running on a device
			var appConfiguration = ConfigureApp.iOS.InstalledApp(AppPaths.BundleId).Debug();

			if(!String.IsNullOrWhiteSpace(UDID))
			{
				appConfiguration = appConfiguration.DeviceIdentifier(UDID);
			}

			if (TestContext.Parameters.Exists("IncludeScreenShots") &&
				Convert.ToBoolean(TestContext.Parameters["IncludeScreenShots"]))
			{
				appConfiguration = appConfiguration.EnableLocalScreenshots();
			}

			var app = appConfiguration.StartApp(Xamarin.UITest.Configuration.AppDataMode.DoNotClear);

			int _iosVersion;
			if (int.TryParse(app.Invoke("iOSVersion").ToString(), out _iosVersion))
			{
				iOSVersion = _iosVersion;
			}*/


			var enviOSPath = Environment.GetEnvironmentVariable("iOS_APP");
			var UDID = Environment.GetEnvironmentVariable("DEVICE_UDID");

			var fullApkPath = string.IsNullOrEmpty(enviOSPath) ? IOPath.Combine(TestContext.CurrentContext.TestDirectory, AppPaths.iOSPath)
																: enviOSPath;

			// Running on the simulator
			var appConfiguration = ConfigureApp.iOS
							.PreferIdeSettings()
							.AppBundle(fullApkPath)
							.Debug();

			if (!string.IsNullOrWhiteSpace(UDID))
			{
				appConfiguration = appConfiguration.DeviceIdentifier(UDID);
			}
							
			var app = appConfiguration.StartApp();

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
							//.AppBundle("/Users/rmarinho/Xamarin/Xamarin.Forms/Microsoft.Maui.Controls.ControlGallery.MacOS/bin/Debug/Microsoft.Maui.Controls.ControlGallery.MacOS.app")
							.AppBundle(AppPaths.MacOSPath)
							.BundleId(AppPaths.MacOSBundleId)
							.StartApp();
			return new Microsoft.Maui.Controls.macOS.UITests.MacOSApp(app);
		}
#endif

#if WINDOWS
		static IApp InitializeUWPApp()
		{
			return WindowsTestBase.ConfigureApp();
		}
#endif

		public static void NavigateToIssue(Type type, IApp app)
		{
			var typeIssueAttribute = type.GetCustomAttribute<IssueAttribute>();

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

#if WINDOWS
			bool attemptOneRestart = false;
			bool waitNoElementAttempt = false;
#endif
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

#if WINDOWS
					// Windows doens't have an 'invoke' option right now for us to do the more direct navigation
					// we're using for Android/iOS
					// So we're just going to use the 'Reset' method to bounce the app to the opening screen
					// and then fall back to the old manual navigation
					WindowsTestBase.Reset();
					app.RestartIfAppIsClosed();
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

					app.EnterText(q => q.Raw("* marked:'SearchBarGo'"), $"{cellName}");

					app.WaitForElement(q => q.Raw("* marked:'SearchButton'"));
					app.Tap(q => q.Raw("* marked:'SearchButton'"));

#if WINDOWS
					try
					{
						if (!waitNoElementAttempt)
						{
							waitNoElementAttempt = true;
							app.WaitForNoElement(q => q.Raw("* marked:'TestCasesIssueList'"), timeout: TimeSpan.FromMinutes(1));
						}
					}
					catch
					{
						app.Restart();
						attempts--;
						throw;
					}
#endif

					if (!app.RestartIfAppIsClosed())
						return;
				}
#if WINDOWS
				catch (Exception we)
				when (we.IsWindowClosedException() && !attemptOneRestart)
				{
					attemptOneRestart = true;
					attempts--;
					app.RestartIfAppIsClosed();
				}
#endif
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
				// Issue 7207 - if current culture of the current thread is not set to the invariant culture
				// then initializing the app causes a "NUnit.Framework.InconclusiveException" with the exception-
				// message "App did not start for some reason. System.Argument.Exception: 1 is not supported code page.
				// Parameter name: codepage."
				if (System.Threading.Thread.CurrentThread.CurrentCulture != System.Globalization.CultureInfo.InvariantCulture)
					System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

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
			(RunningApp as ScreenshotConditionalApp).TestSetup(GetType(), Isolate);
		}

		[TearDown]
		public void TearDown()
		{
			(RunningApp as ScreenshotConditionalApp).TestTearDown(Isolate);
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
			(RunningApp as ScreenshotConditionalApp).TestSetup(GetType(), Isolate);
		}

		[TearDown]
		public virtual void TearDown()
		{
			(RunningApp as ScreenshotConditionalApp).TestTearDown(Isolate);
		}
#endif

		protected abstract void Init();
	}

#if UITEST
	[Category(Compatibility.UITests.UITestCategories.FlyoutPage)]
#endif
	public abstract class TestFlyoutPage : FlyoutPage
	{
#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;

		protected virtual bool Isolate => false;
#endif

		protected TestFlyoutPage()
		{
#if APP
			Init();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			(RunningApp as ScreenshotConditionalApp).TestSetup(GetType(), Isolate);
		}

		[TearDown]
		public virtual void TearDown()
		{
			(RunningApp as ScreenshotConditionalApp).TestTearDown(Isolate);
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
			(RunningApp as ScreenshotConditionalApp).TestSetup(GetType(), Isolate);
		}

		[TearDown]
		public void TearDown()
		{
			(RunningApp as ScreenshotConditionalApp).TestTearDown(Isolate);
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
			(RunningApp as ScreenshotConditionalApp).TestSetup(GetType(), Isolate);
		}

		[TearDown]
		public virtual void TearDown()
		{
			(RunningApp as ScreenshotConditionalApp).TestTearDown(Isolate);
		}
#endif

		protected abstract void Init();
	}



#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public abstract class TestShell : Shell
	{
#if ANDROID
		protected const string FlyoutIconAutomationId = "Open navigation drawer";
#else
		protected const string FlyoutIconAutomationId = "OK";
#endif
#if __IOS__ || WINDOWS
		protected const string BackButtonAutomationId = "Back";
#else
		protected const string BackButtonAutomationId = "Navigate up";
#endif

#if UITEST
		public IApp RunningApp => AppSetup.RunningApp;
		protected virtual bool Isolate => true;
#endif

		protected void IncreaseFlyoutItemsHeightSoUITestsCanClickOnThem()
		{
			this.Resources.Add(new Style(typeof(Label))
			{
				ApplyToDerivedTypes = true,
				Class = FlyoutItem.LayoutStyle,
				Setters =
				{
					new Setter()
					{
						Property = HeightRequestProperty,
						Value = 50
					}
				}
			});
		}

		protected TestShell() : base()
		{
			Routing.Clear();
#if APP
			Init();
#endif
		}

		protected ContentPage DisplayedPage
		{
			get
			{
				return (ContentPage)(CurrentItem.CurrentItem as IShellSectionController).PresentedPage;
			}
		}

		public ContentPage AddTopTab(string title, string icon = null, ShellSection root = null)
		{
			var page = new ContentPage()
			{
				Title = title
			};

			AddTopTab(page, title, icon, root);
			return page;
		}


		public void AddTopTab(ContentPage page, string title = null, string icon = null, ShellSection root = null)
		{
			if (Items.Count == 0)
			{
				var item = AddContentPage(page);
				item.Items[0].Items[0].Title = title ?? page.Title;
				return;
			}

			root = Items[0].Items[0];
			title = title ?? page.Title;
			var content = new ShellContent()
			{
				Title = title,
				Content = page,
				Icon = icon,
				AutomationId = title
			};

			root.Items.Add(content);

			if (!String.IsNullOrWhiteSpace(content.Title))
				content.Route = content.Title;
		}

		public ContentPage AddBottomTab(ContentPage page, string title, string icon = null)
		{
			if (Items.Count == 0)
			{
				var item = AddContentPage(page);
				item.Items[0].Items[0].Title = title ?? page.Title;
				item.Items[0].Title = title ?? page.Title;
				return page;
			}

			Items[0].Items.Add(new ShellSection()
			{
				AutomationId = title,
				Route = title,
				Title = title,
				Icon = icon,
				Items =
 				{
 					new ShellContent()
 					{
 						ContentTemplate = new DataTemplate(() => page),
 						Title = title
 					}
 				}
			});
			return page;
		}

		public ContentPage AddBottomTab(string title, string icon = null)
		{
			ContentPage page = new ContentPage();
			if (Items.Count == 0)
			{
				var item = AddContentPage(page, title);
				item.Items[0].Items[0].Title = title ?? page.Title;
				item.Items[0].Title = title ?? page.Title;
				return page;
			}

			Items[0].Items.Add(new ShellSection()
			{
				AutomationId = title,
				Route = title,
				Title = title,
				Icon = icon,
				Items =
				{
					new ShellContent()
					{
						ContentTemplate = new DataTemplate(() => page),
						Title = title
					}
				}
			});
			return page;
		}

		public ContentPage CreateContentPage<TShellItem>(string title)
			where TShellItem : ShellItem
		{
			ContentPage page = new ContentPage() { Title = title };
			AddContentPage<TShellItem, Tab>(page, title);
			return page;
		}

		public FlyoutItem AddFlyoutItem(string title)
		{
			return AddContentPage<FlyoutItem, Tab>(new ContentPage(), title);
		}

		public FlyoutItem AddFlyoutItem(ContentPage page, string title)
		{
			return AddContentPage<FlyoutItem, Tab>(page, title);
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
			shellSection.Title = shellItemTitle;

			shellSection.Items.Add(new ShellContent()
			{
				ContentTemplate = new DataTemplate(() => page)
			});

			item.Items.Add(shellSection);

			Items.Add(item);
			return page;
		}

		public ShellItem AddContentPage(ContentPage contentPage = null, string title = null)
			=> AddContentPage<ShellItem, ShellSection>(contentPage, title);

		public TShellItem AddContentPage<TShellItem, TShellSection>(ContentPage contentPage = null, string title = null)
			where TShellItem : ShellItem
			where TShellSection : ShellSection
		{
			title = title ?? contentPage?.Title;
			contentPage = contentPage ?? new ContentPage();
			TShellItem item = Activator.CreateInstance<TShellItem>();
			item.Title = title;
			TShellSection shellSection = Activator.CreateInstance<TShellSection>();
			Items.Add(item);
			item.Items.Add(shellSection);
			shellSection.Title = title;

			var content = new ShellContent()
			{
				ContentTemplate = new DataTemplate(() => contentPage),
				Title = title
			};

			shellSection.Items.Add(content);

			if (!String.IsNullOrWhiteSpace(title))
			{
				content.Route = title;
			}

			return item;
		}

#if UITEST
		[SetUp]
		public void Setup()
		{
			(RunningApp as ScreenshotConditionalApp).TestSetup(GetType(), Isolate);
		}

		[TearDown]
		public virtual void TearDown()
		{
			(RunningApp as ScreenshotConditionalApp).TestTearDown(Isolate);
		}

		public void ShowFlyout(string flyoutIcon = FlyoutIconAutomationId, bool usingSwipe = false, bool testForFlyoutIcon = true)
		{
			if (testForFlyoutIcon)
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

		public void TapBackArrow(string backArrowIcon = BackButtonAutomationId)
		{
			RunningApp.WaitForElement(backArrowIcon, "Back Arrow Not Found");
			RunningApp.Tap(backArrowIcon);
		}


		public void TapInFlyout(string text, string flyoutIcon = FlyoutIconAutomationId, bool usingSwipe = false, string timeoutMessage = null, bool makeSureFlyoutStaysOpen = false)
		{
			timeoutMessage = timeoutMessage ?? text;
#if WINDOWS
			RunningApp.WaitForElement(flyoutIcon);
#endif

			System.Threading.Thread.Sleep(500);
			CheckIfOpen();
			try
			{
				RunningApp.Tap(text);
			}
			catch
			{
				// Give it one more try
				CheckIfOpen();
				RunningApp.Tap(text);
			}

			if (makeSureFlyoutStaysOpen)
			{
				System.Threading.Thread.Sleep(500);
				if (RunningApp.Query(text).Count() == 0)
					this.ShowFlyout(flyoutIcon);
			}

			void CheckIfOpen()
			{
				if (RunningApp.Query(text).Count() == 0)
				{
					ShowFlyout(flyoutIcon, usingSwipe);
					RunningApp.WaitForElement(text, timeoutMessage);
				}
			}
		}

		public void DoubleTapInFlyout(string text, string flyoutIcon = FlyoutIconAutomationId, bool usingSwipe = false, string timeoutMessage = null)
		{
			timeoutMessage = timeoutMessage ?? text;
			ShowFlyout(flyoutIcon, usingSwipe);
			RunningApp.WaitForElement(text, timeoutMessage);
			RunningApp.DoubleTap(text);
		}

#endif

		protected abstract void Init();

	}

#if UITEST
	/// <summary>
	/// All of our tests are nested inside Maui.Controls Types and a few of those types need a 
	/// dispatcher to be set inside the ctor. 
	/// So, when the NUnit runner instantiates a `TabbedPage` as the base class for the `TestFixture`
	/// it'll throw an exception because it can't find a Dispatcher for the BindableObject.
	/// That dispatcher will never actually be used but this will at least fill in the requirements.
	/// Nunit doesn't actually used the features of a `TabbedPage` for anything that's just where
	/// the Xamarin.UITest commands live for running a test.
	/// </summary>
	class TestApp : Application
	{
		public TestApp(IApp app)
		{
			Handler = new TestAppHandler(this, app);
		}

		class TestAppHandler : IElementHandler
		{
			TestApp _testApp;
			IApp _app;
			IMauiContext _mauiContext;

			public TestAppHandler(TestApp testApp, IApp app)
			{
				_app = app;
				_testApp = testApp;
				_mauiContext = new ContextStub();
			}

			public object PlatformView => _app;

			public IElement VirtualView => _testApp;

			public IMauiContext MauiContext => _mauiContext;

			public void DisconnectHandler()
			{
			}

			public void Invoke(string command, object args = null)
			{
			}

			public void SetMauiContext(IMauiContext mauiContext)
			{
			}

			public void SetVirtualView(IElement view)
			{
			}

			public void UpdateValue(string property)
			{
			}
		}

		class ContextStub : IMauiContext, IServiceProvider
		{
			public IDispatcher _dispatcher = new TestDispatcher();

			public IServiceProvider Services => this;

			public IMauiHandlersFactory Handlers => throw new NotImplementedException();

			public object GetService(Type serviceType)
			{
				if (serviceType == typeof(IDispatcher))
					return _dispatcher;

				if (serviceType == typeof(ApplicationDispatcher))
					return new ApplicationDispatcher(_dispatcher);

				throw new NotImplementedException();
			}

			class TestDispatcher : IDispatcher
			{
				public bool IsDispatchRequired => false;

				public IDispatcherTimer CreateTimer()
				{
					throw new NotImplementedException();
				}

				public bool Dispatch(Action action)
				{
					action.Invoke();
					return true;
				}

				public bool DispatchDelayed(TimeSpan delay, Action action)
				{
					throw new NotImplementedException();
				}
			}
		}
	}

#endif
}

#if UITEST
namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	using System;
	using NUnit.Framework;

	// Run setup once for all tests in the Microsoft.Maui.Controls.ControlGallery.Issues namespace
	// (instead of once for each test)
	[SetUpFixture]
	public class IssuesSetup
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			AppSetup.RunningApp = AppSetup.Setup(null);
		}
	}
}
#endif
