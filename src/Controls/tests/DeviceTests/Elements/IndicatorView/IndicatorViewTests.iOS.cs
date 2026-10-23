using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Layout)]
	public class IndicatorViewTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(FlowDirection.LeftToRight)]
		[InlineData(FlowDirection.RightToLeft)]
		public async Task TemplateBoundsAndOrderFollowFlowDirection(FlowDirection initialDirection)
		{
			var indicator = CreateIndicator(initialDirection);

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<IndicatorViewHandler>(indicator);
				foreach (var direction in new[] { initialDirection, FlowDirection.RightToLeft, FlowDirection.LeftToRight })
				{
					indicator.FlowDirection = direction;
					Assert.Equal(new CGSize(68, 20), MeasureAndLayout(handler, new CGSize(300, 100)));
					AssertTemplateBoundsAndOrder(indicator, handler);
				}

				((IElementHandler)handler).DisconnectHandler();
			});
		}

		[Theory]
		[InlineData(FlowDirection.LeftToRight)]
		[InlineData(FlowDirection.RightToLeft)]
		public async Task UnchangedTemplateLayoutDoesNotRemeasure(FlowDirection direction)
		{
			var factory = new CountingLayoutManagerFactory();
			var indicator = CreateIndicator(direction, factory);

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<IndicatorViewHandler>(indicator);
				var available = new CGSize(300, 100);
				MeasureAndLayout(handler, available);
				int measureCount = factory.MeasureCount;
				Assert.True(measureCount > 0);

				for (int i = 0; i < 10; i++)
				{
					MeasureAndLayout(handler, available);
				}

				Assert.Equal(measureCount, factory.MeasureCount);
				AssertTemplateBoundsAndOrder(indicator, handler);
				((IElementHandler)handler).DisconnectHandler();
			});
		}

		[Fact]
		public async Task TemplateMeasureRespondsToContentAndConstraintChanges()
		{
			var factory = new CountingLayoutManagerFactory();
			var indicator = CreateIndicator(FlowDirection.LeftToRight, factory);

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<IndicatorViewHandler>(indicator);
				await handler.PlatformView.AttachAndRun(() =>
				{
					var available = new CGSize(300, 100);
					Assert.Equal(new CGSize(68, 20), MeasureAndLayout(handler, available));
					int measureCount = factory.MeasureCount;

					var layout = (Microsoft.Maui.Controls.Layout)indicator.IndicatorLayout;
					((View)layout.Children[0]).WidthRequest = 40;
					Assert.Equal(new CGSize(88, 20), MeasureAndLayout(handler, available));
					Assert.True(factory.MeasureCount > measureCount);
					measureCount = factory.MeasureCount;

					((ObservableCollection<string>)indicator.ItemsSource).Add("four");
					Assert.Equal(new CGSize(112, 20), MeasureAndLayout(handler, available));
					Assert.True(factory.MeasureCount > measureCount);
					measureCount = factory.MeasureCount;

					indicator.MaximumVisible = 2;
					Assert.Equal(new CGSize(64, 20), MeasureAndLayout(handler, available));
					Assert.True(factory.MeasureCount > measureCount);
					measureCount = factory.MeasureCount;

					var resized = new CGSize(200, 80);
					MeasureAndLayout(handler, resized);
					Assert.True(factory.MeasureCount > measureCount);
					measureCount = factory.MeasureCount;
					handler.PlatformView.Bounds = new CGRect(0, 0, 160, 40);
					handler.PlatformView.LayoutSubviews();
					((LayoutView)layout.ToPlatform()).LayoutSubviews();
					Assert.Equal(measureCount, factory.MeasureCount);
					AssertTemplateBoundsAndOrder(indicator, handler);
				});
				((IElementHandler)handler).DisconnectHandler();
			});
		}

		[Theory]
		[InlineData(FlowDirection.LeftToRight)]
		[InlineData(FlowDirection.RightToLeft)]
		public async Task NonTemplatedIndicatorsKeepNativeSizing(FlowDirection direction)
		{
			var indicator = CreateIndicator(direction);
			indicator.IndicatorTemplate = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<IndicatorViewHandler>(indicator);
				using var native = new UIPageControl
				{
					Pages = 3,
					HidesForSinglePage = true,
					BackgroundStyle = handler.PlatformView.BackgroundStyle
				};
				native.UpdateFlowDirection(indicator);
				var available = new CGSize(300, 100);

				Assert.Equal(native.SizeThatFits(available), handler.PlatformView.SizeThatFits(available));
				Assert.Equal(native.Pages, handler.PlatformView.Pages);
				((IElementHandler)handler).DisconnectHandler();
			});
		}

		IndicatorView CreateIndicator(FlowDirection direction, CountingLayoutManagerFactory factory = null)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<IndicatorView, IndicatorViewHandler>();
					handlers.AddHandler<Microsoft.Maui.Controls.Layout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
				if (factory is not null)
					builder.Services.AddSingleton<ILayoutManagerFactory>(factory);
			});

			return new IndicatorView
			{
				FlowDirection = direction,
				ItemsSource = new ObservableCollection<string> { "one", "two", "three" },
				IndicatorTemplate = new DataTemplate(() => new Label { WidthRequest = 20, HeightRequest = 20 })
			};
		}

		static CGSize MeasureAndLayout(IndicatorViewHandler handler, CGSize available)
		{
			var size = handler.PlatformView.SizeThatFits(available);
			handler.PlatformView.Bounds = new CGRect(CGPoint.Empty, size);
			handler.PlatformView.LayoutSubviews();
			var indicator = (ITemplatedIndicatorView)handler.VirtualView;
			((LayoutView)indicator.IndicatorsLayoutOverride.ToPlatform()).LayoutSubviews();
			return size;
		}

		static void AssertTemplateBoundsAndOrder(IndicatorView indicator, IndicatorViewHandler handler)
		{
			var layout = (Microsoft.Maui.Controls.Layout)indicator.IndicatorLayout;
			var host = (LayoutView)layout.ToPlatform();
			Assert.Equal(handler.PlatformView.Bounds, host.Frame);
			Assert.Equal(System.Math.Min(indicator.Count, indicator.MaximumVisible), layout.Children.Count);
			var rtl = indicator.FlowDirection == FlowDirection.RightToLeft;
			Assert.Equal(rtl ? UIUserInterfaceLayoutDirection.RightToLeft : UIUserInterfaceLayoutDirection.LeftToRight,
				host.EffectiveUserInterfaceLayoutDirection);

			double previousX = rtl ? double.PositiveInfinity : double.NegativeInfinity;
			foreach (IView child in layout.Children)
			{
				var frame = child.ToPlatform().Frame;
				Assert.True(frame.Width > 0 && frame.Height > 0);
				Assert.True(frame.Left >= 0 && frame.Right <= host.Bounds.Width);
				Assert.True(frame.Top >= 0 && frame.Bottom <= host.Bounds.Height);
				Assert.True(rtl ? frame.X < previousX : frame.X > previousX);
				previousX = frame.X;
			}
		}

		sealed class CountingLayoutManagerFactory : ILayoutManagerFactory
		{
			public int MeasureCount { get; private set; }

			public ILayoutManager CreateLayoutManager(Microsoft.Maui.Controls.Layout layout) =>
				layout is IStackLayout stack ? new CountingLayoutManager(stack, this) : null;

			sealed class CountingLayoutManager : HorizontalStackLayoutManager
			{
				readonly CountingLayoutManagerFactory _factory;

				public CountingLayoutManager(IStackLayout layout, CountingLayoutManagerFactory factory) : base(layout)
				{
					_factory = factory;
				}

				public override Size Measure(double widthConstraint, double heightConstraint)
				{
					_factory.MeasureCount++;
					return base.Measure(widthConstraint, heightConstraint);
				}
			}
		}
	}
}
