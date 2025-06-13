using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21325 : _IssuesUITest
	{
		public override string Issue => "Grey cannot be used to set Background property, and doesn't display a preview in the XAML editor";

		public Issue21325(TestDevice device) : base(device)
		{
		}


		[Fact]
		[Category(UITestCategories.Brush)]
		public void VerifyGreyShades()
		{
			App.WaitForElement("greyShades");
			VerifyScreenshot();
		}
	}
}