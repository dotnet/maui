using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla39668 : _IssuesUITest
	{
		public Bugzilla39668(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Overriding ListView.CreateDefault Does Not Work on Windows";

		[Fact]
		[Trait("Category", UITestCategories.ListView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Bugzilla39668Test()
		{
			App.WaitForElement("Success");
		}
	}
}