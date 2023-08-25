using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16320 : _IssuesUITest
	{
		public Issue16320(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Adding an item to a CollectionView with linear layout crashes";

		[Test]
		public void Issue16320Test()
		{
			App.Tap("Add");

			Assert.NotNull(App.WaitForElement("item: 1"));
		}
	}
}
