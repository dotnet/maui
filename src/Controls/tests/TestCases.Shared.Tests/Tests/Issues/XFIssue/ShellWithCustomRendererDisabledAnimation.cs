using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellWithCustomRendererDisabledAnimation : _IssuesUITest
{
	public ShellWithCustomRendererDisabledAnimation(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Shell] Overriding animation with custom renderer to remove animation breaks next navigation";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void NavigationWithACustomRendererThatDoesntSetAnAnimationStillWorks()
	//{
	//	App.Tap("PageLoaded");
	//	App.Tap("GoBack");
	//	App.WaitForElement("PageLoaded");
	//}
}