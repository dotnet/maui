﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues
{
	internal class Issue25551 : _IssuesUITest
	{
		public override string Issue => "CollectionView SelectedItem binding issue on initial loading";

		public Issue25551(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		
		public void CollectionViewPreSelectionShouldUpdate()
		{
			App.WaitForElement("SingleSelection");
			VerifyScreenshot();
			App.WaitForElement("MultipleSelection");
			VerifyScreenshot();
		}
	}
}
