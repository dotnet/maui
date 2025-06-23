using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17865 : _IssuesUITest
	{
		const string ButtonId = "WaitForStubControl";

		public Issue17865(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView throws NRE when ScrollTo method is called from a handler of event Window.Created";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue17865Test()
		{
			App.WaitForElement(ButtonId);
			App.Click(ButtonId);
			App.WaitForElement(ButtonId);

			// NOTE: Without crashes the test has passed.
		}
	}
}