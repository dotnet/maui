#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20264 : _IssuesUITest
	{
		public Issue20264(TestDevice device) : base(device)
		{ }

		public override string Issue => "Android does not update visibility of a visual element if it has a Shadow";

		[Test]
		public void Issue20264Test()
		{
			App.WaitForElement("button");
			App.Click("button");
			App.WaitForElement("label");
		}
	}
}
#endif