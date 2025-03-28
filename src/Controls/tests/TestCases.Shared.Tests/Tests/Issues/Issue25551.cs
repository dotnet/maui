using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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
			Exception? exception = null;
			
			App.WaitForElement("SingleSelection");
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_SingleSelection");
			App.WaitForElement("MultipleSelection");
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_MultipleSelection");
			
			if (exception != null)
			{
				throw exception;
			}
		}
	}
}
