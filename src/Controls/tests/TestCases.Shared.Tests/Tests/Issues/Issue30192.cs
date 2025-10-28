#if TEST_FAILS_ON_CATALYST // https://github.com/dotnet/maui/issues/30322
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30192 : _IssuesUITest
{
    public Issue30192(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "TimePicker FlowDirection Not Working on All Platforms";

    [Test]
    [Category(UITestCategories.TimePicker)]
    public void TimePickerFlowDirectionTest()
    {
        App.WaitForElement("ToggleFlowDirectionButton");
        App.Tap("ToggleFlowDirectionButton");
        VerifyScreenshot();
    }
}
#endif