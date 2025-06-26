using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6963 : _IssuesUITest
	{
		public Issue6963(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView multiple pre-selection throws ArgumentOutOfRangeException when SelectedItems is bound to an ObservableCollection initialized inside the constructor.";

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void SelectedItemsNotInSourceDoesNotCrash()
		{
			// If this page didn't crash, then we're good
			App.WaitForElement("FirstLabel");
		}
	}
}
