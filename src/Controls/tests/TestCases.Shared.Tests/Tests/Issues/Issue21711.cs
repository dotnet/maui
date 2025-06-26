﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Trait("Category", UITestCategories.Layout)]
	public class Issue21711 : _IssuesUITest
	{
		public Issue21711(TestDevice device) : base(device)
		{
		}

		public override string Issue => "NullReferenceException from FlexLayout.InitItemProperties";

		[Fact]
		public void AddDoesNotCrash()
		{
			App.WaitForElement("Add");

			App.Click("Add");

			App.WaitForElement("Item2");
			App.WaitForElement("Item3");
		}

		[Fact]
		public void InsertDoesNotCrash()
		{
			App.WaitForElement("Insert");

			App.Click("Insert");

			App.WaitForElement("Item2");
			App.WaitForElement("Item3");
		}

		[Fact]
		public void UpdateDoesNotCrash()
		{
			App.WaitForElement("Update");

			App.Click("Update");

			App.WaitForElement("Item3");
		}

		[Fact]
		public void RemoveDoesNotCrash()
		{
			App.WaitForElement("Remove");

			App.Click("Remove");

			App.WaitForElement("Item2");
		}
	}
}
