using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4715 : _IssuesUITest
{
	public Issue4715(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] Layout containers not visible to UI automation";

	[Test]
	[Category(UITestCategories.Layout)]
	public void GridWithAutomationIdIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// Grid with AutomationId must be visible in the UIA tree
		App.WaitForElement("TestGrid");
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerticalStackLayoutWithAutomationIdIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// VerticalStackLayout with AutomationId must be visible in the UIA tree
		App.WaitForElement("TestVerticalStackLayout");
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void HorizontalStackLayoutWithAutomationIdIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// HorizontalStackLayout with AutomationId must be visible in the UIA tree
		App.WaitForElement("TestHorizontalStackLayout");
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void FlexLayoutWithAutomationIdIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// FlexLayout with AutomationId must be visible in the UIA tree
		App.WaitForElement("TestFlexLayout");
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void AbsoluteLayoutWithAutomationIdIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// AbsoluteLayout with AutomationId must be visible in the UIA tree
		App.WaitForElement("TestAbsoluteLayout");
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void NestedOuterLayoutWithAutomationIdIsFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// Outer nested Grid with AutomationId must be visible
		App.WaitForElement("TestNestedOuterGrid");
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void AnonymousLayoutWithoutAutomationIdIsNotFoundByAppium()
	{
		App.WaitForElement("WaitForStubControl");

		// Anonymous Grid (no AutomationId) must NOT be found in the UIA tree
		App.WaitForNoElement("Anonymous Grid");
	}
}
