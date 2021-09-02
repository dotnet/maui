using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		[Fact(DisplayName = "Shadow Initializes Correctly")]
		public async Task ShadowInitializesCorrectly()
		{
			var xPlatShadow = new Shadow
			{
				Offset = new Size(10, 10),
				Opacity = 1.0f,
				Radius = 2.0f
			};

			var layout = new LayoutStub
			{
				Height = 50,
				Width = 50
			};

			layout.Shadow = xPlatShadow;

			await ValidateHasColor(layout, Colors.Red, () => xPlatShadow.Color = Colors.Red);
		}

		LayoutView GetNativeLayout(LayoutHandler layoutHandler)
		{
			return layoutHandler.NativeView;
		}

		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return layoutHandler.NativeView.Subviews.Length;
		}

		double GetNativeChildCount(object nativeView)
		{
			return (nativeView as UIView).Subviews.Length;
		}

		IReadOnlyList<UIView> GetNativeChildren(LayoutHandler layoutHandler)
		{
			return layoutHandler.NativeView.Subviews;
		}

		Task ValidateHasColor(ILayout layout, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeLayout = GetNativeLayout(CreateHandler(layout));
				action?.Invoke();
				nativeLayout.AssertContainsColor(color);
			});
		}
	}
}