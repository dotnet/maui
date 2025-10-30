﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21708 : _IssuesUITest
{
	public Issue21708(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView.Scrolled event offset isn't correctly reset when items change";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCollectionViewVerticalOffset()
	{
		App.WaitForElement("Fill");
		App.ScrollDown("CollectionView");
		Assert.That(App.FindElement("Label").GetText(), Is.GreaterThan("0"));
		App.Tap("Empty");
		Assert.That(App.FindElement("Label").GetText(), Is.EqualTo("0"));
	}
}