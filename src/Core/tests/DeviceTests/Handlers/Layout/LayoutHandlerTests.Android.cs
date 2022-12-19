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
		double GetNativeChildCount(IElementHandler layoutHandler)
		{
			return GetNativeChildCount(layoutHandler.PlatformView as LayoutViewGroup);
		}

		double GetNativeChildCount(object platformView)
		{
			return (platformView as LayoutViewGroup).ChildCount;
		}

		IReadOnlyList<AView> GetNativeChildren(LayoutHandler layoutHandler)
		{
			var views = new List<AView>();

			for (int i = 0; i < layoutHandler.PlatformView.ChildCount; i++)
			{
				views.Add(layoutHandler.PlatformView.GetChildAt(i));
			}

			return views;
		}

		LayoutViewGroup GetNativeLayout(LayoutHandler layoutHandler)
		{
			return layoutHandler.PlatformView;
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
				return children.OrderBy(platformView => GetNativeText(platformView))
					.Aggregate("", (str, platformView) => str + (str.Length > 0 ? ", " : "") + GetNativeText(platformView));
			});

			string actual = await InvokeOnMainThreadAsync(() =>
			{
				return children.Aggregate("", (str, platformView) => str + (str.Length > 0 ? ", " : "") + GetNativeText(platformView));
			});

			Assert.Equal(expected, actual);
		}
	}
}
