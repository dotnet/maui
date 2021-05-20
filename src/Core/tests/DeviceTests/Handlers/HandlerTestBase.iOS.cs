using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
	{
		protected string GetAutomationId(IFrameworkElementHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityIdentifier;

		protected string GetSemanticDescription(IFrameworkElementHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityLabel;

		protected string GetSemanticHint(IFrameworkElementHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityHint;

		protected SemanticHeadingLevel GetSemanticHeading(IFrameworkElementHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityTraits.HasFlag(UIAccessibilityTrait.Header)
				? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;

		protected Visibility GetVisibility(IFrameworkElementHandler viewHandler)
		{
			var nativeView = (UIView)viewHandler.NativeView;

			foreach (var constraint in nativeView.Constraints)
			{
				if (constraint is CollapseConstraint collapseConstraint)
				{
					// Active the collapse constraint; that will squish the view down to zero height
					if (collapseConstraint.Active)
					{
						return Visibility.Collapsed;
					}
				}
			}

			if (nativeView.Hidden)
			{
				return Visibility.Hidden;
			}

			return Visibility.Visible;
		}
	}
}