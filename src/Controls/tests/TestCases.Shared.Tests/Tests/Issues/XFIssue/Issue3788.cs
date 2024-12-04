using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue3788 : _IssuesUITest
{
	public Issue3788(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[UWP] ListView with observable collection always seems to refresh the entire list";

	// TODO: these _ variables are variabled in the UI and need to be AutomationId values
	//[Test]
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void ReplaceItemScrollsListToTop()
	//{
	//	App.WaitForElement(_replaceMe);
	//	App.Tap(_buttonText);
	//	App.WaitForElement(_last);
	//}
}