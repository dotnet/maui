using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7673 : _IssuesUITest
{
	public override string Issue => "Android Switch view has inconsistent colors when off";

	public Issue21368(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Switch)]
	public void VerifyOffTrackColor()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}

	public void VerifyOnTrackColor()
	{
		App.Click("TogglingSwitch");
		VerifyScreenshot();
	}

	public void VerifyTogglingOnOffColor()
	{
		App.Click("TogglingSwitch");
		VerifyScreenshot();
	}
}