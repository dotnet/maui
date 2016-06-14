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

	 	public static IApp App { get; private set; }
		public string PlatformViewType { get; protected set; }
		public bool ShouldResetPerFixture { get; protected set; }
		public AppRect ScreenBounds { get; private set; }

		protected BaseTestFixture ()
		{
			ShouldResetPerFixture = true;
		}

		protected abstract void NavigateToGallery ();

#pragma warning disable 618
		[TestFixtureSetUp]
#pragma warning restore 618
		protected virtual void FixtureSetup ()
		{
			try
			{
				if (ShouldResetPerFixture)
				{
					RelaunchApp();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				throw;
			}
		}

#pragma warning disable 618
		[TestFixtureTearDown]
#pragma warning restore 618
		protected virtual void FixtureTeardown ()
		{	
		}

		[SetUp]
		protected virtual void TestSetup () 
		{
			if (!ShouldResetPerFixture) {
				RelaunchApp ();
			}
			App.Screenshot ("Begin Test");
		}

		[TearDown]
		protected virtual void TestTearDown ()
		{
			App.Screenshot ("Test complete");
		}

		void RelaunchApp ()
		{
			App = null;
			RunningApp.App = null;

			try {
				RunningApp.Restart ();
			} catch (Exception ex) {
				// if at first you dont succeed
				RunningApp.Restart ();
			}

			// Wrap the app in ScreenshotConditional so it only takes screenshots if the SCREENSHOTS symbol is specified
			App = new ScreenshotConditionalApp(RunningApp.App);

			App.SetOrientationPortrait ();
			ScreenBounds = App.RootViewRect ();
			NavigateToGallery ();
		}
	}
}
