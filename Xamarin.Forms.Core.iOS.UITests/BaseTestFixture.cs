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

		protected BaseTestFixture()
		{
			ShouldResetPerFixture = true;
		}

		protected abstract void NavigateToGallery();

#pragma warning disable 618
		[TestFixtureSetUp]
#pragma warning restore 618
		protected virtual void FixtureSetup()
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
		protected virtual void FixtureTeardown()
		{
		}

		[SetUp]
		protected virtual void TestSetup()
		{
			if (!ShouldResetPerFixture)
			{

				RelaunchApp();
			}
		}

		[TearDown]
		protected virtual void TestTearDown()
		{

		}

		void RelaunchApp()
		{
			App = null;
			App = AppSetup.Setup();
			App.SetOrientationPortrait();
			ScreenBounds = App.RootViewRect();
			NavigateToGallery();
		}
	}
}
