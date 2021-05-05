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
			var alpha = nativeView.Alpha;

			if (alpha == 0)
				return Visibility.Hidden;
			else
			{
				if (nativeView.Hidden)
					return Visibility.Collapsed;

				return Visibility.Visible;
			}
		}
	}
}