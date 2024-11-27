using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class ShellInsets : _IssuesUITest
{
	public ShellInsets(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Inset Test";

	//	[Test]
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void EntryScrollTest()
	//{
	//	App.Tap(EntryTest);
	//	var originalPosition = App.WaitForElement(EntrySuccess)[0].Rect;
	//	App.Tap(EntryToClick);
	//	App.EnterText(EntryToClick, "keyboard");

	//	// if the device has too much height then try clicking the second entry
	//	// to trigger keyboard movement
	//	if (App.Query(EntrySuccess).Length != 0)
	//	{
	//		App.Tap(ResetKeyboard);
	//		App.DismissKeyboard();
	//		App.Tap(EntryToClick2);
	//		App.EnterText(EntryToClick2, "keyboard");
	//	}

	//	var entry = App.Query(EntrySuccess);

	//	// ios10 on appcenter for some reason still returns this entry
	//	// even though it's hidden so this is a fall back test just to ensure
	//	// that the entry has scrolled up
	//	if (entry.Length > 0 && entry[0].Rect.Y > 0)
	//	{
	//		Thread.Sleep(2000);
	//		entry = App.Query(EntrySuccess);

	//		if (entry.Length > 0)
	//			Assert.LessOrEqual(entry[0].Rect.Y, originalPosition.Y);
	//	}

	//	App.Tap(ResetKeyboard2);
	//	var finalPosition = App.WaitForElement(EntrySuccess)[0].Rect;

	//	// verify that label has returned to about the same spot
	//	var diff = Math.Abs(originalPosition.Y - finalPosition.Y);
	//	Assert.LessOrEqual(diff, 2);

	//}

	//[Test]
	//public void ListViewScrollTest()
	//{
	//	App.Tap(ListViewTest);
	//	App.WaitForElement("Item0");

	//}

	//[Test]
	//[Compatibility.UITests.FailsOnIOSWhenRunningOnXamarinUITest]
	//public void SafeAreaOnBlankPage()
	//{
	//	App.Tap(EmptyPageSafeAreaTest);
	//	var noSafeAreaLocation = App.WaitForElement(SafeAreaTopLabel);
	//	Assert.AreEqual(0, noSafeAreaLocation[0].Rect.Y);
	//}

	//[Test]
	//[Compatibility.UITests.FailsOnIOSWhenRunningOnXamarinUITest]
	//public void SafeArea()
	//{
	//	App.Tap(SafeAreaTest);
	//	var noSafeAreaLocation = App.WaitForElement(SafeAreaBottomLabel);

	//	Assert.AreEqual(1, noSafeAreaLocation.Length);
	//	App.Tap(Reset);

	//	App.Tap(ToggleSafeArea);
	//	App.Tap(SafeAreaTest);
	//	var safeAreaLocation = App.WaitForElement(SafeAreaBottomLabel);
	//	Assert.AreEqual(1, safeAreaLocation.Length);

	//	Assert.Greater(safeAreaLocation[0].Rect.Y, noSafeAreaLocation[0].Rect.Y);
	//}

	//[Test]
	//public void PaddingWithoutSafeArea()
	//{
	//	App.EnterText(q => q.Raw($"* marked:'{PaddingEntry}'"), "0");
	//	App.Tap(PaddingTest);
	//	var zeroPadding = App.WaitForElement(PaddingLabel);

	//	Assert.AreEqual(1, zeroPadding.Length);
	//	App.Tap(Reset);

	//	App.EnterText(PaddingEntry, "100");
	//	App.Tap(PaddingTest);
	//	var somePadding = App.WaitForElement(PaddingLabel);
	//	Assert.AreEqual(1, somePadding.Length);

	//	Assert.Greater(somePadding[0].Rect.Y, zeroPadding[0].Rect.Y);
	//}
}