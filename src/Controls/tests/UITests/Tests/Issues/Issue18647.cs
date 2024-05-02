using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18647 : _IssuesUITest
	{

		public Issue18647(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Editor TextTransform property works as expected";

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorTextTransformWorks()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Input the string 'aBcDeFg' into the editor below.
			App.EnterText("NoneTextTransformEditor", "aBcDeFg");

			// 2. The test fails if the editor below does not display the string 'aBcDeFg' exactly (case-sensitive).
			var result2 = App.FindElement("NoneTextTransformEditor").GetText()?.Trim();
			Assert.AreEqual("aBcDeFg", result2);

			// 3. Input the string 'aBcDeFg' into the editor below.
			App.EnterText("LowercaseTextTransformEditor", "aBcDeFg");

			// 4. The test fails if the editor below does not display the string 'abcdefg' exactly (case-sensitive).
			var result4 = App.FindElement("LowercaseTextTransformEditor").GetText()?.Trim();
			Assert.AreEqual("abcdefg", result4);

			// 5. Input the string 'aBcDeFg' into the editor below.
			App.EnterText("UppercaseTextTransformEditor", "aBcDeFg");

			// 6. The test fails if the editor below does not display the string 'ABCDEFG' exactly (case-sensitive).
			var result6 = App.FindElement("UppercaseTextTransformEditor").GetText()?.Trim();
			Assert.AreEqual("ABCDEFG", result6);

			// 7. The test fails if the editor below does not display the string 'abcdefg' exactly (case-sensitive).
			var result7 = App.FindElement("FilledLowercaseTextTransformEditor").GetText()?.Trim();
			Assert.AreEqual("abcdefg", result7);

			// 8. The test fails if the editor below does not display the string 'ABCDEFG' exactly (case-sensitive).
			var result8 = App.FindElement("FilledUppercaseTextTransformEditor").GetText()?.Trim();

			Assert.AreEqual("ABCDEFG", result8);
		}
	}
}
