using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
	{
		protected string GetAutomationId(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityIdentifier;

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityLabel;

		protected string GetSemanticHint(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityHint;

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityTraits.HasFlag(UIAccessibilityTrait.Header)
				? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;

		protected Visibility GetVisibility(IViewHandler viewHandler)
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