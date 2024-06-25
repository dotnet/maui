#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla36009 : _IssuesUITest
	{
		public Bugzilla36009(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		[Test]
		[Category(UITestCategories.BoxView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid("The content is not visible. The test is failing.")]
		[FailsOnIOS]
		[FailsOnMac]
		public void Bugzilla36009Test()
		{
			App.WaitForElement("Victory");
		}
	}
}
#endif