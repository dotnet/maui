using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewTests
	{
		[Fact]
		public async Task CustomViewSafeAreaEdgesReachMauiView()
		{
			var view = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.Container,
					SafeAreaRegions.Container,
					SafeAreaRegions.None)
			};
			var handler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(view);
			Assert.IsNotAssignableFrom<ISafeAreaViewStrategy>(view);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				platformView.Frame = new CGRect(0, 0, 100, 100);
				platformView.SafeAreaInsetsDidChange();
				platformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 10, 60, 90), view.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task ChangingParentSafeAreaEdgesInvalidatesDescendants()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.None
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.Container
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(() =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				childPlatformView.Frame = new CGRect(0, 0, 100, 100);
				parentPlatformView.AddSubview(childPlatformView);

				parentPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.SafeAreaInsetsDidChange();
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(20, 10, 40, 60), child.LastArrangeBounds);

				parent.SafeAreaEdges = SafeAreaEdges.All;
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 100), child.LastArrangeBounds);

				parent.SafeAreaEdges = SafeAreaEdges.None;
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(20, 10, 40, 60), child.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task ParentOnlySuppressesOverlappingChildSafeAreaEdges()
		{
			var top = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.Container,
				SafeAreaRegions.None,
				SafeAreaRegions.None);
			var topAndBottom = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.Container,
				SafeAreaRegions.None,
				SafeAreaRegions.Container);
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = top
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = topAndBottom
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(() =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				childPlatformView.Frame = new CGRect(0, 0, 100, 100);
				parentPlatformView.AddSubview(childPlatformView);

				parentPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.SafeAreaInsetsDidChange();
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 70), child.LastArrangeBounds);

				parent.SafeAreaEdges = SafeAreaEdges.None;
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 10, 100, 60), child.LastArrangeBounds);

				parent.SafeAreaEdges = top;
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 70), child.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task ParentOnlySuppressesOverlappingScrollViewSafeAreaEdges()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.Container,
					SafeAreaRegions.None,
					SafeAreaRegions.None)
			};
			var scrollView = new RecordingSafeAreaScrollView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.Container,
					SafeAreaRegions.None,
					SafeAreaRegions.Container)
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var scrollViewHandler = await CreateHandlerAsync<TestSafeAreaScrollViewHandler>(scrollView);

			await InvokeOnMainThreadAsync(() =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var scrollViewPlatformView = Assert.IsType<TestSafeAreaMauiScrollView>(scrollViewHandler.PlatformView);
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				scrollViewPlatformView.Frame = new CGRect(0, 0, 100, 100);
				parentPlatformView.AddSubview(scrollViewPlatformView);

				parentPlatformView.SafeAreaInsetsDidChange();
				scrollViewPlatformView.SafeAreaInsetsDidChange();
				parentPlatformView.LayoutSubviews();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 70), scrollView.LastArrangeBounds);

				parent.SafeAreaEdges = SafeAreaEdges.None;
				parentPlatformView.LayoutSubviews();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 10, 100, 60), scrollView.LastArrangeBounds);

				parent.SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.Container,
					SafeAreaRegions.None,
					SafeAreaRegions.None);
				parentPlatformView.LayoutSubviews();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 70), scrollView.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task ChangingAncestorSafeAreaEdgesInvalidatesEdgeDisjointGrandchild()
		{
			var top = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.Container,
				SafeAreaRegions.None,
				SafeAreaRegions.None);
			var bottom = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.Container);
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = top
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = bottom
			};
			var grandchild = new CustomSafeAreaView
			{
				SafeAreaEdges = top
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);
			var grandchildHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(grandchild);

			await InvokeOnMainThreadAsync(() =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;
				var grandchildPlatformView = grandchildHandler.PlatformView;

				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				childPlatformView.Frame = new CGRect(0, 0, 100, 100);
				grandchildPlatformView.Frame = new CGRect(0, 0, 100, 100);
				parentPlatformView.AddSubview(childPlatformView);
				childPlatformView.AddSubview(grandchildPlatformView);

				parentPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.SafeAreaInsetsDidChange();
				grandchildPlatformView.SafeAreaInsetsDidChange();
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();
				grandchildPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 100), grandchild.LastArrangeBounds);

				parent.SafeAreaEdges = SafeAreaEdges.None;
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();
				grandchildPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 10, 100, 90), grandchild.LastArrangeBounds);

				parent.SafeAreaEdges = top;
				parentPlatformView.LayoutSubviews();
				childPlatformView.LayoutSubviews();
				grandchildPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 100), grandchild.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task GestureRecognizersAttachToPlatformViewWhenNoContainerViewIsPresent()
		{
			var view = new Label()
			{
				GestureRecognizers = { new TapGestureRecognizer() }
			};
			(view as IWindowController).Window = new Window();

			var handler = await CreateHandlerAsync<LabelHandler>(view);

			var gestures = await InvokeOnMainThreadAsync(() =>
			{
				return new
				{
					PlatformView = GetGestureRecognizerTypes(handler.PlatformView),
					ContainerView = GetGestureRecognizerTypes(handler.ContainerView),
				};
			});

			Assert.Empty(gestures.ContainerView);
			Assert.NotEmpty(gestures.PlatformView);

			Assert.Contains(typeof(UITapGestureRecognizer), gestures.PlatformView);
		}

		[Fact]
		public async Task GestureRecognizersAttachToContainerViewWhenUsingContainerView()
		{
			var view = new Label()
			{
				Shadow = new Shadow(), // this results in a container view
				GestureRecognizers = { new TapGestureRecognizer() }
			};

			(view as IWindowController).Window = new Window();

			var handler = await CreateHandlerAsync<LabelHandler>(view);

			var gestures = await InvokeOnMainThreadAsync(() =>
			{
				return new
				{
					PlatformView = GetGestureRecognizerTypes(handler.PlatformView),
					ContainerView = GetGestureRecognizerTypes(handler.ContainerView),
				};
			});

			Assert.NotEmpty(gestures.ContainerView);
			Assert.Empty(gestures.PlatformView);

			Assert.Contains(typeof(UITapGestureRecognizer), gestures.ContainerView);
		}

		static Type[] GetGestureRecognizerTypes(UIView view) =>
			view?.GestureRecognizers?.Select(g => g.GetType()).ToArray() ?? Array.Empty<Type>();

		sealed class CustomSafeAreaViewHandler : ViewHandler<CustomSafeAreaView, TestSafeAreaContentView>
		{
			static readonly IPropertyMapper<CustomSafeAreaView, CustomSafeAreaViewHandler> Mapper =
				new PropertyMapper<CustomSafeAreaView, CustomSafeAreaViewHandler>(ViewHandler.ViewMapper);

			public CustomSafeAreaViewHandler()
				: base(Mapper)
			{
			}

			protected override TestSafeAreaContentView CreatePlatformView() =>
				new()
				{
					View = VirtualView,
					CrossPlatformLayout = VirtualView
				};

			public override void SetVirtualView(IView view)
			{
				base.SetVirtualView(view);
				PlatformView.View = view;
				PlatformView.CrossPlatformLayout = VirtualView;
			}

			protected override void DisconnectHandler(TestSafeAreaContentView platformView)
			{
				platformView.View = null;
				platformView.CrossPlatformLayout = null;
				base.DisconnectHandler(platformView);
			}
		}

		sealed class TestSafeAreaContentView : Microsoft.Maui.Platform.ContentView
		{
			public override UIEdgeInsets SafeAreaInsets => new(10, 20, 30, 40);
		}

		sealed class TestSafeAreaScrollViewHandler : ScrollViewHandler
		{
			protected override UIScrollView CreatePlatformView() => new TestSafeAreaMauiScrollView();
		}

		sealed class TestSafeAreaMauiScrollView : MauiScrollView
		{
			public override UIEdgeInsets SafeAreaInsets => new(10, 20, 30, 40);
		}

		sealed class RecordingSafeAreaScrollView : ScrollView, ICrossPlatformLayout
		{
			public Rect LastArrangeBounds { get; private set; }

			Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
			{
				LastArrangeBounds = bounds;
				return bounds.Size;
			}

			Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint) =>
				new(widthConstraint, heightConstraint);
		}
	}
}
