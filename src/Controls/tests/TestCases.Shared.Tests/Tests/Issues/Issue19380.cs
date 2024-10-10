using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19380 : _IssuesUITest
	{
		public override string Issue => "Switch control ignores OnColor and ThumbColor";

		public Issue19380(TestDevice device) : base(device)
		{
		}	

		[Test]
		public void ShouldOverrideThumbAndOnColorsFromResources()
		{
			_= App.WaitForElement("switch1");

			App.Click("switch1");
			App.Click("switch2");

			// The first switch should have a blue thumb and a red track
			// The second switch should have a white thumb an a green track
			App.Screenshot();
		}
	}
}
