using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue29700 : _IssuesUITest
	{
		public Issue29700(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TextTransform Property Does Not Apply at Runtime When TextType is equal to Html";

		[Test]
		[Category(UITestCategories.Label)]
		public void TextTransformPropertyAppliesAtRuntime()
		{
			var label = App.WaitForElement("TestLabel");
			Assert.That(label.GetText(), Is.EqualTo("Hello, World"));
			App.Tap("ChangeTextTransformButton");
			Assert.That(label.GetText(), Is.EqualTo("HELLO, WORLD"));
			App.Tap("ChangeTextTransformButton");
			Assert.That(label.GetText(), Is.EqualTo("hello, world"));
			App.Tap("ChangeTextTransformButton");
			Assert.That(label.GetText(), Is.EqualTo("Hello, World"));
		}
	}
}