using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class ScrollViewEmptyContentUITests : LayoutUITests
	{
		public ScrollViewEmptyContentUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("No crash measuring empty ScrollView")]
		public async Task EmptyScrollView()
		{
			App.Click("EmptyScrollView");
			App.WaitForElement("TestScrollView");

			await Task.Delay(1000);
			App.WaitForElement("Foo");
		}
	}
}