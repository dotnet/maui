using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22914 : _IssuesUITest
	{
		const string ButtonId = "Tap1Button";

		readonly string[] _expectedNullBackgroundColorIds = [ButtonId, "ContentView1", "Label1", "VerticalStackLayout1"];
		
		public Issue22914(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting BackgroundColor to null does not actually changes BackgroundColor";

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		public void UpdateBackgroundColorToNull()
		{
			var tapAreaResult = App.WaitForElement(ButtonId, $"Timed out waiting for {ButtonId}");
			tapAreaResult.Tap();

			foreach (var elementId in _expectedNullBackgroundColorIds)
			{
				var element = App.WaitForElement(elementId, $"Timed out waiting for {elementId}");
				var elementBackgroundColor = element.GetAttribute<Color?>("BackgroundColor");
				ClassicAssert.IsTrue(elementBackgroundColor is null, $"{elementId} has unexpected BackgroundColor");
			}
		}
	}
}
