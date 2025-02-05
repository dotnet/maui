using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla52533 : _IssuesUITest
	{
		const string LabelId = "label";

		public Bugzilla52533(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "System.ArgumentException: NaN is not a valid value for width";

		[Test]
		[Category(UITestCategories.Label)]
		public void Bugzilla52533Test()
		{
			App.WaitForElement(LabelId);
		}
	}
}