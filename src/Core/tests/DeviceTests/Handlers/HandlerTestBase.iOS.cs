using System;
using System.Threading.Tasks;
using CoreAnimation;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
	{
		[Fact(DisplayName = "Transformation Initialize Correctly")]
		public async Task TransformationInitializeCorrectly()
		{
			var view = new TStub()
			{
				TranslationX = 10,
				TranslationY = 10,
				Scale = 1.2,
				Rotation = 90
			};

			var handler = await CreateHandlerAsync(view);
			var nativeView = (UIView)handler.NativeView;

			var transform = nativeView.Layer.Transform;

			Assert.NotEqual(CATransform3D.Identity, transform);
		}

		protected THandler CreateHandler(IView view)
		{
			var handler = Activator.CreateInstance<THandler>();
			handler.SetMauiContext(MauiContext);

			handler.SetVirtualView(view);
			view.Handler = handler;

			return handler;
		}

		protected string GetAutomationId(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityIdentifier;

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityLabel;

		protected string GetSemanticHint(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityHint;

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityTraits.HasFlag(UIAccessibilityTrait.Header)
				? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;
	}
}