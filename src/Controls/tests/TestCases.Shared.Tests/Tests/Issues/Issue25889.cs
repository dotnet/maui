using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25889 : _IssuesUITest
	{
		public Issue25889(TestDevice device) : base(device)
		{
		}

		public override string Issue => "CollectionView RemainingItemsThresholdReachedCommand not Firing";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void RemainingItemsThresholdReachedCommandFired()
		{
			App.WaitForElement("collectionView");
			App.ScrollDown("collectionView", ScrollStrategy.Gesture, 0.99);
			// In CI, the CommandFired text does not update when using the ScrollDown action. To reliably trigger the command, first scroll up and then scroll down again.
			App.ScrollUp("collectionView", ScrollStrategy.Gesture, swipePercentage: 0.99);
			App.WaitForElement("collectionView");
			App.ScrollDown("collectionView", ScrollStrategy.Gesture, 0.99);

			var label = App.WaitForElement("mainPageLabel");
			Assert.That(label.GetText(), Is.EqualTo("Command Fired!"));
		}
	}
}
