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
	//[FailsOnIOS]
	//public void EntryScrollTest()
	//{
	//	RunningApp.Tap(EntryTest);
	//	var originalPosition = RunningApp.WaitForElement(EntrySuccess)[0].Rect;
	//	RunningApp.Tap(EntryToClick);
	//	RunningApp.EnterText(EntryToClick, "keyboard");

	//	// if the device has too much height then try clicking the second entry
	//	// to trigger keyboard movement
	//	if (RunningApp.Query(EntrySuccess).Length != 0)
	//	{
	//		RunningApp.Tap(ResetKeyboard);
	//		RunningApp.DismissKeyboard();
	//		RunningApp.Tap(EntryToClick2);
	//		RunningApp.EnterText(EntryToClick2, "keyboard");
	//	}

	//	var entry = RunningApp.Query(EntrySuccess);

	//	// ios10 on appcenter for some reason still returns this entry
	//	// even though it's hidden so this is a fall back test just to ensure
	//	// that the entry has scrolled up
	//	if (entry.Length > 0 && entry[0].Rect.Y > 0)
	//	{
	//		Thread.Sleep(2000);
	//		entry = RunningApp.Query(EntrySuccess);

	//		if (entry.Length > 0)
	//			Assert.LessOrEqual(entry[0].Rect.Y, originalPosition.Y);
	//	}

	//	RunningApp.Tap(ResetKeyboard2);
	//	var finalPosition = RunningApp.WaitForElement(EntrySuccess)[0].Rect;

	//	// verify that label has returned to about the same spot
	//	var diff = Math.Abs(originalPosition.Y - finalPosition.Y);
	//	Assert.LessOrEqual(diff, 2);

	//}

	//[Test]
	//public void ListViewScrollTest()
	//{
	//	RunningApp.Tap(ListViewTest);
	//	RunningApp.WaitForElement("Item0");

	//}

	//[Test]
	//[Compatibility.UITests.FailsOnIOS]
	//public void SafeAreaOnBlankPage()
	//{
	//	RunningApp.Tap(EmptyPageSafeAreaTest);
	//	var noSafeAreaLocation = RunningApp.WaitForElement(SafeAreaTopLabel);
	//	Assert.AreEqual(0, noSafeAreaLocation[0].Rect.Y);
	//}

	//[Test]
	//[Compatibility.UITests.FailsOnIOS]
	//public void SafeArea()
	//{
	//	RunningApp.Tap(SafeAreaTest);
	//	var noSafeAreaLocation = RunningApp.WaitForElement(SafeAreaBottomLabel);

	//	Assert.AreEqual(1, noSafeAreaLocation.Length);
	//	RunningApp.Tap(Reset);

	//	RunningApp.Tap(ToggleSafeArea);
	//	RunningApp.Tap(SafeAreaTest);
	//	var safeAreaLocation = RunningApp.WaitForElement(SafeAreaBottomLabel);
	//	Assert.AreEqual(1, safeAreaLocation.Length);

	//	Assert.Greater(safeAreaLocation[0].Rect.Y, noSafeAreaLocation[0].Rect.Y);
	//}

	//[Test]
	//public void PaddingWithoutSafeArea()
	//{
	//	RunningApp.EnterText(q => q.Raw($"* marked:'{PaddingEntry}'"), "0");
	//	RunningApp.Tap(PaddingTest);
	//	var zeroPadding = RunningApp.WaitForElement(PaddingLabel);

	//	Assert.AreEqual(1, zeroPadding.Length);
	//	RunningApp.Tap(Reset);

	//	RunningApp.EnterText(PaddingEntry, "100");
	//	RunningApp.Tap(PaddingTest);
	//	var somePadding = RunningApp.WaitForElement(PaddingLabel);
	//	Assert.AreEqual(1, somePadding.Length);

	//	Assert.Greater(somePadding[0].Rect.Y, zeroPadding[0].Rect.Y);
	//}
}