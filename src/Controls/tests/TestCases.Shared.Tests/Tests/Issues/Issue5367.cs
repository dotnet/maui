using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5367 : _IssuesUITest
	{
		const string MaxLengthEditor = "MaxLengthEditor";
		const string ForceBigStringButton = "StringButton";

		public Issue5367(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Editor with MaxLength";

		[Fact]
		[Category(UITestCategories.Editor)]
		[Category(UITestCategories.Compatibility)]
		public void Issue5367Test()
		{
			App.WaitForElement(MaxLengthEditor);
			App.Tap(ForceBigStringButton);
			var text = App.WaitForElement(MaxLengthEditor).GetText() ?? string.Empty;
			int count = text.Count();
			Assert.Equal(14, count);
		}
	}
}