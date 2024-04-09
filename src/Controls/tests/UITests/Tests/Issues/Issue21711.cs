using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21711 : _IssuesUITest
	{
		public Issue21711(TestDevice device) : base(device)
		{
		}

		public override string Issue => "NullReferenceException from FlexLayout.InitItemProperties";

		[Test]
		public void AddDoesNotCrash()
		{
			App.WaitForElement("Add");

			App.Click("Add");

			App.WaitForElement("Item 2");
			App.WaitForElement("Item 3");
		}

		[Test]
		public void InsertDoesNotCrash()
		{
			App.WaitForElement("Insert");

			App.Click("Insert");

			App.WaitForElement("Item 2");
			App.WaitForElement("Item 3");
		}

		[Test]
		public void UpdateDoesNotCrash()
		{
			App.WaitForElement("Update");

			App.Click("Update");

			App.WaitForElement("Item 3");
		}

		[Test]
		public void RemoveDoesNotCrash()
		{
			App.WaitForElement("Remove");

			App.Click("Remove");

			App.WaitForElement("Item 2");
		}
	}
}
