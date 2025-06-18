using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5949 : _IssuesUITest
{
	public const string BackButton = "5949GoBack";
	public const string ToolBarItem = "Login";

	public Issue5949(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView cannot access a disposed object.";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void DoNotAccessDisposedCollectionView()
	{
		App.WaitForElement(ToolBarItem);
		App.Tap(ToolBarItem);
		App.WaitForElement(BackButton);
		App.Tap(BackButton);
		App.WaitForElement(ToolBarItem);
	}
}