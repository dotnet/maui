using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class ShellInsets : _IssuesUITest
{
	const string EntryTest = nameof(EntryTest);
	const string EntryToClick = "EntryToClick";
	const string EntryToClick2 = "EntryToClick2";
	const string CreateTopTabButton = "CreateTopTabButton";
	const string CreateBottomTabButton = "CreateBottomTabButton";

	const string EntrySuccess = "EntrySuccess";
	const string ResetKeyboard = "Hide Keyboard";
	const string ResetKeyboard2 = "HideKeyboard2";
	const string ResetButton = "Reset";

	const string ToggleSafeArea = "ToggleSafeArea";
	const string SafeAreaTest = "SafeAreaTest";
	const string SafeAreaTopLabel = "SafeAreaTopLabel";
	const string SafeAreaBottomLabel = "SafeAreaBottomLabel";

	const string ListViewTest = "ListViewTest";

	const string PaddingTest = "PaddingTest";
	const string PaddingEntry = "PaddingEntry";
	const string PaddingLabel = "PaddingLabel";

	const string EmptyPageSafeAreaTest = "EmptyPageSafeAreaTest";
	public ShellInsets(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Inset Test";

#if ANDROID || IOS // Keyboard test is only applicable for mobile platforms.
	[Test, Order(4)]
	public void EntryScrollTest()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(EntryTest);
		App.Tap(EntryTest);
		var originalPosition = App.WaitForElement(EntrySuccess).GetRect();
		App.Tap(EntryToClick);
		App.EnterText(EntryToClick, "keyboard");
		var isLabelVisible = App.WaitForElement(EntrySuccess).GetRect().Y <= originalPosition.Y;

		if (!isLabelVisible)
		{
			App.Tap(ResetKeyboard);
			App.DismissKeyboard();
			App.Tap(EntryToClick2);
			App.EnterText(EntryToClick2, "keyboard");
		}

		var movedPosition = App.WaitForElement(EntrySuccess).GetRect();
		Assert.That(movedPosition.Y, Is.LessThanOrEqualTo(originalPosition.Y));
		App.WaitForElement(ResetKeyboard2);
		App.Tap(ResetKeyboard2);
		var finalPosition = App.WaitForElement(EntrySuccess).GetRect();

		var positionDifference = Math.Abs(originalPosition.Y - finalPosition.Y);
		Assert.That(positionDifference, Is.LessThanOrEqualTo(2));
	}
#endif

#if !MACCATALYST // This test fails on Catalyt while running on CI, but locally its not failing.
	[Test, Order(5)]
	public void ListViewScrollTest()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(ListViewTest);
		App.Tap(ListViewTest);
		App.WaitForElement("Item0");

	}
#endif

#if IOS // SafeArea is only enabled for iOS.
	// SafeArea is not working as expected on iOS Issue: https://github.com/dotnet/maui/issues/19720
	//[Test, Order(2)]
	public void SafeAreaOnBlankPage()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(EmptyPageSafeAreaTest);
		App.Tap(EmptyPageSafeAreaTest);
		var noSafeAreaLocation = App.WaitForElement(SafeAreaTopLabel);
		Assert.That(noSafeAreaLocation.GetRect().Y, Is.EqualTo(0));

	}

	//[Test, Order(3)]
	public void SafeArea()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(SafeAreaTest);
		App.Tap(SafeAreaTest);
		var noSafeAreaLocation = App.FindElements(SafeAreaBottomLabel).Count();
		var noSafeArea = App.WaitForElement(SafeAreaBottomLabel).GetRect().Y;
		Assert.That(noSafeAreaLocation, Is.EqualTo(1));
		App.Tap(ResetButton);
		App.Tap(ToggleSafeArea);
		App.Tap(SafeAreaTest);
		var safeAreaLocation = App.FindElements(SafeAreaBottomLabel).Count();
		var safeArea = App.WaitForElement(SafeAreaBottomLabel).GetRect().Y;
		Assert.That(safeAreaLocation, Is.EqualTo(1));
		Assert.That(safeArea, Is.GreaterThan(noSafeArea));
	}
#endif

	[Test, Order(1)]
	public void PaddingWithoutSafeArea()
	{

		App.WaitForElement(PaddingEntry);
		App.EnterText(PaddingEntry, "0");
		App.WaitForElement(PaddingTest);
		App.Tap(PaddingTest);
		App.WaitForElement(PaddingLabel);
		var zeroPadding = App.FindElements(PaddingLabel).Count();
		var zeroPaddingValue = App.WaitForElement(PaddingLabel).GetRect().Y;
		Assert.That(zeroPadding, Is.EqualTo(1));
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(PaddingEntry);
		App.EnterText(PaddingEntry, "100");
		App.WaitForElement(PaddingTest);
		App.Tap(PaddingTest);
		App.WaitForElement(PaddingLabel);
		var somePadding = App.FindElements(PaddingLabel).Count();
		var somePaddingValue = App.WaitForElement(PaddingLabel).GetRect().Y;
		Assert.That(somePadding, Is.EqualTo(1));
		Assert.That(somePaddingValue, Is.GreaterThan(zeroPaddingValue));
	}
}