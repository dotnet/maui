#if !MACCATALYST || !WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25433 : _IssuesUITest
	{
		public override string Issue => "Collection view with horizontal grid layout has extra space on right end";

		public Issue25433(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewHorizontalItemSpacing()
		{
			App.WaitForElement("collectionView");

			VerifyScreenshot();
		}
	}
}
#endif
