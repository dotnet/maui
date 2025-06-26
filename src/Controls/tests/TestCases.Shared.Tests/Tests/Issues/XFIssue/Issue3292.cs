﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3292 : _IssuesUITest
{

	public Issue3292(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TableSection.Title property binding fails in XAML";

	[Fact]
	[Trait("Category", UITestCategories.TableView)]
	public void Issue3292Test()
	{
		App.WaitForElementTillPageNavigationSettled("Hello World Changed");
	}
}