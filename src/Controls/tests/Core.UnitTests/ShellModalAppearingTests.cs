using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ShellModalAppearingTests : ShellTestBase
	{
		[Fact]
		public async Task ModalPageOnAppearingTriggeredOnceWithShellItemChange()
		{
			// Create shell with two ShellItems
			TestShell shell = new TestShell();

			// Register modal page route
			Routing.RegisterRoute("ModalPage", typeof(ModalPage31584));

			// Add two shell items
			shell.Items.Add(CreateShellItem(shellItemRoute: "MainPage", shellSectionRoute: "MainSection", shellContentRoute: "MainContent"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "Home", shellSectionRoute: "HomeSection", shellContentRoute: "HomeContent"));

			// Ensure we're on the MainPage ShellItem initially
			Assert.Equal("MainPage", shell.CurrentItem.Route);

			// Navigate to modal page via different ShellItem (//Home/ModalPage)
			await shell.GoToAsync("//Home/ModalPage");

			// Get the modal page
			var modalPage = (shell.Items[1].Items[0] as IShellSectionController).PresentedPage as ModalPage31584;
			Assert.NotNull(modalPage);

			// Verify OnAppearing was called exactly once
			Assert.Equal(1, modalPage.AppearingCount);
		}

		[Fact]
		public async Task ModalPageOnAppearingTriggeredOnceWithShellItemChangeToModal()
		{
			// Test the exact scenario from the issue: navigating from one ShellItem to another with a modal page
			TestShell shell = new TestShell();

			Routing.RegisterRoute("ModalPage", typeof(ModalPage31584));

			var mainItem = CreateShellItem(shellItemRoute: "MainPage", shellSectionRoute: "MainSection", shellContentRoute: "MainContent");
			var homeItem = CreateShellItem(shellItemRoute: "Home", shellSectionRoute: "HomeSection", shellContentRoute: "HomeContent");

			shell.Items.Add(mainItem);
			shell.Items.Add(homeItem);

			// Start on MainPage
			Assert.Equal(mainItem, shell.CurrentItem);

			// Navigate to //Home/ModalPage (changes ShellItem AND pushes modal)
			await shell.GoToAsync("//Home/ModalPage");

			// Verify we switched to Home ShellItem
			Assert.Equal(homeItem, shell.CurrentItem);

			// Get the modal page from the Home ShellItem's section
			var homeSection = homeItem.Items[0];
			var modalPage = (homeSection as IShellSectionController).PresentedPage as ModalPage31584;

			Assert.NotNull(modalPage);

			// The bug would cause OnAppearing to be called twice:
			// 1. When the modal is pushed to the new ShellItem
			// 2. When the old ShellItem's section tries to send disappearing to it
			// The fix ensures OnAppearing is only called once
			Assert.Equal(1, modalPage.AppearingCount);
		}

		[Fact]
		public async Task ModalPageDisappearingNotCalledByOldShellItemDuringSwitch()
		{
			// This test verifies the core fix: when switching ShellItems with a modal page,
			// the old ShellItem's section should NOT call SendDisappearing on the modal page
			// because the modal belongs to the new ShellItem, not the old one
			TestShell shell = new TestShell();

			Routing.RegisterRoute("ModalPage", typeof(ModalPage31584));

			var mainItem = CreateShellItem(shellItemRoute: "MainPage", shellSectionRoute: "MainSection", shellContentRoute: "MainContent");
			var homeItem = CreateShellItem(shellItemRoute: "Home", shellSectionRoute: "HomeSection", shellContentRoute: "HomeContent");

			shell.Items.Add(mainItem);
			shell.Items.Add(homeItem);

			// Navigate to //Home/ModalPage
			await shell.GoToAsync("//Home/ModalPage");

			var homeSection = homeItem.Items[0];
			var modalPage = (homeSection as IShellSectionController).PresentedPage as ModalPage31584;

			Assert.NotNull(modalPage);

			// The modal should have appeared once
			Assert.Equal(1, modalPage.AppearingCount);

			// And should not have disappeared (because it's still visible)
			Assert.Equal(0, modalPage.DisappearingCount);
		}

		[Fact]
		public async Task ModalPageLifecycleCorrectWithNormalModalPush()
		{
			// Verify that normal modal push/pop still works correctly after the fix
			TestShell shell = new TestShell();

			Routing.RegisterRoute("ModalPage", typeof(ModalPage31584));

			shell.Items.Add(CreateShellItem(shellItemRoute: "MainPage", shellSectionRoute: "MainSection", shellContentRoute: "MainContent"));

			// Normal modal navigation (no ShellItem change)
			await shell.GoToAsync("ModalPage");

			var mainSection = shell.Items[0].Items[0];
			var modalPage = (mainSection as IShellSectionController).PresentedPage as ModalPage31584;

			Assert.NotNull(modalPage);
			Assert.Equal(1, modalPage.AppearingCount);
			Assert.Equal(0, modalPage.DisappearingCount);

			// Pop the modal
			await shell.GoToAsync("..");

			// Modal should have disappeared once
			Assert.Equal(1, modalPage.AppearingCount);
			Assert.Equal(1, modalPage.DisappearingCount);
		}

		[Fact]
		public async Task RapidShellItemSwitchingWithModal()
		{
			// Test rapidly switching between ShellItems with modals to ensure lifecycle stays correct
			TestShell shell = new TestShell();

			Routing.RegisterRoute("ModalPage1", typeof(ModalPage31584));
			Routing.RegisterRoute("ModalPage2", typeof(ModalPage31584));

			shell.Items.Add(CreateShellItem(shellItemRoute: "Item1", shellSectionRoute: "Section1", shellContentRoute: "Content1"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "Item2", shellSectionRoute: "Section2", shellContentRoute: "Content2"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "Item3", shellSectionRoute: "Section3", shellContentRoute: "Content3"));

			// Navigate to modal on Item2
			await shell.GoToAsync("//Item2/ModalPage1");
			var section2 = shell.Items[1].Items[0];
			var modal1 = (section2 as IShellSectionController).PresentedPage as ModalPage31584;

			Assert.NotNull(modal1);
			Assert.Equal(1, modal1.AppearingCount);

			// Switch to Item3 with another modal
			await shell.GoToAsync("//Item3/ModalPage2");
			var section3 = shell.Items[2].Items[0];
			var modal2 = (section3 as IShellSectionController).PresentedPage as ModalPage31584;

			Assert.NotNull(modal2);
			// Second modal should have appeared exactly once (the bug would cause it to appear twice)
			Assert.Equal(1, modal2.AppearingCount);
			Assert.Equal(0, modal2.DisappearingCount);
		}

		public class ModalPage31584 : ContentPage
		{
			public int AppearingCount { get; private set; }
			public int DisappearingCount { get; private set; }

			public ModalPage31584()
			{
				Shell.SetPresentationMode(this, PresentationMode.Modal);
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				AppearingCount++;
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();
				DisappearingCount++;
			}
		}

		public ShellModalAppearingTests()
		{
			// Routes are already registered in the test methods as needed
		}
	}
}
