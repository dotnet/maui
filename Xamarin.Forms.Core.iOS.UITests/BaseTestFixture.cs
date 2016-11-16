using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Xamarin.Forms.Controls;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract class BaseTestFixture
	{
		// TODO: Landscape tests

		public static IApp App { get; set; }
		public string PlatformViewType { get; protected set; }
		public static AppRect ScreenBounds { get; set; }

		[TestFixtureTearDown]
		protected virtual void FixtureTeardown()
		{
		}

		static int s_testsrun;
		const int ConsecutiveTestLimit = 40;

		// Until we get more of our memory leak issues worked out, restart the app 
		// after a specified number of tests so we don't get bogged down in GC
		public void EnsureMemory()
		{
			s_testsrun += 1;

			if (s_testsrun >= ConsecutiveTestLimit)
			{
				s_testsrun = 0;

				CoreUITestsSetup.LaunchApp();

				FixtureSetup();
			}
		}

		[SetUp]
		protected virtual void TestSetup()
		{
			EnsureMemory();
		}

		[TearDown]
		protected virtual void TestTearDown()
		{
		}

		protected abstract void NavigateToGallery();

#pragma warning disable 618
		[TestFixtureSetUp]
#pragma warning restore 618
		protected virtual void FixtureSetup()
		{
			ResetApp();
			NavigateToGallery();
		}

		protected void ResetApp()
		{
#if __IOS__
			App.Invoke("reset:", string.Empty);
#endif
#if __ANDROID__
			App.Invoke("Reset");
#endif
		}
	}
}

#if UITEST
namespace Xamarin.Forms.Core.UITests
{
	using NUnit.Framework;

	[SetUpFixture]
	public class CoreUITestsSetup
	{
		[SetUp]
		public void RunBeforeAnyTests()
		{
			LaunchApp();
		}

		public static void LaunchApp()
		{
			BaseTestFixture.App = null;
			BaseTestFixture.App = AppSetup.Setup();

			BaseTestFixture.App.SetOrientationPortrait();
			BaseTestFixture.ScreenBounds = BaseTestFixture.App.RootViewRect();
		}
	}
}
#endif
