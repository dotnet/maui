#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37360 : _IssuesUITest
{
	public Issue37360(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell root remains connected after reentrant modal navigation";

	[Test]
	[Category(UITestCategories.Shell)]
	public void RootRemainsConnectedAfterModalDismissalNavigatesToDetail()
	{
		const string expected = "Root: connected; unexpected pops: 0";
		Assert.That(App.WaitForElement("RootState").GetText(), Is.EqualTo(expected));
		App.WaitForElement("RunControl");
		App.Tap("RunControl");
		if (!App.WaitForTextToBePresentInElement("SequencePhase", "Control complete", TimeSpan.FromSeconds(5)))
			throw new TimeoutException("The ordinary push/pop control did not complete.");
		Assert.That(App.WaitForElement("RootState").GetText(), Is.EqualTo(expected),
			"The root must remain connected after the non-reentrant control.");

		App.Tap("RunRepro");
		if (!App.WaitForTextToBePresentInElement("SequencePhase", "Repro complete", TimeSpan.FromSeconds(12)))
			throw new TimeoutException("The modal-dismissal/detail-return sequence did not complete.");
		Assert.That(App.WaitForElement("RootState").GetText(), Is.EqualTo(expected),
			"Navigating inside Shell.Navigated must not pop or disconnect the ShellContent root.");
	}
}
#endif
