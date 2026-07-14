using System.Collections.Generic;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;

namespace Microsoft.Maui.Platform
{
	internal partial class ContentPanelAutomationPeer : FrameworkElementAutomationPeer
	{
		internal ContentPanelAutomationPeer(ContentPanel owner) : base(owner) { }

		bool HasDescription => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(Owner));

		protected override AutomationControlType GetAutomationControlTypeCore() =>
			HasDescription ? AutomationControlType.Text : AutomationControlType.Custom;

		protected override string GetLocalizedControlTypeCore() =>
			HasDescription ? string.Empty : base.GetLocalizedControlTypeCore() ?? string.Empty;

		protected override IList<AutomationPeer>? GetChildrenCore() =>
			HasDescription ? null : base.GetChildrenCore();
	}
}
