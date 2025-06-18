#if TEST_FAILS_ON_WINDOWS //Maps control is not supported on Windows platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla26171 : _IssuesUITest
{
	public Bugzilla26171(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Microsoft.Maui.Controls.Maps is not updating VisibleRegion property when layout is changed";

	[Test]
	[Category(UITestCategories.Maps)]
	public void Bugzilla26171Test()
	{
		App.WaitForElement("lblValue");
	}
}
#endif