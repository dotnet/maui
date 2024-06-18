using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue6963 : IssuesUITest
	{
		public Issue6963(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView multiple pre-selection throws ArgumentOutOfRangeException when SelectedItems is bound to an ObservableCollection initialized inside the constructor.";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void SelectedItemsNotInSourceDoesNotCrash()
		{      
			// If this page didn't crash, then we're good
			RunningApp.WaitForElement("FirstLabel");
		}
	}
}
