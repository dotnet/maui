using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5949 : _IssuesUITest
{
	public Issue5949(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView cannot access a disposed object.";

	//[Test]
	//[Category(UITestCategories.CollectionView)]
	//public void DoNotAccessDisposedCollectionView()
	//{
	//	App.WaitForElement("Login");
	//	App.Tap("Login");

	//	App.WaitForElement(Issue5949_2.BackButton);
	//	App.Tap(Issue5949_2.BackButton);

	//	App.WaitForElement("Login");
	//}
}