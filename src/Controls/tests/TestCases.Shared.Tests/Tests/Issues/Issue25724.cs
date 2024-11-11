#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue25724 : _IssuesUITest
	{
		public Issue25724(TestDevice device) : base(device) { }

		public override string Issue => "ObjectDisposedException When Toggling Header/Footer in CollectionView Dynamically";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewDynamicHeaderShouldNotCrashOnDisplay()
		{
			App.WaitForElement("This Is A Header");
            App.Tap("ToggleHeaderButton"); // This is the button that toggles the header to null
            App.Tap("ToggleHeaderButton");// This is the button that toggles the header back to the original value
            App.WaitForElement("This Is A Header");
		}
	}
}
#endif