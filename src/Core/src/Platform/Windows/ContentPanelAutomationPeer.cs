using System.Collections.Generic;
using Microsoft.UI.Xaml.Automation.Peers;

namespace Microsoft.Maui.Platform
{
	public partial class ContentPanelAutomationPeer : FrameworkElementAutomationPeer
	{
		public ContentPanelAutomationPeer(ContentPanel owner) : base(owner)
		{
		}

		protected override string GetClassNameCore() => nameof(ContentPanel);

		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			// Return Group as the control type for a content panel, which is appropriate for containers
			return AutomationControlType.Group;
		}

		protected override IList<AutomationPeer>? GetChildrenCore()
		{
			// Return null to suppress child automation peers, similar to MauiButtonAutomationPeer
			// This prevents nested controls from being announced separately
			return null;
		}

		protected override bool IsControlElementCore()
		{
			// Make the panel appear in the control view of the automation tree
			// This allows it to be keyboard navigable
			return true;
		}

		protected override bool IsKeyboardFocusableCore()
		{
			// Allow keyboard focus when semantic properties are set
			var owner = Owner as ContentPanel;
			if (owner?.CrossPlatformLayout is IView view)
			{
				var semantics = view.Semantics;
				return semantics != null && !string.IsNullOrEmpty(semantics.Description);
			}
			return false;
		}
	}
}
