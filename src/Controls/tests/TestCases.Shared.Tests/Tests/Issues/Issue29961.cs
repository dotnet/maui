using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29961 : _IssuesUITest
{
	public Issue29961(TestDevice device) : base(device)
	{
	}

	public override string Issue => "The resize method returns an image that has already been disposed";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void VerifyResizeMethodReturnsValidImage()
	{
		App.WaitForElement("ConvertedImageStatusLabel");
		App.Tap("Issue29961_ResizeBtn");

		var resizeLabelText = App.FindElement("ConvertedImageStatusLabel").GetText();
		Assert.That(resizeLabelText, Is.EqualTo("Success"));
	}
}