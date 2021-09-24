using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{

		[Fact(DisplayName = "Shadow Initializes Correctly")]
		public async Task ShadowInitializesCorrectly()
		{
			var xPlatShadow = new ShadowStub
			{
				Offset = new Point(10, 10),
				Opacity = 1.0f,
				Radius = 2.0f
			};

			var layout = new LayoutStub
			{
				Height = 50,
				Width = 50
			};

			layout.Shadow = xPlatShadow;

			await ValidateHasColor(layout, Colors.Red, () => xPlatShadow.Paint = new SolidPaint(Colors.Red));
		}

		double GetNativeChildCount(IElementHandler layoutHandler)
		{
			return GetNativeChildCount(layoutHandler.NativeView as LayoutViewGroup);
		}

		double GetNativeChildCount(object nativeView)
		{
			return (nativeView as LayoutViewGroup).ChildCount;
		}

		IReadOnlyList<AView> GetNativeChildren(LayoutHandler layoutHandler)
		{
			var views = new List<AView>();

			for (int i = 0; i < layoutHandler.NativeView.ChildCount; i++)
			{
				views.Add(layoutHandler.NativeView.GetChildAt(i));
			}

			return views;
		}

		LayoutViewGroup GetNativeLayout(LayoutHandler layoutHandler)
		{
			return layoutHandler.NativeView;
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
