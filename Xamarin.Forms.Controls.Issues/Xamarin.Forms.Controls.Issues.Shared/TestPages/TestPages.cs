using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Xamarin.Forms.CustomAttributes;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;

#endif

namespace Xamarin.Forms.Controls
{
	internal static class AppPaths
    {
        public static string ApkPath = "../../../Xamarin.Forms.ControlGallery.Android/bin/Debug/AndroidControlGallery.AndroidControlGallery-Signed.apk";

		// Have to continue using the old BundleId for now; Test Cloud doesn't like
		// when you change the BundleId
        public static string BundleId = "com.xamarin.quickui.controlgallery";
    }

#if UITEST
	internal static class AppSetup
	{
		static IApp InitializeApp ()
		{
			IApp app = null;
#if __ANDROID__
			app = ConfigureApp.Android.ApkFile (AppPaths.ApkPath).Debug ().StartApp ();
#elif __IOS__ 
			app = ConfigureApp.iOS.InstalledApp (AppPaths.BundleId).Debug ()
				//Uncomment to run from a specific iOS SIM, get the ID from XCode -> Devices
				//.DeviceIdentifier("55555555-5555-5555-5555-555555555555")
				.StartApp ();
#endif
			if (app == null)
				throw new NullReferenceException ("App was not initialized.");

			// Wrap the app in ScreenshotConditional so it only takes screenshots if the SCREENSHOTS symbol is specified
			return new ScreenshotConditionalApp(app);
		}

		static void NavigateToIssue (Type type, IApp app)
		{
			var typeIssueAttribute = type.GetTypeInfo ().GetCustomAttribute <IssueAttribute> ();

			string cellName = "";
			if (typeIssueAttribute.IssueTracker.ToString () != "None" &&
				typeIssueAttribute.IssueNumber != 1461 &&
				typeIssueAttribute.IssueNumber != 342) {
				cellName = typeIssueAttribute.IssueTracker.ToString ().Substring(0, 1) + typeIssueAttribute.IssueNumber.ToString ();
			} else {
				cellName = typeIssueAttribute.Description;
			}

			app.Tap (q => q.Button ("Go to Test Cases"));
			app.WaitForElement (q => q.Raw ("* marked:'TestCasesIssueList'"));

			app.EnterText (q => q.Raw ("* marked:'SearchBarGo'"), cellName);

			app.WaitForElement (q => q.Raw ("* marked:'SearchButton'"));
			app.Tap (q => q.Raw ("* marked:'SearchButton'"));
		}

		public static IApp Setup (Type pageType = null)
		{
			IApp runningApp = null;
			try {
				runningApp = InitializeApp ();
			} catch (Exception e) {
				Assert.Inconclusive ($"App did not start for some reason: {e}");
			}
			
			if (pageType != null)
				NavigateToIssue (pageType, runningApp);

			return runningApp;
		}
	}
#endif

	public abstract class TestPage : Page
	{
#if UITEST
		public IApp RunningApp { get; private set; }
#endif

		protected TestPage ()
		{
#if APP
			Init ();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup ()
		{
			RunningApp = AppSetup.Setup (GetType ());
		}
#endif

		protected abstract void Init ();
	}


	public abstract class TestContentPage : ContentPage
	{
#if UITEST
		public IApp RunningApp { get; private set; }
#endif

		protected TestContentPage ()
		{
#if APP
			Init ();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup ()
		{
			RunningApp = AppSetup.Setup (GetType ());
		}
#endif

		protected abstract void Init ();
	}

	public abstract class TestCarouselPage : CarouselPage
	{
#if UITEST
		public IApp RunningApp { get; private set; }
#endif

		protected TestCarouselPage ()
		{
#if APP
			Init ();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup ()
		{
			RunningApp = AppSetup.Setup (GetType ());
		}
#endif

		protected abstract void Init ();
	}

	public abstract class TestMasterDetailPage : MasterDetailPage
	{
#if UITEST
		public IApp RunningApp { get; private set; }
#endif

		protected TestMasterDetailPage ()
		{
#if APP
			Init ();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup ()
		{
			RunningApp = AppSetup.Setup (GetType ());
		}
#endif

		protected abstract void Init ();
	}

	public abstract class TestNavigationPage : NavigationPage
	{
#if UITEST
		public IApp RunningApp { get; private set; }
#endif

		protected TestNavigationPage ()
		{
#if APP
			Init ();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup ()
		{
			RunningApp = AppSetup.Setup (GetType ());
		}
#endif

		protected abstract void Init ();
	}

	public abstract class TestTabbedPage : TabbedPage
	{
#if UITEST
		public IApp RunningApp { get; private set; }
#endif

		protected TestTabbedPage ()
		{
#if APP
			Init ();
#endif
		}

#if UITEST
		[SetUp]
		public void Setup ()
		{
			RunningApp = AppSetup.Setup (GetType ());
		}
#endif

		protected abstract void Init ();
	}
}
