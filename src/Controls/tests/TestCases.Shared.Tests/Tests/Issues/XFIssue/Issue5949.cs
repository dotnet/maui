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
	//	RunningApp.WaitForElement("Login");
	//	RunningApp.Tap("Login");

	//	RunningApp.WaitForElement(Issue5949_2.BackButton);
	//	RunningApp.Tap(Issue5949_2.BackButton);

	//	RunningApp.WaitForElement("Login");
	//}
}