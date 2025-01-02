using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
using System.Threading.Tasks;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25829 : _IssuesUITest
	{
		public Issue25829(TestDevice device) : base(device) { }

		public override string Issue => "ScrollView starts at the position of first Entry control on the bottom rather than at 0";

		[Test]
		[Category(UITestCategories.Entry)]
		public void ScrollViewStartsOccasionallyStartsAtTheFirstEntry()
		{
			App.WaitForElement("PushModal");
			App.Tap("PushModal");
			App.WaitForElement("Success");
			App.Tap("Success");
		}
	}
}