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
			return layoutHandler.PlatformView.Children.Count;
		}

		double GetNativeChildCount(object platformView)
		{
			return (platformView as LayoutPanel).Children.Count;
		}

		IReadOnlyList<UIElement> GetNativeChildren(LayoutHandler layoutHandler)
		{
			var views = new List<UIElement>();

			for (int i = 0; i < layoutHandler.PlatformView.Children.Count; i++)
			{
				views.Add(layoutHandler.PlatformView.Children[i]);
			}

			return views;
		}

		async Task AssertZIndexOrder(IReadOnlyList<UIElement> children)
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

		[Theory]
		[InlineData(FlowDirection.LeftToRight)]
		[InlineData(FlowDirection.RightToLeft)]
		[InlineData(FlowDirection.MatchParent)]
		public async Task LayoutFlowDirectionAlwaysMapToLeftToRight(FlowDirection flowDirection)
		{
			var layout = new LayoutStub();

			layout.FlowDirection = flowDirection;

			var handler = await CreateHandlerAsync(layout);

			await InvokeOnMainThreadAsync(() =>
			{
				if (handler.PlatformView is LayoutPanel layoutPanel)
				{
					Assert.Equal(UI.Xaml.FlowDirection.LeftToRight, layoutPanel.FlowDirection);
				}
			});
		}
	}
}
