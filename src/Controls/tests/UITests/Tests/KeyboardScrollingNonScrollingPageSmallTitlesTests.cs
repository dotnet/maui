using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using TestUtils.Appium.UITests;
using Xamarin.UITest;

namespace Microsoft.Maui.AppiumTests
{
	public class KeyboardScrollingNonScrollingPageSmallTitlesTests : UITestBase
	{
		const string KeyboardScrollingGallery = "* marked:'Keyboard Scrolling Gallery - NonScrolling Page / Small Titles'";
		public KeyboardScrollingNonScrollingPageSmallTitlesTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			App.NavigateBack();
		}

		[Test]
		public void EntriesScrollingPageTest()
		{
			KeyboardScrolling.EntriesScrollingTest(App, UITestContext, KeyboardScrollingGallery);
		}

		[Test]
		public void EditorsScrollingPageTest()
		{
			KeyboardScrolling.EditorsScrollingTest(App, UITestContext, KeyboardScrollingGallery);
		}

		[Test]
		public void EntryNextEditorTest()
		{
			KeyboardScrolling.EntryNextEditorScrollingTest(App, UITestContext, KeyboardScrollingGallery);
		}
	}
}
