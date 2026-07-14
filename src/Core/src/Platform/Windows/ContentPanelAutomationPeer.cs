using System.Collections.Generic;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;

namespace Microsoft.Maui.Platform
{
	internal partial class ContentPanelAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
	{
		internal ContentPanelAutomationPeer(ContentPanel owner) : base(owner) { }

		bool HasDescription => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(Owner));

		protected override AutomationControlType GetAutomationControlTypeCore() =>
			HasDescription ? AutomationControlType.Text : AutomationControlType.Custom;

		protected override string GetLocalizedControlTypeCore() =>
			HasDescription ? string.Empty : base.GetLocalizedControlTypeCore() ?? string.Empty;

		// Suppress children only when this container wraps nothing (avoids duplicate announcement
		// for the common single-child wrapper case). A container with real content (e.g. a
		// BindableLayout of independently-focusable items) keeps its children reachable — see #33612.
		protected override IList<AutomationPeer>? GetChildrenCore() =>
			HasDescription && Owner is ContentPanel { CrossPlatformLayout: IContentView { PresentedContent: null } }
				? null
				: base.GetChildrenCore();

		// Expose the Invoke pattern only when GesturePlatformManager has wired an activate callback
		// (i.e. this element's only interaction is a TapGestureRecognizer). This is what makes
		// Narrator announce "double tap to activate" and lets Enter/Narrator-activate fire the tap.
		protected override object? GetPatternCore(PatternInterface patternInterface) =>
			patternInterface == PatternInterface.Invoke && Owner is ContentPanel { AutomationActivateCallback: not null }
				? this
				: base.GetPatternCore(patternInterface);

		// IInvokeProvider
		public void Invoke() => (Owner as ContentPanel)?.AutomationActivateCallback?.Invoke();
	}
}
