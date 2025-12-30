using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31375 : _IssuesUITest
	{
		public Issue31375(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Windows] RefreshView Command executes multiple times when IsRefreshing is set to True";

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void VerifyRefreshViewCommandExecution()
		{
			App.WaitForElement("CounterLabel");
			App.Tap("RefreshButton");
			App.Tap("RefreshButton");
			Assert.That(App.FindElement("CounterLabel").GetText(), Is.EqualTo("2"));
		}
	}
}