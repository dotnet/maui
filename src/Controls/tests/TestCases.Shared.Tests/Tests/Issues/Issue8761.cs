﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8761 : _IssuesUITest
{
	public override string Issue => "CollectionView Header Template and Footer Template don't work";

	public Issue8761(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	[Ignore("This test is very flaky and needs to be fixed. See https://github.com/dotnet/maui/issues/27272")]
	public void CollectionViewHeaderTemplateAndFooterTemplateDontWork()
	{
		for (int i = 0; i < 4; i++)
		{
			App.WaitForElement("AddItem");
			App.Tap("AddItem");
			App.WaitForElement("FooterLabel");
		}
	}
}