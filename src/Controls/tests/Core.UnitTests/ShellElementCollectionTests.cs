using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellElementCollection : ShellTestBase
	{
		[Fact]
		public void ClearFiresOnlyOneRemovedEvent()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem());
			var shellSection = shell.CurrentItem.CurrentItem;

			int firedCount = 0;

			(shellSection as IShellSectionController).ItemsCollectionChanged += (_, e) =>
			{
				if (e.OldItems != null)
					firedCount++;
			};

			shellSection.Items.Clear();
			Assert.Equal(1, firedCount);
		}

		// Regression tests for https://github.com/dotnet/maui/issues/28078
		// Shell crashes with IllegalArgumentException when tapping flyout after dynamically replacing items.
		// Fix: ShellElementCollection tracks _isItemsCleared and calls NotifyFlyoutBehaviorObservers()
		// when the item count reaches 2 after a clear, ensuring platform handlers update their flyout state.

		[Fact]
		public void FlyoutBehaviorObserverNotifiedWhenItemsReplacedWithMultiple()
		{
			// Arrange: Shell starts with a single implicit item (flyout disabled)
			var shell = new Shell();
			shell.Items.Add(CreateShellItem(asImplicit: true));

			var notifiedBehaviors = new List<FlyoutBehavior>();
			var observer = new TestFlyoutBehaviorObserver(b => notifiedBehaviors.Add(b));
			(shell as IShellController).AddFlyoutBehaviorObserver(observer);
			notifiedBehaviors.Clear(); // Ignore initial notification from AddFlyoutBehaviorObserver

			// Act: Simulate login flow — clear single item and replace with multiple items
			shell.Items.Clear();
			shell.Items.Add(CreateShellItem(asImplicit: true));
			shell.Items.Add(CreateShellItem(asImplicit: true));

			// Assert: Observer was notified with Flyout behavior when second item was added
			Assert.Contains(FlyoutBehavior.Flyout, notifiedBehaviors);
		}

		[Fact]
		public void FlyoutBehaviorObserverNotifiedExactlyOnceWhenMoreThanTwoItemsAddedAfterClear()
		{
			// Arrange: Shell starts with a single implicit item (flyout disabled)
			var shell = new Shell();
			shell.Items.Add(CreateShellItem(asImplicit: true));

			int flyoutNotificationCount = 0;
			var observer = new TestFlyoutBehaviorObserver(b =>
			{
				if (b == FlyoutBehavior.Flyout)
					flyoutNotificationCount++;
			});
			(shell as IShellController).AddFlyoutBehaviorObserver(observer);
			flyoutNotificationCount = 0; // Ignore initial notification from AddFlyoutBehaviorObserver

			// Act: Clear and add 3 items
			shell.Items.Clear();
			shell.Items.Add(CreateShellItem(asImplicit: true));
			shell.Items.Add(CreateShellItem(asImplicit: true)); // Notification fires here (count==2)
			shell.Items.Add(CreateShellItem(asImplicit: true)); // Should NOT fire again (_isItemsCleared already reset)

			// Assert: Observer notified with Flyout exactly once — at count==2, not again at count==3
			Assert.Equal(1, flyoutNotificationCount);
		}

		class TestFlyoutBehaviorObserver : IFlyoutBehaviorObserver
		{
			readonly Action<FlyoutBehavior> _onChanged;

			public TestFlyoutBehaviorObserver(Action<FlyoutBehavior> onChanged)
			{
				_onChanged = onChanged;
			}

			public void OnFlyoutBehaviorChanged(FlyoutBehavior behavior) => _onChanged(behavior);
		}
	}
}
