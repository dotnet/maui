#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // Issue Link for Windows: https://github.com/dotnet/maui/issues/16767 and for Android: https://github.com/dotnet/maui/issues/30576
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21886 : _IssuesUITest
{
	public Issue21886(TestDevice device) : base(device)
	{
	}

	public override string Issue => "The original image remains undisposed even after setting disposeOriginal to true in the Resize and Downsize methods";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void VerifyOriginalImageBeingDisposed()
	{
		App.WaitForElement("OriginalImageStatusLabel");
		App.Tap("Issue21886ResizeBtn");

		var resizeLabelText = App.FindElement("OriginalImageStatusLabel").GetText();
		Assert.That(resizeLabelText, Is.EqualTo("Success"));

		App.Tap("Issue21886DownSizeBtn");

		var downsizeLabelText = App.FindElement("OriginalImageStatusLabel").GetText();
		Assert.That(downsizeLabelText, Is.EqualTo("Success"));
	}
}
#endif