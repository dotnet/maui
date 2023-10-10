using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16787 : _IssuesUITest
	{
		public Issue16787(TestDevice device) : base(device)
		{
		}

		public override string Issue => "CollectionView runtime binding errors when loading the ItemSource asynchronously";

		[Test]
		public void CollectionViewBindingContextOnlyChangesOnce()
		{
			Assert.AreEqual("1", App.WaitForElement("LabelBindingCount").GetText());
		}
	}
}
