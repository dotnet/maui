using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class RadioButtonTemplateFromStyle : _IssuesUITest
{
	public RadioButtonTemplateFromStyle(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Crash when creating a CollectionView inside a CollectionView";

	// TODO: HostApp UI pushes some ControlGallery specific page? Commented out now, fix that first!
	//[Test]
	//[Category(UITestCategories.RadioButton)]
	//public void ContentRenderers()
	//{
	//	App.WaitForElement("A");
	//	App.WaitForElement("B");
	//	App.WaitForElement("C");
	//}
}