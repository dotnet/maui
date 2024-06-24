using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23029 : _IssuesUITest
	{
		public override string Issue => "RefreshView doesn't use correct layout mechanisms";

		public Issue23029(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void ValidateRefreshViewContentGetsFrameSet()
		{
			App.WaitForElement("SizeChangedLabel");
		}
	}
}