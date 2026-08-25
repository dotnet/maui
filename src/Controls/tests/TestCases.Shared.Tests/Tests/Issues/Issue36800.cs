#if IOS // Fix is specific to iOS's ContentInsetAdjustmentBehavior/AdjustedContentInset handling; not applicable on Android/Catalyst/Windows
using System.Globalization;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36800 : _IssuesUITest
{
	public override string Issue => "ScrollView with SafeAreaEdges=\"Container\" reserves the safe area twice, causing phantom scroll range";

	public Issue36800(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ScrollViewWithContainerSafeAreaDoesNotDoubleReserveSafeArea()
	{
		App.WaitForElement("TestScrollView");
		App.Tap("DumpButton");

		var diagText = App.WaitForElement("DiagLabel").GetText() ?? string.Empty;

		Assert.That(diagText, Does.StartWith("Phantom="), $"Unexpected diagnostic text: {diagText}");

		var phantomValueText = diagText.Substring("Phantom=".Length);
		var phantom = double.Parse(phantomValueText, CultureInfo.InvariantCulture);

		Assert.That(phantom, Is.LessThanOrEqualTo(1.0),
			$"ScrollView with SafeAreaEdges=Container should not have a phantom scroll range, but measured {phantom}pt");
	}
}
#endif
