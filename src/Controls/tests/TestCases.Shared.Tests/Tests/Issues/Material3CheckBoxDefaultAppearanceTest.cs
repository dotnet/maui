#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Material3CheckBoxDefaultAppearanceTest : _IssuesUITest
    {
        public Material3CheckBoxDefaultAppearanceTest(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "Material3 CheckBox Testing";

        [Test]
        [Category(UITestCategories.Material3)]
        public void Material3CheckBox_DefaultAppearance()
        {
            App.WaitForElement("DefaultCheckBox");
            VerifyScreenshot();
        }
    }
}
#endif