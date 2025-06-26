using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Trait("Category", UITestCategories.Layout)]
public class Issue6932_emptyviewstring : _IssuesUITest
{
	const int Count = 10;
	const string LayoutAutomationId = "1";
	const string AddAutomationId = "Add";
	const string RemoveAutomationId = "Remove";
	const string ClearAutomationId = "Clear";
	const string EmptyViewStringDescription = "Nothing to see here";

	public Issue6932_emptyviewstring(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "EmptyView for BindableLayout (string)";

	[Fact]
	public void BEmptyViewStringBecomesVisibleWhenItemsSourceIsCleared()
	{
		App.WaitForElement(AddAutomationId);
		for (var i = 0; i < Count / 2; i++)
			App.Tap(AddAutomationId);
		App.Tap(ClearAutomationId);
		App.WaitForElement(EmptyViewStringDescription);
	}

	[Fact]
	public void AEmptyViewStringBecomesVisibleWhenItemsSourceIsEmptiedOneByOne()
	{
		App.WaitForElement(LayoutAutomationId);

		for (var i = 0; i < Count; i++)
			App.Tap(RemoveAutomationId);

		App.WaitForElement(EmptyViewStringDescription);
	}

	[Fact]
	public void CEmptyViewStringHidesWhenItemsSourceIsFilled()
	{
		App.WaitForElement(EmptyViewStringDescription);
		App.Tap(AddAutomationId);
		App.WaitForNoElement(EmptyViewStringDescription);
	}
}