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

		public override string Issue => "[Bug] Collection View items don't load bindable properties values inside OnElementChanged";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.CollectionView2)]
		public void BindablePropertiesAvailableAtOnElementChanged()
		{
			App.WaitForElement(Success);
		}
	}
}