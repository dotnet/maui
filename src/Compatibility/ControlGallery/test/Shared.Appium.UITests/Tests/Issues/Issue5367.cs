using NUnit.Framework;
using UITest.Appium;

namespace UITests.Tests.Issues
{
	public class Issue5367 : IssuesUITest
	{
		const string MaxLengthEditor = "MaxLength Editor";
		const string ForceBigStringButton = "Force Big String Button";

		public Issue5367(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Editor with MaxLength";

		[Test]
		[Category(UITestCategories.Editor)]
		public void Issue3390Test()
		{
			App.WaitForElement(MaxLengthEditor);
			App.Click(ForceBigStringButton);
		}
	}
}