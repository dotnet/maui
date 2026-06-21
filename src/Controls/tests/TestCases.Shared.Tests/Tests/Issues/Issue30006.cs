#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // Issue Link for Windows: https://github.com/dotnet/maui/issues/16767 and for Android: https://github.com/dotnet/maui/issues/30576
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30006 : _IssuesUITest
{
	public Issue30006(TestDevice device) : base(device)
	{
	}

	public override string Issue => "The downsized image continues to retain its original dimensions";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void VerifyDownsizedImageHasNewDimensions()
	{
		App.WaitForElement("ConvertedImage");
		App.Tap("Issue30006DownSizeBtn");

		var downsizedLabelText = App.FindElement("ConvertedImage").GetText();
		Assert.That(downsizedLabelText, Is.EqualTo("Success"));
	}
}
#endif