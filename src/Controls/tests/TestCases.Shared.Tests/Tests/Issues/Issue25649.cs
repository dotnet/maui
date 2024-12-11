#if !MACCATALYST

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25649 : _IssuesUITest
	{
		public Issue25649(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView OnCollectionViewScrolled Calls and parameters are inconsistent or incorrect";


#if IOS
		private const string firstVisibleIndex = "5";
#elif ANDROID
		private const string firstVisibleIndex = "6";
#elif WINDOWS
		private const string firstVisibleIndex = "14";
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue25649Test()
		{
			App.WaitForElement("collectionView");
			App.ScrollDown("collectionView", ScrollStrategy.Gesture, 0.99);
			var firstVisibleItemIndex = App.FindElement("FirstVisibleItemIndex").GetText();
			Assert.That(firstVisibleItemIndex, Is.EqualTo(firstVisibleIndex));
			var lastVisibleItemIndex = App.FindElement("LastVisibleItemIndex").GetText();
			Assert.That(lastVisibleItemIndex, Is.EqualTo("30"));
		}
	}
}
#endif