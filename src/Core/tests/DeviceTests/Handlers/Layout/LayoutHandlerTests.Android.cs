using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return ((layoutHandler as IElementHandler).NativeView as LayoutViewGroup).ChildCount;
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
	}
}
