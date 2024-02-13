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
#if NATIVE_AOT
			Assert.Ignore("Times out when running with NativeAOT, see https://github.com/dotnet/maui/issues/20553");
#endif
			Assert.AreEqual("1", App.WaitForElement("LabelBindingCount").GetText());
		}
	}
}
