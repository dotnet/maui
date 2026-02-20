using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28321 : _IssuesUITest
	{
		public Issue28321(TestDevice device) : base(device) { }

		public override string Issue => "CV RemainingItemsThresholdReachedCommand fires on initial data load";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void RemainingItemsThresholdReachedCommandShouldNotFireOnInitialDataLoad()
		{
			App.WaitForElement("LoadedItemsLabel");
			App.WaitForElement("Item3");
			var loadedItemsText = App.FindElement("LoadedItemsLabel").GetText();
			Assert.That("Loaded items: 4", Is.EqualTo(loadedItemsText));
		}

	}
}