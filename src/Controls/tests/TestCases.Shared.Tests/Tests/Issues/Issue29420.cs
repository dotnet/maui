#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // CarouselView Fails to Keep Last Item in View on iOS and macOS https://github.com/dotnet/maui/issues/18029
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29420 : _IssuesUITest
{
	public override string Issue => "KeepLastInView Not Working as Expected in CarouselView";

	public Issue29420(TestDevice device) : base(device)
	{ }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewKeepLastInViewOnItemAdd()
	{
		App.WaitForElement("CarouselView");
		for (var i = 0; i < 5; i++)
		{
			App.Tap("InsertButton");
		}
		
		App.WaitForElement("Item 5");
		App.Tap("AddButton");
		App.WaitForElement("NewItem");
	}
}
#endif