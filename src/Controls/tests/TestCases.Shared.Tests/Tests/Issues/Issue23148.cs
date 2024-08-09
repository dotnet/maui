using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23148 : _IssuesUITest
	{
		public override string Issue => "Incorrect height of CollectionView when ItemsSource is empty";

		public Issue23148(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]

		public void CollectionViewEmptyItemsHeight()
		{
			// Is a Android issue; see https://github.com/dotnet/maui/issues/23148

			App.WaitForElement("StackLayout");
			VerifyScreenshot();
		}
	}
}
