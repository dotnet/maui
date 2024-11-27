﻿#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20294 : _IssuesUITest
{
	public Issue20294(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView containing a Footer and a Border with StrokeThickness set to decimal value crashes on scroll";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ScrollToEndDoesntCrash()
	{
		App.ScrollTo("FOOTER");
		App.ScrollUp("theCollectionView", ScrollStrategy.Gesture, 0.5);
		App.ScrollDown("theCollectionView", ScrollStrategy.Gesture, 0.5);
		App.ScrollDown("theCollectionView", ScrollStrategy.Gesture, 0.5);
		App.ScrollUp("theCollectionView", ScrollStrategy.Gesture, 0.5);
		App.ScrollDown("theCollectionView", ScrollStrategy.Gesture, 0.5);
	}
}
#endif