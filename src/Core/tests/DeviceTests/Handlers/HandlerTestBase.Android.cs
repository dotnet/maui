using Android.Views;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
	{
		protected string GetAutomationId(IViewHandler viewHandler) =>
			$"{((View)viewHandler.NativeView).GetTag(ViewExtensions.AutomationTagId)}";

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			((View)viewHandler.NativeView).ContentDescription;

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler)
		{
			// AccessibilityHeading is only available on API 28+
			// With lower Apis you use ViewCompat.SetAccessibilityHeading
			// but there exists no ViewCompat.GetAccessibilityHeading
			if (NativeVersion.IsAtLeast(28))
				return ((View)viewHandler.NativeView).AccessibilityHeading
					? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;

			return viewHandler.VirtualView.Semantics.HeadingLevel;
		}

		protected Visibility GetVisibility(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			if (nativeView.Visibility == ViewStates.Visible)
				return Visibility.Visible;
			else if (nativeView.Visibility == ViewStates.Gone)
				return Visibility.Collapsed;
			else
				return Visibility.Hidden;
		}
	}
}