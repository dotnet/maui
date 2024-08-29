# if !MACCATALYST
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22528 : _IssuesUITest
	{
		public Issue22528(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Prevent the label text from being cut off from the top when the specified LineHeight";

		[Test]
		[Category(UITestCategories.Label)]
		[FailsOnAndroid("Currently fails on Android; see https://github.com/dotnet/maui/issues/24504")]
		public void PreventLabelTextCrop()
		{
			App.WaitForElement("label");

			VerifyScreenshot();
		}
	}
}
#endif
