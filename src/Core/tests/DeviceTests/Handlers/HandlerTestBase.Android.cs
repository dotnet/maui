using System;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
	{
		protected THandler CreateHandler(IView view)
		{
			var handler = Activator.CreateInstance<THandler>();
			handler.SetMauiContext(MauiContext);

			handler.SetVirtualView(view);
			view.Handler = handler;

			return handler;
		}

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
	}
}