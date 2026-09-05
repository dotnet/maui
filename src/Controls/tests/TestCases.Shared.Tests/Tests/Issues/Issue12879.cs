using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12879 : _IssuesUITest
{
	public Issue12879(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Image.HeightRequest not respected when Image added in a Horizontal StackLayout";

	[Test]
	[Category(UITestCategories.Image)]
	public void ImageScalesUpToExplicitHeightRequestInHorizontalStackLayout()
	{
		App.WaitForElement("CheckImageButton");
		App.Tap("CheckImageButton");

		var statusText = App.WaitForElement("ImageStatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("Success"));
	}
}
