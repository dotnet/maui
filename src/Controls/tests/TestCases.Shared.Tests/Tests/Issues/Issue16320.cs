using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16320 : _IssuesUITest
	{
		public Issue16320(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Adding an item to a CollectionView with linear layout crashes";

		[Test]
		[Category(UITestCategories.CollectionView)]
		// TODO: It looks like this test has never passed on Android, failing with 
		// "System.TimeoutException : Timed out waiting for element". We (e.g. ema) should
		// investigate and properly fix, but we'll ignore for now.
		[FailsOnAndroid("This test is failing, likely due to product issue")]
		public void Issue16320Test()
		{
			App.Tap("Add");

			ClassicAssert.NotNull(App.WaitForElement("item: 1"));
		}
	}
}
