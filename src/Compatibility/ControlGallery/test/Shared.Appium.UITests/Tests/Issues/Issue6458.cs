using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Issue6458 : IssuesUITest
	{
		public Issue6458(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Fix load TitleIcon on non app compact";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue6458Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("IssuePageLabel");
			var element = App.WaitForElement("banktitleicon");

			ClassicAssert.NotNull(element);

			var elementRect = element.GetRect();
			ClassicAssert.Greater(elementRect.Height, 10);
			ClassicAssert.Greater(elementRect.Width, 10);
		}
	}
}
