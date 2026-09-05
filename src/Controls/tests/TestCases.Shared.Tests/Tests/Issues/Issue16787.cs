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
		[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
		public void CollectionViewBindingContextOnlyChangesOnce()
		{
			ClassicAssert.AreEqual("1", App.WaitForElement("LabelBindingCount").GetText());
		}
	}
}
