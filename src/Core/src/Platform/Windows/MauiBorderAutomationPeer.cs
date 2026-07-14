using System.Collections.Generic;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	// TODO: Make this class public in .NET11.0. Issue Link: https://github.com/dotnet/maui/issues/30205
	internal partial class MauiBorderAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
	{
		internal MauiBorderAutomationPeer(ContentPanel owner) : base(owner) { }

		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Pane;
		}

		protected override string GetClassNameCore()
		{
			if (Owner is ContentPanel panel)
			{
				return panel.CrossPlatformLayout?.GetType().Name ?? nameof(Panel);
			}

			return nameof(Panel);
		}

		// Border is a structural container — excluded from the Control view by default so screen
		// readers don't stop on every nested Border. Opt in via AutomationProperties.IsInAccessibleTree
		// or SemanticProperties.Description / Hint. See MauiLayoutAutomationPeer for rationale.
		protected override bool IsControlElementCore()
		{
			if (Owner is not ContentPanel panel)
			{
				return false;
			}

			var accessibilityView = panel.ReadLocalValue(AutomationProperties.AccessibilityViewProperty);

			// Explicit opt-out (IsInAccessibleTree="False" -> AccessibilityView.Raw) wins over any opt-in signal.
			if (accessibilityView is AccessibilityView.Raw)
			{
				return false;
			}
			else if (accessibilityView is AccessibilityView.Control or AccessibilityView.Content)
			{
				return true;
			}

			if (!string.IsNullOrEmpty(AutomationProperties.GetName(panel)))
			{
				return true;
			}

			if (!string.IsNullOrEmpty(AutomationProperties.GetHelpText(panel)))
			{
				return true;
			}

			return false;
		}

		// Content view requires explicit opt-in via AutomationProperties.IsInAccessibleTree="True".
		// AutomationId alone is treated as a test-only hook (visible in the raw view only).
		protected override bool IsContentElementCore()
		{
			if (Owner is not ContentPanel panel)
			{
				return false;
			}

			var accessibilityView = panel.ReadLocalValue(AutomationProperties.AccessibilityViewProperty);
			return accessibilityView is AccessibilityView.Content;
		}

		// Once this Border has its own Control-view identity (Name/HelpText/explicit opt-in), treat
		// it as a leaf — same as MauiButtonAutomationPeer. Otherwise Narrator's cursor lands on the
		// inner content (e.g. a bound Label) instead of the Border, so it never reads this Border's
		// HelpText ("Double tap to activate") and never reaches its IInvokeProvider (see #33612).
		// Guard against a detached/disposed Owner (e.g. during item recycling) before calling into
		// base.GetChildrenCore(), which is not guaranteed to be null-safe in that state.
		protected override IList<AutomationPeer>? GetChildrenCore()
		{
			if (Owner is null)
			{
				return null;
			}

			return IsControlElementCore() ? null : base.GetChildrenCore();
		}

		// Expose the Invoke pattern only when GesturePlatformManager has wired an activate callback
		// (i.e. this Border's only interaction is a TapGestureRecognizer). This is what makes
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
