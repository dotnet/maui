using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue30426 : _IssuesUITest
	{
		public Issue30426(TestDevice device) : base(device)
		{
		}

		public override string Issue => "IImage Downsize() throws Exception in 9.0.81 when called from background thread on iOS";

		[Test]
		[Category(UITestCategories.Image)]
		public void LoadTestImageButtonShouldLoadImageWithoutException()
		{
			App.WaitForElement("WaitForStubControl");
			App.WaitForElement("LoadImageButton");
			App.Tap("LoadImageButton");
			App.WaitForElement("StatusLabel");
			Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Status: Image loaded successfully"));
			App.WaitForElement("ProcessImageButton");
			App.Tap("ProcessImageButton");
			App.WaitForElement("StatusLabel");
			Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Status: Image processed successfully"));
		}
	}
}