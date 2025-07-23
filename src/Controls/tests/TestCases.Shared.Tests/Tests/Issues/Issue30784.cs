using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30784 : _IssuesUITest
{
	public Issue30784(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Aspect ratio not maintained when Downsize is called with MaxWidth and MaxHeight";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void VerifyDownsizeMaintainsRatioWithMaxWidthHeight()
	{
		App.WaitForElement("Issue30784DownSizeBtn");
		App.Tap("Issue30784DownSizeBtn");

		var downsizedImageLabelText = App.WaitForElement("ConvertedImageStatusLabel").GetText();
		Assert.That(downsizedImageLabelText, Is.EqualTo("Success"));
	}
}