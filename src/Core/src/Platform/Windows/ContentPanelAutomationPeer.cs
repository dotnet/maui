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

		// Suppress children when this container wraps nothing (Guard 1 — avoids duplicate
		// announcement for the common empty wrapper case), or when it owns its own tap gesture,
		// i.e. is itself the actionable unit (Guard 2 — mirrors iOS SemanticExtensions.cs). Without
		// Guard 2, a tappable ContentView with a single child (e.g. a bound Label) would never
		// collapse, so Narrator's cursor lands on the child instead of this peer and never reads its
		// HelpText ("Double tap to activate") or discovers its IInvokeProvider. A container with real,
		// non-actionable content (e.g. a BindableLayout of independently-focusable items) still keeps
		// its children reachable — see #33612.
		protected override IList<AutomationPeer>? GetChildrenCore()
		{
			if (Owner is not ContentPanel panel)
			{
				return base.GetChildrenCore();
			}

			if (panel.AutomationActivateCallback is not null)
			{
				return null;
			}

			return HasDescription && panel is { CrossPlatformLayout: IContentView { PresentedContent: null } }
				? null
				: base.GetChildrenCore();
		}

		// Expose the Invoke pattern only when GesturePlatformManager has wired an activate callback
		// (i.e. this element's only interaction is a TapGestureRecognizer). This is what makes
		// Narrator announce "double tap to activate" and lets Enter/Narrator-activate fire the tap.
		protected override object? GetPatternCore(PatternInterface patternInterface) =>
			patternInterface == PatternInterface.Invoke && Owner is ContentPanel { AutomationActivateCallback: not null }
				? this
				: base.GetPatternCore(patternInterface);

		// ContentPanel derives from Panel, not Control, so it has no native Tab-stop/keyboard focus.
		// Report this peer as keyboard-focusable whenever it's activatable so Narrator's Enter-key
		// activation routes here instead of being swallowed (mirrors IsTabStop set on ContentPanel
		// when AutomationActivateCallback is assigned — see ContentPanel.AutomationActivateCallback).
		protected override bool IsKeyboardFocusableCore() =>
			(Owner as ContentPanel)?.AutomationActivateCallback is not null || base.IsKeyboardFocusableCore();

		protected override void SetFocusCore() => Owner?.Focus(global::Microsoft.UI.Xaml.FocusState.Programmatic);

		// IInvokeProvider
		public void Invoke() => (Owner as ContentPanel)?.AutomationActivateCallback?.Invoke();
	}
}
