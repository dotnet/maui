using System.Diagnostics;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TableView)]
public class Bugzilla31333 : _IssuesUITest
{
	public Bugzilla31333(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Focus() on Entry in ViewCell brings up keyboard, but doesn't have cursor in EditText";

	// TODO: Migrating from Xamarin.UITest, some method calls in here
	// do not translate to Appium (yet) need to look into that later.
	// 	[FailsOnAndroidWhenRunningOnXamarinUITest]
	// 	[FailsOnIOSWhenRunningOnXamarinUITest]
	// 	[Test]
	// #if __MACOS__
	// 	[Ignore("EnterText on UITest.Desktop not implemented")]
	// #endif
	// 	//[UiTest(typeof(NavigationPage))]
	// 	public void Issue31333FocusEntryInListViewCell()
	// 	{
	// 		App.Tap("Focus Entry in ListView");
	// 		WaitForFocus();
	// 		App.EnterText("Entry in ListView Success");
	// 		WaitForTextQuery("Entry in ListView Success");
	// 		App.Tap("Focus Entry in ListView");
	// 	}

	// 	[FailsOnAndroid]
	// 	[FailsOnIOSWhenRunningOnXamarinUITest]
	// 	[Test]
	// #if __MACOS__
	// 	[Ignore("EnterText on UITest.Desktop not implemented")]
	// #endif
	// 	//[UiTest(typeof(NavigationPage))]
	// 	public void Issue31333FocusEditorInListViewCell()
	// 	{
	// 		App.Tap("Focus Editor in ListView");
	// 		WaitForFocus();
	// 		App.EnterText("Editor in ListView Success");
	// 		WaitForTextQuery("Editor in ListView Success");
	// 		App.Tap("Focus Editor in ListView");
	// 	}


	// 	[FailsOnAndroid]
	// 	[FailsOnIOSWhenRunningOnXamarinUITest]
	// 	[Test]
	// #if __MACOS__
	// 	[Ignore("EnterText on UITest.Desktop not implemented")]
	// #endif
	// 	//[UiTest(typeof(NavigationPage))]
	// 	public void Issue31333FocusEntryInTableViewCell()
	// 	{
	// 		App.Tap("Focus Entry in Table");
	// 		WaitForFocus();
	// 		App.EnterText("Entry in TableView Success");
	// 		WaitForTextQuery("Entry in TableView Success");
	// 		App.Tap("Focus Entry in Table");
	// 	}

	// 	[FailsOnAndroid]
	// 	[FailsOnIOSWhenRunningOnXamarinUITest]
	// 	[Test]
	// #if __MACOS__
	// 	[Ignore("EnterText on UITest.Desktop not implemented")]
	// #endif
	// 	//[UiTest(typeof(NavigationPage))]
	// 	public void Issue31333FocusEditorInTableViewCell()
	// 	{
	// 		App.Tap("Focus Editor in Table");
	// 		WaitForFocus();
	// 		App.EnterText("Editor in TableView Success");
	// 		WaitForTextQuery("Editor in TableView Success");
	// 		App.Tap("Focus Editor in Table");
	// 	}

	// 	void WaitForFocus()
	// 	{
	// 		Task.Delay(500).Wait();
	// 	}

	// 	void WaitForTextQuery(string text)
	// 	{
	// 		var watch = new Stopwatch();
	// 		watch.Start();

	// 		// 4-5 seconds should be more than enough time to wait for the query to work
	// 		while (watch.ElapsedMilliseconds < 5000)
	// 		{
	// 			// We have to query this way (instead of just using WaitForElement) because
	// 			// WaitForElement on iOS won't find text in Entry or Editor
	// 			// And we can't rely on running this query immediately after entering the text into the control
	// 			// because on Android the query will occasionally fail if it runs too soon after entering the text
	// 			var textQuery = App.Query(query => query.Text(text);
	// 			if (textQuery.Length > 0)
	// 			{
	// 				return;
	// 			}

	// 			Task.Delay(1000).Wait();
	// 		}

	// 		watch.Stop();

	// 		Assert.Fail($"Timed out waiting for text '{text}'");
	// 	}
}