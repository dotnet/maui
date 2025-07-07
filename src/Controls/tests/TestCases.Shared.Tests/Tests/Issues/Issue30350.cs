#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30465 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30350 : _IssuesUITest
{
	public Issue30350(TestDevice device) : base(device)
	{
	}

	public override string Issue => "When an image is downsized the resulting image appears upside down";

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyDownsizedImageIsNotFlipped()
	{
		App.WaitForElement("Issue30350_DownsizedImageLabel");
		VerifyScreenshot();
	}
}
#endif