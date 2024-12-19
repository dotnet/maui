using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14037 : _IssuesUITest
{
	readonly AppiumQuery _keyQuery = AppiumQuery.ByClass("android.widget.EditText");
	readonly AppiumQuery _valueQuery = AppiumQuery.ByClass("android.widget.EditText");
	readonly AppiumQuery _returnQuery = AppiumQuery.ByClass("android.widget.Button");

	protected AppiumAndroidApp? AndroidApp { get; private set; }

	public override string Issue => "MauiAppCompatActivity.AllowFragmentRestore=False prevents getting result from Activity";

	public Issue14037(TestDevice device) : base(device) { }

	public override void TestSetup()
	{
		base.TestSetup();

		AndroidApp = App is AppiumAndroidApp androidApp
			? androidApp
			: throw new InvalidOperationException($"Invalid App Type For this Test: {App}. Expected {nameof(AppiumAndroidApp)}.");
	}

	[Test]
	[Category(UITestCategories.Visual)]
	public void GetResultFromActivity()
	{
		App.WaitForElement("LaunchActivityForResult");
		App.Tap("LaunchActivityForResult");

		AndroidApp?.WaitForElement(() => _keyQuery.FindElements(AndroidApp).FirstOrDefault());
		_keyQuery.FindElements(AndroidApp)?.FirstOrDefault()?.SendKeys("A_KEY");

		AndroidApp?.WaitForElement(() => _valueQuery.FindElements(AndroidApp).LastOrDefault());
		_valueQuery.FindElements(AndroidApp)?.LastOrDefault()?.SendKeys("A_VALUE");

		_returnQuery.FindElements(AndroidApp)?.FirstOrDefault()?.Tap();
		App.WaitForElement("LaunchActivityForResult");

		VerifyScreenshot();
	}
}