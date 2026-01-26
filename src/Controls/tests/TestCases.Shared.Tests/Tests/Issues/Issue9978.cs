using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue9978 : _IssuesUITest
{
	public Issue9978(TestDevice device) : base(device)
	{
	}

	public override string Issue => "VisualElement.HeightRequest defaults to 0 instead of -1 when using OnIdiom default value";

    
    [Test]
	[Category(UITestCategories.Layout)]
	public void Issue9978Test()
	{
		App.WaitForElement("MauiImage");
		
	}
}
