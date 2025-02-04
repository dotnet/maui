#if !MACCATALYST
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
			App.ScrollDown("collectionView", ScrollStrategy.Gesture);

			App.WaitForElement("collectionView");
			App.ScrollDown("collectionView", ScrollStrategy.Gesture);

			var label = App.WaitForElement("mainPageLabel");
			Assert.That(label.GetText(), Is.EqualTo("Command Fired!"));
		}
	}
}
#endif