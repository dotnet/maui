using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla44166 : _IssuesUITest
{

	public Bugzilla44166(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "FlyoutPage instances do not get disposed upon GC";

	[Test]
#if ANDROID
	[Ignore("Failing on net10 https://github.com/dotnet/maui/issues/27411")]
#endif
	[Category(UITestCategories.Performance)]
	public void Bugzilla44166Test()
	{
		App.WaitForElement("Go");
		App.Tap("Go");

		App.WaitForElement("Previous");
		App.Tap("Previous");

		App.WaitForElement("GC");

		for (var n = 0; n < 10; n++)
		{
			App.Tap("GC");

			if (App.WaitForElement("Result").GetText() == "Success")
			{
				return;
			}
		}

		// The previous content retrieves the value of the page that was still allocated,
		// which is only used for displaying in the error message.
		// We can ignore this in Appium tests since we can't access those properties from the sample.
		Assert.Fail($"At least one of the pages was not collected");
	}
}