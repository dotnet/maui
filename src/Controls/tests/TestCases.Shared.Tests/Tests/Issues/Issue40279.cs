#if ANDROID || IOS //The test fails on Windows and MacCatalyst because the BackgroundApp and ForegroundApp method, which is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40279 : _IssuesUITest
{
	public Issue40279(TestDevice device) : base(device) { }

	public override string Issue => "Replacing Shell while stopped does not crash on resume";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ReplacingShellWhileStoppedDoesNotCrashOnResume()
	{
		App.WaitForElement("Issue40279StartSignInButton");
		App.Tap("Issue40279StartSignInButton");
		App.BackgroundApp();
		App.ForegroundApp();

		var label = App.WaitForElement("Issue40279SignedInLabel");
		Assert.That(label.GetText(), Is.EqualTo("Shell replaced while stopped"));
	}
}
#endif
