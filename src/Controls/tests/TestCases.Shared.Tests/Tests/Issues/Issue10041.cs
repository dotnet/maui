#nullable disable
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10041 : _IssuesUITest
	{
		public Issue10041(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Throws exception when shell item is null";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShouldThrowExceptionWhenShellItemIsNull()
		{

			Assert.Throws<InvalidOperationException>(() =>
			{
				// App.WaitForElement("CloseButton");
			});
			

		}
	}
}