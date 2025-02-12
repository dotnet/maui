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
	public class Issue20274 : _IssuesUITest
	{
		public override string Issue => "IsEnabled=False on CollectionView not working";

		public Issue20274(TestDevice device) : base(device)
		{
		}


		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyCollectionViewIsEnableState()
		{
			App.WaitForElement("Button");
			App.Tap("ItemsLabel");
			Assert.That(App.FindElement("Label").GetText(), Is.EqualTo("Item 1"));
			App.Tap("Button");
			App.Tap("ItemsLabel");
			Assert.That(App.FindElement("Label").GetText(), Is.EqualTo("Item 1 - Selected"));
		}
	}
}