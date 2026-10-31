using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21711 : _IssuesUITest
	{
		public Issue21711(TestDevice device) : base(device)
		{
		}

		public override string Issue => "NullReferenceException from FlexLayout.InitItemProperties";

		[Test]
		[ShardedTestCategory(UITestCategories.Layout, shard: 3)]
		public void AddDoesNotCrash()
		{
			App.WaitForElement("Add");

			App.Click("Add");

			App.WaitForElement("Item2");
			App.WaitForElement("Item3");
		}

		[Test]
		[ShardedTestCategory(UITestCategories.Layout, shard: 1)]
		public void InsertDoesNotCrash()
		{
			App.WaitForElement("Insert");

			App.Click("Insert");

			App.WaitForElement("Item2");
			App.WaitForElement("Item3");
		}

		[Test]
		[ShardedTestCategory(UITestCategories.Layout, shard: 1)]
		public void UpdateDoesNotCrash()
		{
			App.WaitForElement("Update");

			App.Click("Update");

			App.WaitForElement("Item3");
		}

		[Test]
		[ShardedTestCategory(UITestCategories.Layout, shard: 2)]
		public void RemoveDoesNotCrash()
		{
			App.WaitForElement("Remove");

			App.Click("Remove");

			App.WaitForElement("Item2");
		}
	}
}
