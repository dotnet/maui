using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
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

		string GetNativeText(AView view)
		{
			return (view as TextView).Text;
		}

		async Task AssertZIndexOrder(IReadOnlyList<AView> children)
		{
			// Lots of ways we could compare the two lists, but dumping them both to comma-separated strings
			// makes it easy to give the test useful output

			string expected = await InvokeOnMainThreadAsync(() =>
			{
				return children.OrderBy(nativeView => GetNativeText(nativeView))
					.Aggregate("", (str, nativeView) => str + (str.Length > 0 ? ", " : "") + GetNativeText(nativeView));
			});

			string actual = await InvokeOnMainThreadAsync(() =>
			{
				return children.Aggregate("", (str, nativeView) => str + (str.Length > 0 ? ", " : "") + GetNativeText(nativeView));
			});

			Assert.Equal(expected, actual);
		}
	}
}
