using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4715 : _IssuesUITest
{
	public Issue4715(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] Layout containers not visible to UI automation";

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void GridWithAutomationIdOnlyIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// AutomationId-only layouts must remain visible to Windows UI tests.
		App.WaitForElement("TestGrid");
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void VerticalStackLayoutWithAccessibleTreeOptInIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// VerticalStackLayout with explicit accessible-tree opt-in must be visible in the UIA tree.
		App.WaitForElement("TestVerticalStackLayout");
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void HorizontalStackLayoutWithAccessibleTreeOptInIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// HorizontalStackLayout with explicit accessible-tree opt-in must be visible in the UIA tree.
		App.WaitForElement("TestHorizontalStackLayout");
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void FlexLayoutWithAccessibleTreeOptInIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// FlexLayout with explicit accessible-tree opt-in must be visible in the UIA tree.
		App.WaitForElement("TestFlexLayout");
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void AbsoluteLayoutWithAccessibleTreeOptInIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// AbsoluteLayout with explicit accessible-tree opt-in must be visible in the UIA tree.
		App.WaitForElement("TestAbsoluteLayout");
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void NestedOuterLayoutWithAccessibleTreeOptInIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// Outer nested Grid with explicit accessible-tree opt-in must be visible.
		App.WaitForElement("TestNestedOuterGrid");
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void LayoutWithAccessibleTreeOptOutIsNotFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// A layout with an AutomationId but an explicit IsInAccessibleTree="False" opts out of the
		// UIA Control view. Appium must NOT find it by its AutomationId, proving the Raw opt-out takes
		// precedence over the AutomationId discoverability hook. Anonymous (no-AutomationId) layout
		// exclusion is covered authoritatively by the LayoutPanel device tests.
		App.WaitForNoElement("OptedOutGrid");
	}
}
