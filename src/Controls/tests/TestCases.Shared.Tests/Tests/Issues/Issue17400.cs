﻿#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17400 : _IssuesUITest
	{
		public Issue17400(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "CollectionView wrong Layout";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue17400Test()
		{
			// Is a Windows issue; see https://github.com/dotnet/maui/issues/17400

			App.WaitForElement("UpdateBtn");
			App.Tap("UpdateBtn");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
#endif