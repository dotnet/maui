using Microsoft.Maui.TestCases.Tests;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public partial class ShellTransientTests : _IssuesUITest
	{
		public ShellTransientTests(TestDevice device) : base(device) { }

		public override string Issue => "Validate Basic Service Lifetime Behavior On Shell";

		protected override bool ResetAfterEachTest => true;

		[Test]
		[Category(UITestCategories.Shell)]
		public void ValidateBasicServiceLifetimePageBehavior()
		{
			// Navigate to Transient Page for the First time
			App.WaitForElement("NewPage");

			// Navigate to Unregistered Page
			App.WaitForElement("NavigateToUnregisteredPage");
			App.Tap("NavigateToUnregisteredPage");
			App.WaitForElement("NewPage", "New Page Not Created For Initial Navigation to Unregistered Page");

			// Navigate to Scoped Page for the First time
			App.WaitForElement("NavigateToScopedPage");
			App.Tap("NavigateToScopedPage");
			App.WaitForElement("NewPage", "New Page Not Created For Initial Navigation to Scoped Page");

			// Navigate to Transient Page for the Second time
			App.WaitForElement("NavigateToTransientPage");
			App.Tap("NavigateToTransientPage");
			App.WaitForElement("NewPage", "New Page Not Created For Second Navigation To Transient Page");

			// Navigate to Transient Page for the Second time
			App.WaitForElement("NavigateToScopedPage");
			App.Tap("NavigateToScopedPage");
			App.WaitForElement("OldPage", "New Page Incorrectly Created For Scoped Page");

			// Navigate to Unregistered Page
			App.WaitForElement("NavigateToUnregisteredPage");
			App.Tap("NavigateToUnregisteredPage");
			App.WaitForElement("OldPage", "New Page Incorrectly Created For Unregistered Page");
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void SwappingShellInstancesRecreatesPages()
		{
			// Navigate to Scoped Page so we can test that it's resued in Swapped Shell
			App.WaitForElement("NavigateToScopedPage");
			App.Tap("NavigateToScopedPage");
			App.WaitForElement("NewPage", "New Page Not Created For Initial Navigation to Scoped Page");

			// Verify New Page Created for Transient Page
			App.WaitForElement("NewPage");
			App.Tap("NavigateToNewShell");
			App.WaitForElement("NewPage");

			// Navigate to Scoped Page on Second Shell
			App.WaitForElement("NavigateToScopedPage");
			App.Tap("NavigateToScopedPage");
			App.WaitForElement("OldPage", "New Page Incorrectly Created For Scoped Page");
		}
	}
}