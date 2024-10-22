#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue25224 : _IssuesUITest
	{
		public Issue25224(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView - EmptyView with EmptyViewTemplate for Data template selector page throws an exception";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewEmptyViewWithDataTemplateSelectorShouldNotCrash()
		{
			App.WaitForNoElement("Success");
		}
	}
}
#endif