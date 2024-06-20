using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WColors = Microsoft.UI.Colors;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout;

public partial class LayoutHandlerTests
{
	/*
	Layouts on Windows (InputTransparent and Background)

	Windows does not support views being input transparent without all of the children also transparent.
	To work around this we leverage the fact that a null background is input transparent and a transparent background is not.

	By default a background is null and thus input transparent.

	For the matrix of IT and BK:

	IT=false + BK=null = background=Transparent
	IT=false + BK=Red  = background=Red
	IT=true  + BK=null = background=null
	IT=true  + BK=Red  = background=Transparent & Wrapper=Red

	 */
	[Fact]
	public Task ZZZ_NonInputTransparentNullBackgroundSetsPlatformBackgroundTransparent() =>
		SetupTest(
			arrange: layout => { },
			act: layout => { },
			assert: (layoutView, parentView) =>
			{
				// the layout has a transparent background
				var platBack = Assert.IsType<WSolidColorBrush>(layoutView.Background);
				Assert.Equal(WColors.Transparent, platBack.Color);

				// it is NOT input transparent
				Assert.True(layoutView.IsHitTestVisible);

				// there is NO wrapper view
				Assert.Equal(parentView, layoutView.Parent);
				Assert.Equal(parentView.Children[0], layoutView);
			});

	[Fact]
	public Task ZZZ_NonInputTransparentColorBackgroundSetsPlatformBackgroundColor() =>
		SetupTest(
			arrange: layout => { },
			act: layout =>
			{
				layout.Background = new SolidPaint(Colors.Red);
				layout.Handler.UpdateValue(nameof(IView.Background));
			},
			assert: (layoutView, parentView) =>
			{
				// the layout has a color background
				var platBack = Assert.IsType<WSolidColorBrush>(layoutView.Background);
				Assert.Equal(WColors.Red, platBack.Color);

				// it is NOT input transparent
				Assert.True(layoutView.IsHitTestVisible);

				// there is NO wrapper view
				Assert.Equal(parentView, layoutView.Parent);
				Assert.Equal(parentView.Children[0], layoutView);
			});

	[Fact]
	public Task ZZZ_InputTransparentNullBackgroundSetsPlatformBackgroundNull() =>
		SetupTest(
			arrange: layout => { },
			act: layout =>
			{
				layout.InputTransparent = true;
				layout.Handler.UpdateValue(nameof(IView.InputTransparent));
			},
			assert: (layoutView, parentView) =>
			{
				// the layout has a null background
				Assert.Null(layoutView.Background);

				// it is NOT input transparent
				Assert.True(layoutView.IsHitTestVisible);

				// there is NO wrapper view
				Assert.Equal(parentView, layoutView.Parent);
				Assert.Equal(parentView.Children[0], layoutView);
			});

	[Fact]
	public Task ZZZ_InputTransparentColorBackgroundSetsWrapperPlatformBackgroundColor() =>
		SetupTest(
			arrange: layout =>
			{
				layout.Background = new SolidPaint(Colors.Red);
			},
			act: layout =>
			{
				layout.InputTransparent = true;
				layout.Handler.UpdateValue(nameof(IView.InputTransparent));
			},
			assert: (layoutView, parentView) =>
			{
				// the layout has a null background
				Assert.Null(layoutView.Background);

				// it is NOT input transparent
				Assert.True(layoutView.IsHitTestVisible);

				// there IS a wrapper view
				var wrapper = Assert.IsType<WrapperView>(layoutView.Parent);
				Assert.Equal(parentView, wrapper.Parent);
				Assert.Equal(parentView.Children[0], wrapper);

				// the wrapper has a color background
				var platBack = Assert.IsType<WSolidColorBrush>(wrapper.BackgroundHost.Background);
				Assert.Equal(WColors.Red, platBack.Color);
			});

	async Task SetupTest(
		Action<LayoutStub> arrange,
		Action<LayoutStub> act,
		Action<LayoutPanel, LayoutPanel> assert)
	{
		EnsureHandlerCreated(builder =>
		{
			builder.ConfigureMauiHandlers(handler =>
			{
				handler.AddHandler<LayoutStub, LayoutHandler>();
			});
		});

		LayoutStub layout;
		var parent = new LayoutStub
		{
			Width = 100,
			Height = 100,
			Background = null,
			InputTransparent = false,
			Children =
			{
				(layout = new LayoutStub
				{
					Width = 100,
					Height = 100,
					Background = null,
					InputTransparent = false,
				})
			}
		};

		arrange(layout);

		await AttachAndRun(parent, _ =>
		{
			var parentView = parent.Handler.PlatformView as LayoutPanel;
			var layoutView = layout.Handler.PlatformView as LayoutPanel;

			Assert.NotNull(parentView);
			Assert.NotNull(layoutView);

			Assert.Equal(parentView, layoutView.Parent);
			Assert.Equal(parentView.Children[0], layoutView);

			act(layout);

			assert(layoutView, parentView);
		});
	}

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
}
