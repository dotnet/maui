using Xunit;
using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		public void Issue16320Test()
		{
			App.WaitForElement("Add");
			App.Tap("Add");
			Assert.That(App.WaitForElement("item: 1"), Is.Not.Null);
		}
	}
}
