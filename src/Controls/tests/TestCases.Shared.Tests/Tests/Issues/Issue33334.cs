using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33334 : _IssuesUITest
{
	public Issue33334(TestDevice device) : base(device) { }

	public override string Issue => "Password obfuscation causes crash on Windows";

	[Test]
	[Category(UITestCategories.Entry)]
	public void PasswordEntryShouldNotCrashOnTextSet()
	{
		App.WaitForElement("ReproButton");
		App.Tap("ReproButton");
		
		var entry = App.WaitForElement("PasswordEntry");
		Assert.That(entry.GetText(), Has.Length.EqualTo(4));
	}
}
