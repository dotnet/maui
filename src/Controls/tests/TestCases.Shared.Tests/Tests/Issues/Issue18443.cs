#if !MACCATALYST && !IOS
using NUnit.Framework;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18443 : _IssuesUITest
	{
		public Issue18443(TestDevice device) : base(device) { }

		public override string Issue => "[Android] SelectionLength Property Not Applied to Entry at Runtime";

		[Test]
		[Category(UITestCategories.Entry)]
		public async Task EntrySelectionLengthRuntimeUpdate()
		{
			await Task.Delay(500);
			VerifyScreenshot();
		}

	}
}
#endif