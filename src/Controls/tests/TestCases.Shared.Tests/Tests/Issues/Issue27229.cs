#if TEST_FAILS_ON_ANDROID
// This test started failing on the safe area edges changes because the safe area edges changes
// cause a second measure pass which exposes a bug that already existed in CollectionView on Android.
// You can replicate this bug on NET10 by rotating the device and rotating back, and then you will see that the
// footer will disappear because on the second measure pass the layout of the content is too big.


using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue27229 : _IssuesUITest
	{
		public Issue27229(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView, EmptyView Fills Available Space By Default";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewEmptyViewFillsAvailableSpaceByDefault()
		{
			App.WaitForElement("ReadyToTest");
			VerifyScreenshot();
		}
	}
}
#endif