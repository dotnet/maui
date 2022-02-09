using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using LayoutPanel = Microsoft.Maui.Platform.LayoutPanel;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		string GetNativeText(UIElement view) 
		{
			return (view as TextBlock).Text;
		}

		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return layoutHandler.NativeView.Children.Count;
		}

		double GetNativeChildCount(object nativeView)
		{
			return (nativeView as LayoutPanel).Children.Count;
		}

		IReadOnlyList<UIElement> GetNativeChildren(LayoutHandler layoutHandler)
		{
			var views = new List<UIElement>();

			for (int i = 0; i < layoutHandler.NativeView.Children.Count; i++)
			{
				views.Add(layoutHandler.NativeView.Children[i]);
			}

			return views;
		}

		async Task AssertZIndexOrder(IReadOnlyList<UIElement> children)
		{
			// Lots of ways we could compare the two lists, but dumping them both to comma-separated strings
			// makes it easy to give the test useful output

			string expected = await InvokeOnMainThreadAsync(() => {
				return children.OrderBy(nativeView => GetNativeText(nativeView))
					.Aggregate("", (str, nativeView) => str + (str.Length > 0 ? ", " : "") + GetNativeText(nativeView));
			});

			string actual = await InvokeOnMainThreadAsync(() => {
				return children.Aggregate("", (str, nativeView) => str + (str.Length > 0 ? ", " : "") + GetNativeText(nativeView));
			});

			Assert.Equal(expected, actual);
		}
	}
}
