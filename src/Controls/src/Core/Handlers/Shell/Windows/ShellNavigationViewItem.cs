#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	public partial class ShellNavigationViewItem : NavigationViewItem
	{
		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new ShellNavigationViewItemAutomationPeer(this);
		}
	}

	public partial class ShellNavigationViewItemAutomationPeer : NavigationViewItemAutomationPeer
	{
		readonly ShellNavigationViewItem _owner;

		public ShellNavigationViewItemAutomationPeer(ShellNavigationViewItem owner) : base(owner)
		{
			_owner = owner;
		}

		protected override AutomationPeer GetLabeledByCore()
		{
			if (_owner.Content is ShellFlyoutItemView sf && sf.Content is FrameworkElement fe)
				return FrameworkElementAutomationPeer.FromElement(fe);

			return null;
		}

		protected override IList<AutomationPeer> GetChildrenCore()
		{
			return null;
		}
	}
}
