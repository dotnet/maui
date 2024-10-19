#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14037 : UITest
{
	const string ALWAYS_FINISH_ACTIVITIES_PROPERTY = "always_finish_activities";

	static readonly KeyValuePair<string, object> ENABLE_ALWAYS_FINISH_ACTIVITIES = new(ALWAYS_FINISH_ACTIVITIES_PROPERTY, 1);
	static readonly KeyValuePair<string, object> DISABLE_ALWAYS_FINISH_ACTIVITIES = new(ALWAYS_FINISH_ACTIVITIES_PROPERTY, 0);

	public string Issue => "[Android] MauiAppCompatActivity default prevents getting result from an Activity.";

	public Issue14037(TestDevice device) : base(device) { }

	protected override void FixtureSetup()
	{
		App.GoToTest(Issue);
	}

	[Test]
	[Category(UITestCategories.Visual)]
	public void GetResultFromActivity()
	{
		var dict = new Dictionary<string, object>()
		{
			{ ENABLE_ALWAYS_FINISH_ACTIVITIES.Key, ENABLE_ALWAYS_FINISH_ACTIVITIES.Value },
		};
		App.SetProperties(dict);

		App.WaitForElement("LaunchActivityForResult");
		App.Tap("LaunchActivityForResult");

		var end = "END";
		Console.WriteLine(end);

		var dict2 = new Dictionary<string, object>()
		{
			{ DISABLE_ALWAYS_FINISH_ACTIVITIES.Key, DISABLE_ALWAYS_FINISH_ACTIVITIES.Value },
		};
		App.SetProperties(dict2);
	}
}

#endif