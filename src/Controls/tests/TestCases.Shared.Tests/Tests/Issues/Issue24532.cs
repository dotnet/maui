using System.Diagnostics;
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
		public void RenderingPerformance()
		{
			var sw = new Stopwatch();
			sw.Start();
			App.WaitForElement("TheFirstOne");
			sw.Stop();
			
			Console.WriteLine($"RenderingPerformance first render took {sw.ElapsedMilliseconds}ms to complete");

			sw.Restart();
			App.Tap("RegenerateItems");
			App.WaitForElement("TheSecondOne");
			sw.Stop();
			
			Console.WriteLine($"RenderingPerformance changing context took {sw.ElapsedMilliseconds}ms to complete");
		}
	}
}
