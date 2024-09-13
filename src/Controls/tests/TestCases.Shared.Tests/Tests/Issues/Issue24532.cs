#if ANDROID || IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24532 : _IssuesUITest
	{
		public Issue24532(TestDevice device) : base(device) { }

		public override string Issue => "Rendering performance";

		[Test]
		[Category(UITestCategories.Performance)]
		public async Task RenderingPerformance()
		{
			var button = App.WaitForElement("StartButton");
			App.Tap("StartButton");

			string? text;
			while (true)
			{
				text = button.GetText();
				if (string.IsNullOrEmpty(text) || text == "Start")
				{
					await Task.Delay(1000);
					continue;
				}

				break;
			}

			var times = text.Split(',');
			Console.WriteLine(@$"First render: {times[0]}ms");
			Console.WriteLine(@$"Binding context changed: {times[1]}ms");
			Console.WriteLine(@$"Clear: {times[2]}ms");
		}
	}
}
#endif