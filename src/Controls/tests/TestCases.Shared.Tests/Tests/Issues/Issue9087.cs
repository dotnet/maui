#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9087 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue9087(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Scrollview with null content crashes on UWP";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		public void BindablePropertiesAvailableAtOnElementChanged()
		{
			App.WaitForNoElement(Success);
		}
	}
}
#endif