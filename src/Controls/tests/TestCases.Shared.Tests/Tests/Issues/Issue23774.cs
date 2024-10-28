#if !MACCATALYST
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue23774 : _IssuesUITest 
	{
        public override string Issue => "[Android] Text Clipping Issue in Entry with Long Strings";

        public Issue23774(TestDevice device) : base(device)
        {
        }

        [Test]
		[Category(UITestCategories.Entry)]
		public void VerifyEntryIsReadOnlyInitialLoading()
        {
			App.WaitForElement("Label");
			VerifyScreenshot();
        }
    }
}
#endif