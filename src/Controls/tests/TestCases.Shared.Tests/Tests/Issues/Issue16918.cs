using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16918 : _IssuesUITest
	{
		public Issue16918(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "ImageButton is not properly anti-aliased when scaled down";

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void Issue16918Test()
		{
			// https://github.com/dotnet/maui/issues/16918

			App.WaitForElement("MenuImage");
			VerifyScreenshot();
		}
	}
}