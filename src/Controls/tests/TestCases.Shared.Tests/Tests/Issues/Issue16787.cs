using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16787 : _IssuesUITest
	{
		public Issue16787(TestDevice device) : base(device)
		{
		}

		public override string Issue => "CollectionView runtime binding errors when loading the ItemSource asynchronously";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewBindingContextOnlyChangesOnce()
		{
			ClassicAssert.AreEqual("1", App.WaitForElement("LabelBindingCount").GetText());
		}
	}
}
