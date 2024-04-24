﻿#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19803 : _IssuesUITest
	{
		public Issue19803(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[iOS] Setting Binding on Span GridItemsLayout results in NullReferenceException";

		[Test]
		public void NoNREWhenChangingGridItemsLayout()
		{
			_ = App.WaitForElement("button");

			//The test passes if no NullReferenceException is thrown
			App.Click("button");

		}
	}
}
#endif
