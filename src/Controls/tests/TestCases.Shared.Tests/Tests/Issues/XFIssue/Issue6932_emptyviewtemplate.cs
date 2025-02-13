using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Layout)]
public class Issue6932_emptyviewtemplate : _IssuesUITest
{
	const int Count = 10;
	const string LayoutAutomationId = "1";
	const string AddAutomationId = "Add";
	const string RemoveAutomationId = "Remove";
	const string ClearAutomationId = "Clear";
	const string EmptyTemplateAutomationId = "No items here";
	public Issue6932_emptyviewtemplate(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "EmptyView for BindableLayout (template)";

	[Test]
	public void BEmptyViewTemplateBecomesVisibleWhenItemsSourceIsCleared()
	{
		App.WaitForElement(AddAutomationId);

		for (var i = 0; i < Count / 2; i++)
			App.Tap(AddAutomationId);

		App.Tap(ClearAutomationId);
		App.WaitForElement(EmptyTemplateAutomationId);
	}

	[Test]
	public void AEmptyViewTemplateBecomesVisibleWhenItemsSourceIsEmptiedOneByOne()
	{
		App.WaitForElement(LayoutAutomationId);

		for (var i = 0; i < Count; i++)
			App.Tap(RemoveAutomationId);

		App.WaitForElement(EmptyTemplateAutomationId);
	}

	[Test]
	public void CEmptyViewTemplateHidesWhenItemsSourceIsFilled()
	{
		App.WaitForElement(EmptyTemplateAutomationId);
		App.Tap(AddAutomationId);
		App.WaitForNoElement(EmptyTemplateAutomationId);
	}
}