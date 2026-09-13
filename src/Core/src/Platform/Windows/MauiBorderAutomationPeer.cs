using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Provides automation peer behavior for a .NET MAUI border on Windows.
	/// </summary>
	public partial class MauiBorderAutomationPeer : FrameworkElementAutomationPeer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MauiBorderAutomationPeer"/> class.
		/// </summary>
		/// <param name="owner">The content panel associated with the border.</param>
		public MauiBorderAutomationPeer(ContentPanel owner) : base(owner) { }

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
	}
}
