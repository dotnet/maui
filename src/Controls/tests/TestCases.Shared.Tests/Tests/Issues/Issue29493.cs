#if WINDOWS // Exising issue in Android, iOS and MacCatalyst : https://github.com/dotnet/maui/issues/14497
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29493 : _IssuesUITest
{
	public override string Issue => "[Windows] SearchHandler APIs are not functioning properly";

	public Issue29493(TestDevice device)
	: base(device)
	{ }

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void VerifySearchHandlerValues()
	{
		App.WaitForElement("button");
		App.Tap("button");
		VerifyScreenshot();
	}
}
#endif