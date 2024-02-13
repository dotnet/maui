using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class GroupListViewHeaderIndexOutOfRange : _IssuesUITest
	{
		const string ButtonId = "button";
		public GroupListViewHeaderIndexOutOfRange(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Group ListView Crashes when ItemSource is Cleared";

		[Test]
		public void GroupListViewHeaderIndexOutOfRangeTest()
		{
#if NATIVE_AOT
			Assert.Ignore("Times out when running with NativeAOT, see https://github.com/dotnet/maui/issues/20553");
#endif
			App.WaitForElement(ButtonId);
			App.Click(ButtonId);
			App.WaitForElement(ButtonId);
		}
	}
}