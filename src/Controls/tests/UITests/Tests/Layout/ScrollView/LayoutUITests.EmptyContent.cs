using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class ScrollViewEmptyContentUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public ScrollViewEmptyContentUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(LayoutGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[TearDown]
		public void LayoutUITestTearDown()
		{
			this.Back();
		}

		[Test]
		[Description("No crash measuring empty ScrollView")]
		public void EmptyScrollView()
		{
			App.Click("EmptyScrollView");
			App.WaitForElement("TestScrollView");

			Task.Delay(1000).Wait();
			App.WaitForElement("Foo");
		}
	}
}