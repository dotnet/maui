using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
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
		public async Task MeasureInvalidatedParentDoesNotBlockDescendantSafeAreaInvalidation()
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

				childPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(20, 10, 40, 60), child.LastArrangeBounds);

				childPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(25, 20, 30, 40);
				((IPlatformMeasureInvalidationController)parentPlatformView).InvalidateMeasure(isPropagating: true);
				MauiView.InvalidateSafeArea(parentPlatformView);
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(20, 25, 40, 45), child.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task ParentSafeAreaSuppressionDoesNotDependOnLayoutOrder()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.Container
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
				parentPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(47, 0, 34, 0);
				childPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(47, 0, 34, 0);
				parentPlatformView.AddSubview(childPlatformView);

				parentPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.SafeAreaInsetsDidChange();

				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 100), child.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task ResolvedBottomEdgeSkipsFartherAncestorKeyboardGeometry()
		{
			var grandparent = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.Container,
					SafeAreaRegions.None,
					SafeAreaRegions.SoftInput)
			};
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.Container)
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.Container
			};
			var grandparentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(grandparent);
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(async () =>
			{
				var grandparentPlatformView = grandparentHandler.PlatformView;
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;

				await grandparentPlatformView.AttachAndRun(() =>
				{
					var window = grandparentPlatformView.Window!;
					grandparentPlatformView.Frame = new CGRect(0, window.Bounds.Height - 100, 100, 100);
					parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
					childPlatformView.Frame = new CGRect(0, 0, 100, 100);
					grandparentPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(47, 0, 0, 0);
					parentPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(0, 0, 34, 0);
					childPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(47, 0, 34, 0);
					grandparentPlatformView.AddSubview(parentPlatformView);
					parentPlatformView.AddSubview(childPlatformView);

					grandparentPlatformView.LayoutSubviews();
					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					var keyboardFrameInWindow = new CGRect(
						0,
						window.Bounds.Height - 50,
						window.Bounds.Width,
						50);
					var keyboardFrame = window.ConvertRectToCoordinateSpace(
						keyboardFrameInWindow,
						window.Screen.CoordinateSpace);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);
					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						grandparentPlatformView.ResetConvertRectToViewCount();
						childPlatformView.LayoutSubviews();

						Assert.Equal(0, grandparentPlatformView.ConvertRectToViewCount);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}
				});
			});
		}

		[Fact]
		public async Task ResidualParentInsetDoesNotSuppressChildSafeArea()
		{
			var top = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.Container,
				SafeAreaRegions.None,
				SafeAreaRegions.None);
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = top
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = top
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(() =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				childPlatformView.Frame = new CGRect(0, 0, 100, 100);
				parentPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(10, 0, 0, 0);
				childPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(10, 0, 0, 0);
				parentPlatformView.AddSubview(childPlatformView);

				parentPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 100, 100), child.LastArrangeBounds);

				var residualInset = (nfloat)(0.25 / (double)UIScreen.MainScreen.Scale);
				parentPlatformView.SafeAreaInsetsValue = new UIEdgeInsets(residualInset, 0, 0, 0);
				parentPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.SafeAreaInsetsDidChange();
				childPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 10, 100, 90), child.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task ParentHandledEdgeLookupStopsWhenAllEdgesAreResolved()
		{
			var grandparent = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.Container
			};
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.Container
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.Container
			};
			var grandparentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(grandparent);
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(() =>
			{
				var grandparentPlatformView = grandparentHandler.PlatformView;
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;
				grandparentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				childPlatformView.Frame = new CGRect(0, 0, 100, 100);
				grandparentPlatformView.AddSubview(parentPlatformView);
				parentPlatformView.AddSubview(childPlatformView);

				childPlatformView.SafeAreaInsetsDidChange();
				grandparentPlatformView.ResetSafeAreaInsetsReadCount();
				parentPlatformView.ResetSafeAreaInsetsReadCount();
				childPlatformView.LayoutSubviews();

				Assert.True(
					parentPlatformView.SafeAreaInsetsReadCount > 0,
					"The direct parent's safe area must be evaluated.");
				Assert.True(
					grandparentPlatformView.SafeAreaInsetsReadCount == 0,
					"The ancestor walk should stop after the parent resolves every edge.");
			});
		}

		[Fact]
		public async Task EmptySafeAreaSkipsParentHandledEdgeLookup()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.Container
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
				childPlatformView.SafeAreaInsetsValue = UIEdgeInsets.Zero;
				parentPlatformView.AddSubview(childPlatformView);

				childPlatformView.SafeAreaInsetsDidChange();
				parentPlatformView.ResetSafeAreaInsetsReadCount();
				childPlatformView.LayoutSubviews();

				Assert.Equal(0, parentPlatformView.SafeAreaInsetsReadCount);
				Assert.Equal(new Rect(0, 0, 100, 100), child.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task EmptyManualScrollViewSafeAreaSkipsParentHandledEdgeLookup()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.Container
			};
			var scrollView = new RecordingSafeAreaScrollView
			{
				SafeAreaEdges = SafeAreaEdges.All
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var scrollViewHandler = await CreateHandlerAsync<TestSafeAreaScrollViewHandler>(scrollView);

			await InvokeOnMainThreadAsync(() =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var scrollViewPlatformView = Assert.IsType<TestSafeAreaMauiScrollView>(scrollViewHandler.PlatformView);
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				scrollViewPlatformView.Frame = new CGRect(0, 0, 100, 100);
				scrollViewPlatformView.SafeAreaInsetsValue = UIEdgeInsets.Zero;
				parentPlatformView.AddSubview(scrollViewPlatformView);

				scrollViewPlatformView.SafeAreaInsetsDidChange();
				parentPlatformView.ResetSafeAreaInsetsReadCount();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(0, parentPlatformView.SafeAreaInsetsReadCount);
				Assert.Equal(new Rect(0, 0, 100, 100), scrollView.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task KeyboardSafeAreaChangesInvalidateDescendants()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.SoftInput)
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.Container)
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(async () =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;

				await parentPlatformView.AttachAndRun(() =>
				{
					var windowBounds = parentPlatformView.Window!.Bounds;
					parentPlatformView.Frame = new CGRect(0, windowBounds.Height - 100, 100, 100);
					childPlatformView.Frame = new CGRect(0, 0, 100, 100);
					parentPlatformView.AddSubview(childPlatformView);

					parentPlatformView.SafeAreaInsetsDidChange();
					childPlatformView.SafeAreaInsetsDidChange();
					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					Assert.Equal(new Rect(0, 0, 100, 70), child.LastArrangeBounds);

					var parentFrameInWindow = parentPlatformView.ConvertRectToView(
						parentPlatformView.Bounds,
						parentPlatformView.Window);
					var keyboardFrameInWindow = new CGRect(
						0,
						parentFrameInWindow.Bottom - 50,
						windowBounds.Width,
						50);
					var keyboardFrame = parentPlatformView.Window!.ConvertRectToCoordinateSpace(
						keyboardFrameInWindow,
						parentPlatformView.Window.Screen.CoordinateSpace);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						parentPlatformView.LayoutSubviews();
						childPlatformView.LayoutSubviews();

						Assert.Equal(new Rect(0, 0, 100, 100), child.LastArrangeBounds);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}

					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					Assert.Equal(new Rect(0, 0, 100, 70), child.LastArrangeBounds);
				});
			});
		}

		[Fact]
		public async Task ParentContainerSafeAreaDoesNotSuppressChildKeyboardSafeArea()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.Container)
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.SoftInput)
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(async () =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;

				await parentPlatformView.AttachAndRun(() =>
				{
					var windowBounds = parentPlatformView.Window!.Bounds;
					parentPlatformView.Frame = new CGRect(0, windowBounds.Height - 100, 100, 100);
					childPlatformView.Frame = new CGRect(0, 0, 100, 100);
					parentPlatformView.AddSubview(childPlatformView);

					parentPlatformView.SafeAreaInsetsDidChange();
					childPlatformView.SafeAreaInsetsDidChange();
					parentPlatformView.LayoutSubviews();
					childPlatformView.Frame = new CGRect(
						parent.LastArrangeBounds.X,
						parent.LastArrangeBounds.Y,
						parent.LastArrangeBounds.Width,
						parent.LastArrangeBounds.Height);
					childPlatformView.LayoutSubviews();

					Assert.Equal(new Rect(0, 0, 100, 70), child.LastArrangeBounds);

					var parentFrameInWindow = parentPlatformView.ConvertRectToView(
						parentPlatformView.Bounds,
						parentPlatformView.Window);
					var keyboardFrameInWindow = new CGRect(
						0,
						parentFrameInWindow.Bottom - 50,
						windowBounds.Width,
						50);
					var keyboardFrame = parentPlatformView.Window!.ConvertRectToCoordinateSpace(
						keyboardFrameInWindow,
						parentPlatformView.Window.Screen.CoordinateSpace);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						parentPlatformView.LayoutSubviews();
						childPlatformView.LayoutSubviews();

						var childFrameInWindow = childPlatformView.Superview!
							.ConvertRectToView(childPlatformView.Frame, childPlatformView.Window);
						var keyboardOverlap = Math.Max(0, childFrameInWindow.Bottom - keyboardFrame.Y);

						Assert.True(keyboardOverlap > 0);
						Assert.Equal(
							(double)keyboardFrame.Y,
							childFrameInWindow.Y + child.LastArrangeBounds.Bottom,
							precision: 5);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}

					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					Assert.Equal(new Rect(0, 0, 100, 70), child.LastArrangeBounds);
				});
			});
		}

		[Fact]
		public async Task ParentAndChildKeyboardSafeAreasDoNotDoublePadArrangedChild()
		{
			var bottomSoftInput = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.SoftInput);
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = bottomSoftInput
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = bottomSoftInput
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(async () =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;

				await parentPlatformView.AttachAndRun(() =>
				{
					var windowBounds = parentPlatformView.Window!.Bounds;
					parentPlatformView.Frame = new CGRect(0, windowBounds.Height - 100, 100, 100);
					childPlatformView.Frame = new CGRect(0, 0, 100, 100);
					parentPlatformView.AddSubview(childPlatformView);

					parentPlatformView.SafeAreaInsetsDidChange();
					childPlatformView.SafeAreaInsetsDidChange();
					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					var parentFrameInWindow = parentPlatformView.Superview!
						.ConvertRectToView(parentPlatformView.Frame, parentPlatformView.Window);
					var keyboardFrame = new CGRect(
						0,
						parentFrameInWindow.Bottom - 50,
						windowBounds.Width,
						50);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						parentPlatformView.LayoutSubviews();
						Assert.Equal(new Rect(0, 0, 100, 50), parent.LastArrangeBounds);

						childPlatformView.Frame = new CGRect(
							parent.LastArrangeBounds.X,
							parent.LastArrangeBounds.Y,
							parent.LastArrangeBounds.Width,
							parent.LastArrangeBounds.Height);
						childPlatformView.LayoutSubviews();

						var childFrameInWindow = childPlatformView.Superview!
							.ConvertRectToView(childPlatformView.Frame, childPlatformView.Window);
						var keyboardOverlap = Math.Max(0, childFrameInWindow.Bottom - keyboardFrame.Y);

						Assert.Equal(0d, keyboardOverlap, precision: 5);
						Assert.Equal((double)keyboardFrame.Y, (double)childFrameInWindow.Bottom, precision: 5);
						Assert.Equal(new Rect(0, 0, 100, 50), child.LastArrangeBounds);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}
				});
			});
		}

		[Fact]
		public async Task ParentAndChildKeyboardSafeAreasProtectOverflowingChild()
		{
			var bottomSoftInput = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.SoftInput);
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = bottomSoftInput
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = bottomSoftInput
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(async () =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;

				await parentPlatformView.AttachAndRun(() =>
				{
					var windowBounds = parentPlatformView.Window!.Bounds;
					parentPlatformView.Frame = new CGRect(0, windowBounds.Height - 100, 100, 100);
					childPlatformView.Frame = new CGRect(0, 0, 100, 100);
					parentPlatformView.AddSubview(childPlatformView);

					parentPlatformView.SafeAreaInsetsDidChange();
					childPlatformView.SafeAreaInsetsDidChange();
					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					var parentFrameInWindow = parentPlatformView.Superview!
						.ConvertRectToView(parentPlatformView.Frame, parentPlatformView.Window);
					var keyboardFrame = new CGRect(
						0,
						parentFrameInWindow.Bottom - 50,
						windowBounds.Width,
						50);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						parentPlatformView.LayoutSubviews();
						childPlatformView.LayoutSubviews();

						var childFrameInWindow = childPlatformView.Superview!
							.ConvertRectToView(childPlatformView.Frame, childPlatformView.Window);
						var keyboardOverlap = Math.Max(0, childFrameInWindow.Bottom - keyboardFrame.Y);

						Assert.Equal(new Rect(0, 0, 100, 50), parent.LastArrangeBounds);
						Assert.Equal(50d, keyboardOverlap, precision: 5);
						Assert.Equal(new Rect(0, 0, 100, 50), child.LastArrangeBounds);
						Assert.Equal(
							(double)keyboardFrame.Y,
							childFrameInWindow.Y + child.LastArrangeBounds.Bottom,
							precision: 5);

						childPlatformView.Transform = CGAffineTransform.MakeTranslation(0, -50);
						childPlatformView.LayoutSubviews();
						childFrameInWindow = childPlatformView.Superview!
							.ConvertRectToView(childPlatformView.Frame, childPlatformView.Window);
						keyboardOverlap = Math.Max(0, childFrameInWindow.Bottom - keyboardFrame.Y);

						Assert.Equal(0d, keyboardOverlap, precision: 5);
						Assert.Equal(new Rect(0, 0, 100, 100), child.LastArrangeBounds);

						childPlatformView.Transform = CGAffineTransform.MakeIdentity();
						childPlatformView.LayoutSubviews();

						Assert.Equal(new Rect(0, 0, 100, 50), child.LastArrangeBounds);

						childPlatformView.Frame = new CGRect(
							parent.LastArrangeBounds.X,
							parent.LastArrangeBounds.Y,
							parent.LastArrangeBounds.Width,
							parent.LastArrangeBounds.Height);
						childPlatformView.LayoutSubviews();

						Assert.Equal(new Rect(0, 0, 100, 50), child.LastArrangeBounds);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}

					parentPlatformView.LayoutSubviews();
					childPlatformView.Frame = new CGRect(
						parent.LastArrangeBounds.X,
						parent.LastArrangeBounds.Y,
						parent.LastArrangeBounds.Width,
						parent.LastArrangeBounds.Height);
					childPlatformView.LayoutSubviews();

					Assert.Equal(new Rect(0, 0, 100, 100), child.LastArrangeBounds);
				});
			});
		}

		[Fact]
		public async Task HiddenKeyboardSkipsDuplicateSoftInputStrategyResolution()
		{
			var view = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.SoftInput)
			};
			var handler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(view);

			await InvokeOnMainThreadAsync(async () =>
			{
				var platformView = handler.PlatformView;

				await platformView.AttachAndRun(() =>
				{
					var window = platformView.Window!;
					platformView.SafeAreaInsetsValue = UIEdgeInsets.Zero;
					platformView.Frame = new CGRect(0, window.Bounds.Height - 100, 100, 100);
					platformView.SafeAreaInsetsDidChange();
					platformView.LayoutSubviews();

					view.ResetSafeAreaEdgesReadCount();
					platformView.LayoutSubviews();

					Assert.Equal(1, view.SafeAreaEdgesReadCount);
					Assert.Equal(new Rect(0, 0, 100, 100), view.LastArrangeBounds);

					var viewFrameInWindow = platformView.ConvertRectToView(platformView.Bounds, window);
					var keyboardFrameInWindow = new CGRect(
						0,
						viewFrameInWindow.Bottom - 50,
						window.Bounds.Width,
						50);
					var keyboardFrame = window.ConvertRectToCoordinateSpace(
						keyboardFrameInWindow,
						window.Screen.CoordinateSpace);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						platformView.LayoutSubviews();
						Assert.Equal(new Rect(0, 0, 100, 50), view.LastArrangeBounds);

						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
						platformView.LayoutSubviews();
						Assert.Equal(new Rect(0, 0, 100, 100), view.LastArrangeBounds);

						view.ResetSafeAreaEdgesReadCount();
						platformView.LayoutSubviews();
						Assert.Equal(1, view.SafeAreaEdgesReadCount);

						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillShowNotification,
							null,
							userInfo);
						platformView.LayoutSubviews();
						Assert.Equal(new Rect(0, 0, 100, 50), view.LastArrangeBounds);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}
				});
			});
		}

		[Fact]
		public async Task UnchangedKeyboardGeometryKeepsAncestorSafeAreaCache()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.Container)
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.SoftInput)
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(async () =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;

				await parentPlatformView.AttachAndRun(() =>
				{
					var windowBounds = parentPlatformView.Window!.Bounds;
					parentPlatformView.Frame = new CGRect(0, windowBounds.Height - 100, 100, 100);
					childPlatformView.Frame = new CGRect(0, 0, 100, 100);
					parentPlatformView.AddSubview(childPlatformView);

					parentPlatformView.SafeAreaInsetsDidChange();
					childPlatformView.SafeAreaInsetsDidChange();
					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					var keyboardFrame = new CGRect(0, windowBounds.Height - 50, windowBounds.Width, 50);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						parentPlatformView.LayoutSubviews();
						childPlatformView.LayoutSubviews();
						childPlatformView.LayoutSubviews();

						parentPlatformView.ResetSafeAreaInsetsReadCount();
						childPlatformView.ResetSafeAreaInsetsReadCount();
						childPlatformView.LayoutSubviews();
						childPlatformView.LayoutSubviews();

						Assert.Equal(0, parentPlatformView.SafeAreaInsetsReadCount);
						Assert.Equal(0, childPlatformView.SafeAreaInsetsReadCount);

						var subPixelStep = 0.4 / (double)parentPlatformView.Window!.Screen.Scale;
						childPlatformView.Transform = CGAffineTransform.MakeTranslation(0, (nfloat)subPixelStep);
						childPlatformView.LayoutSubviews();

						Assert.Equal(0, parentPlatformView.SafeAreaInsetsReadCount);
						Assert.Equal(0, childPlatformView.SafeAreaInsetsReadCount);

						childPlatformView.Transform = CGAffineTransform.MakeTranslation(0, (nfloat)(subPixelStep * 2));
						childPlatformView.LayoutSubviews();

						Assert.True(parentPlatformView.SafeAreaInsetsReadCount > 0);
						Assert.True(childPlatformView.SafeAreaInsetsReadCount > 0);
					}
					finally
					{
						childPlatformView.Transform = CGAffineTransform.MakeIdentity();
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}
				});
			});
		}

		[Fact]
		public async Task ChangedKeyboardOverlapInvalidatesNonSoftInputDescendants()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.SoftInput)
			};
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.Container)
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);

			await InvokeOnMainThreadAsync(async () =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;

				await parentPlatformView.AttachAndRun(() =>
				{
					var windowBounds = parentPlatformView.Window!.Bounds;
					parentPlatformView.SafeAreaInsetsValue = UIEdgeInsets.Zero;
					parentPlatformView.Frame = new CGRect(0, windowBounds.Height - 100, 100, 100);
					childPlatformView.Frame = new CGRect(0, 0, 100, 100);
					parentPlatformView.AddSubview(childPlatformView);

					parentPlatformView.SafeAreaInsetsDidChange();
					childPlatformView.SafeAreaInsetsDidChange();
					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					Assert.Equal(new Rect(0, 0, 100, 70), child.LastArrangeBounds);

					var parentFrameInWindow = parentPlatformView.ConvertRectToView(
						parentPlatformView.Bounds,
						parentPlatformView.Window);
					var keyboardFrameInWindow = new CGRect(
						0,
						parentFrameInWindow.Bottom - 50,
						windowBounds.Width,
						50);
					var keyboardFrame = parentPlatformView.Window!.ConvertRectToCoordinateSpace(
						keyboardFrameInWindow,
						parentPlatformView.Window.Screen.CoordinateSpace);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						parentPlatformView.LayoutSubviews();
						childPlatformView.LayoutSubviews();

						Assert.Equal(new Rect(0, 0, 100, 100), child.LastArrangeBounds);

						parentPlatformView.Transform = CGAffineTransform.MakeTranslation(0, -50);
						childPlatformView.Transform = CGAffineTransform.MakeTranslation(0, 50);
						parentPlatformView.LayoutSubviews();

						Assert.False(parentPlatformView.AppliesSafeAreaAdjustments);
						Assert.Equal(
							SafeAreaRegions.None,
							MauiView.GetParentHandledSafeAreaEdges(childPlatformView).Bottom);

						childPlatformView.LayoutSubviews();

						Assert.Equal(new Rect(0, 0, 100, 70), child.LastArrangeBounds);
					}
					finally
					{
						parentPlatformView.Transform = CGAffineTransform.MakeIdentity();
						childPlatformView.Transform = CGAffineTransform.MakeIdentity();
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}
				});
			});
		}

		[Fact]
		public async Task SoftInputAncestorInsideScrollViewDoesNotSuppressKeyboardAutoScroll()
		{
			var parent = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.SoftInput)
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);

			await InvokeOnMainThreadAsync(async () =>
			{
				using var scrollView = new UIScrollView(new CGRect(0, 0, 100, 100));
				using var descendant = new UIView(new CGRect(0, 0, 100, 100));
				var parentPlatformView = parentHandler.PlatformView;
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				scrollView.AddSubview(parentPlatformView);
				parentPlatformView.AddSubview(descendant);

				await scrollView.AttachAndRun(() =>
				{
					Assert.False(MauiView.IsSoftInputHandledByParent(descendant));
				});
			});
		}

		[Fact]
		public async Task KeyboardFrameConvertsFromScreenToWindowCoordinates()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var windowScene = UIApplication.SharedApplication.ConnectedScenes
					.OfType<UIWindowScene>()
					.First();
				using var window = new UIWindow(windowScene)
				{
					Frame = new CGRect(137, 89, 320, 480)
				};
				var keyboardFrameInWindow = new CGRect(0, 330, 320, 150);
				var keyboardFrameInScreen = window.ConvertRectToCoordinateSpace(
					keyboardFrameInWindow,
					window.Screen.CoordinateSpace);

				Assert.NotEqual(keyboardFrameInWindow, keyboardFrameInScreen);

				var convertedFrame = MauiView.ConvertKeyboardFrameToWindow(window, keyboardFrameInScreen);

				Assert.Equal((double)keyboardFrameInWindow.X, (double)convertedFrame.X, precision: 5);
				Assert.Equal((double)keyboardFrameInWindow.Y, (double)convertedFrame.Y, precision: 5);
				Assert.Equal((double)keyboardFrameInWindow.Width, (double)convertedFrame.Width, precision: 5);
				Assert.Equal((double)keyboardFrameInWindow.Height, (double)convertedFrame.Height, precision: 5);
			});
		}

		[Fact]
		public async Task FloatingKeyboardUsesClampedViewIntersection()
		{
			var view = new CustomSafeAreaView
			{
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.SoftInput)
			};
			var handler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(view);

			await InvokeOnMainThreadAsync(async () =>
			{
				var platformView = handler.PlatformView;

				await platformView.AttachAndRun(() =>
				{
					var window = platformView.Window!;
					platformView.SafeAreaInsetsValue = UIEdgeInsets.Zero;
					platformView.Frame = new CGRect(0, window.Bounds.Height - 100, 100, 100);
					platformView.SafeAreaInsetsDidChange();
					platformView.LayoutSubviews();

					var viewFrameInWindow = platformView.ConvertRectToView(platformView.Bounds, window);
					var lateralKeyboardFrameInWindow = new CGRect(
						viewFrameInWindow.Right + 10,
						viewFrameInWindow.Bottom - 50,
						100,
						50);

					PostKeyboardWillShow(window, lateralKeyboardFrameInWindow);

					try
					{
						platformView.LayoutSubviews();

						Assert.Equal(new Rect(0, 0, 100, 100), view.LastArrangeBounds);

						var oversizedKeyboardFrameInWindow = new CGRect(
							viewFrameInWindow.X,
							viewFrameInWindow.Y - 50,
							viewFrameInWindow.Width,
							viewFrameInWindow.Height + 100);
						PostKeyboardWillShow(window, oversizedKeyboardFrameInWindow);
						platformView.LayoutSubviews();

						Assert.Equal(new Rect(0, 0, 100, 0), view.LastArrangeBounds);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}
				});
			});

			static void PostKeyboardWillShow(UIWindow window, CGRect keyboardFrameInWindow)
			{
				var keyboardFrame = window.ConvertRectToCoordinateSpace(
					keyboardFrameInWindow,
					window.Screen.CoordinateSpace);
				using var userInfo = NSDictionary.FromObjectAndKey(
					NSValue.FromCGRect(keyboardFrame),
					UIKeyboard.FrameEndUserInfoKey);

				NSNotificationCenter.DefaultCenter.PostNotificationName(
					UIKeyboard.WillShowNotification,
					null,
					userInfo);
			}
		}

		[Fact]
		public async Task DesiredSizeDrivenSoftInputViewSettlesWhileKeyboardIsVisible()
		{
			var bottomSoftInput = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.SoftInput);
			var target = new CustomSafeAreaView
			{
				SafeAreaEdges = bottomSoftInput,
				MeasureResult = new Size(100, 100)
			};
			var spacer = new CustomSafeAreaView
			{
				SafeAreaEdges = SafeAreaEdges.None,
				MeasureResult = new Size(100, 100)
			};
			var stack = new VerticalStackLayout
			{
				Spacing = 0,
				SafeAreaEdges = SafeAreaEdges.None,
				Children =
				{
					spacer,
					target
				}
			};
			await CreateHandlerAsync<CustomSafeAreaViewHandler>(spacer);
			var targetHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(target);
			var stackHandler = await CreateHandlerAsync<LayoutHandler>(stack);

			await InvokeOnMainThreadAsync(async () =>
			{
				var stackPlatformView = stackHandler.PlatformView;
				var targetPlatformView = targetHandler.PlatformView;

				await stackPlatformView.AttachAndRun(() =>
				{
					var window = stackPlatformView.Window!;
					stackPlatformView.Frame = new CGRect(0, window.Bounds.Height - 400, 100, 400);
					stackPlatformView.SafeAreaInsetsDidChange();
					targetPlatformView.SafeAreaInsetsDidChange();
					stackPlatformView.LayoutSubviews();
					targetPlatformView.LayoutSubviews();

					Assert.Same(targetPlatformView, stackPlatformView.Subviews[1]);
					Assert.Equal(100d, (double)targetPlatformView.Bounds.Height, precision: 5);

					var targetFrameInWindow = targetPlatformView.ConvertRectToView(
						targetPlatformView.Bounds,
						window);
					var keyboardTop = targetFrameInWindow.Bottom - 20;
					var keyboardFrameInWindow = new CGRect(
						0,
						keyboardTop,
						window.Bounds.Width,
						window.Bounds.Height - keyboardTop);
					var keyboardFrame = window.ConvertRectToCoordinateSpace(
						keyboardFrameInWindow,
						window.Screen.CoordinateSpace);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						for (int i = 0; i < 4; i++)
						{
							stackPlatformView.LayoutSubviews();
							targetPlatformView.LayoutSubviews();
						}

						var settledHeight = (double)targetPlatformView.Bounds.Height;
						var settledArrange = target.LastArrangeBounds;
						var settledMeasureCount = target.MeasureCount;

						stackPlatformView.LayoutSubviews();
						targetPlatformView.LayoutSubviews();

						Assert.Equal(settledHeight, (double)targetPlatformView.Bounds.Height, precision: 5);
						Assert.Equal(settledArrange, target.LastArrangeBounds);
						Assert.Equal(settledMeasureCount, target.MeasureCount);
						Assert.Equal(100d, (double)targetPlatformView.Bounds.Height, precision: 5);
						Assert.Equal(new Rect(0, 0, 100, 80), target.LastArrangeBounds);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}
				});
			});
		}

		[Fact]
		public async Task NestedKeyboardSafeAreasUseCrossPlatformArrange()
		{
			var bottomSoftInput = new SafeAreaEdges(
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.None,
				SafeAreaRegions.SoftInput);
			var child = new CustomSafeAreaView
			{
				SafeAreaEdges = bottomSoftInput
			};
			var parent = new Grid
			{
				SafeAreaEdges = bottomSoftInput,
				Children = { child }
			};
			var childHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(child);
			var parentHandler = await CreateHandlerAsync<LayoutHandler>(parent);

			await InvokeOnMainThreadAsync(async () =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var childPlatformView = childHandler.PlatformView;

				await parentPlatformView.AttachAndRun(() =>
				{
					var window = parentPlatformView.Window!;
					parentPlatformView.Frame = new CGRect(0, window.Bounds.Height - 100, 100, 100);
					parentPlatformView.SafeAreaInsetsDidChange();
					childPlatformView.SafeAreaInsetsDidChange();
					parentPlatformView.LayoutSubviews();
					childPlatformView.LayoutSubviews();

					Assert.Same(childPlatformView, Assert.Single(parentPlatformView.Subviews));
					Assert.Equal(100d, (double)childPlatformView.Bounds.Height, precision: 5);

					var parentFrameInScreen = parentPlatformView.ConvertRectToView(parentPlatformView.Bounds, null);
					var keyboardFrame = new CGRect(
						0,
						parentFrameInScreen.Bottom - 50,
						window.Screen.Bounds.Width,
						50);
					using var userInfo = NSDictionary.FromObjectAndKey(
						NSValue.FromCGRect(keyboardFrame),
						UIKeyboard.FrameEndUserInfoKey);

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIKeyboard.WillShowNotification,
						null,
						userInfo);

					try
					{
						parentPlatformView.LayoutSubviews();

						Assert.Equal(50d, (double)childPlatformView.Bounds.Height, precision: 5);

						childPlatformView.LayoutSubviews();

						Assert.Equal(new Rect(0, 0, 100, 50), child.LastArrangeBounds);
					}
					finally
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(
							UIKeyboard.WillHideNotification,
							null);
					}
				});
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

				parent.SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.Container);
				parentPlatformView.LayoutSubviews();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 10, 100, 90), scrollView.LastArrangeBounds);

				parent.SafeAreaEdges = SafeAreaEdges.None;
				parentPlatformView.LayoutSubviews();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 10, 100, 60), scrollView.LastArrangeBounds);
			});
		}

		[Fact]
		public async Task SystemAdjustedScrollViewInsetsAreNotSuppressedByParent()
		{
			EnsureHandlerCreated(builder =>
				builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<BoxView, BoxViewHandler>()));

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
				SafeAreaEdges = SafeAreaEdges.Container,
				Content = new BoxView
				{
					WidthRequest = 30,
					HeightRequest = 50
				}
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var scrollViewHandler = await CreateHandlerAsync<TestSafeAreaScrollViewHandler>(scrollView);

			await InvokeOnMainThreadAsync(() =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var scrollViewPlatformView = Assert.IsType<TestSafeAreaMauiScrollView>(scrollViewHandler.PlatformView);
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				scrollViewPlatformView.Frame = new CGRect(0, 0, 100, 100);
				scrollViewPlatformView.AdjustedContentInsetValue = new UIEdgeInsets(10, 20, 30, 40);
				parentPlatformView.AddSubview(scrollViewPlatformView);

				parentPlatformView.SafeAreaInsetsDidChange();
				scrollViewPlatformView.SafeAreaInsetsDidChange();
				parentPlatformView.LayoutSubviews();
				scrollViewPlatformView.LayoutSubviews();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 40, 60), scrollView.LastArrangeBounds);
				Assert.Equal(new CGSize(90, 90), scrollViewPlatformView.SizeThatFits(new CGSize(100, 100)));

				scrollViewPlatformView.AdjustedContentInsetValue = new UIEdgeInsets(0, 20, 30, 40);
				scrollViewPlatformView.AdjustedContentInsetDidChange();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 40, 70), scrollView.LastArrangeBounds);
				Assert.Equal(new CGSize(90, 80), scrollViewPlatformView.SizeThatFits(new CGSize(100, 100)));
			});
		}

		[Fact]
		public async Task ScrollViewTransitionToSystemAdjustedInsetsDropsParentSuppression()
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
				SafeAreaEdges = SafeAreaEdges.Container
			};
			var parentHandler = await CreateHandlerAsync<CustomSafeAreaViewHandler>(parent);
			var scrollViewHandler = await CreateHandlerAsync<TestSafeAreaScrollViewHandler>(scrollView);

			await InvokeOnMainThreadAsync(() =>
			{
				var parentPlatformView = parentHandler.PlatformView;
				var scrollViewPlatformView = Assert.IsType<TestSafeAreaMauiScrollView>(scrollViewHandler.PlatformView);
				parentPlatformView.Frame = new CGRect(0, 0, 100, 100);
				scrollViewPlatformView.Frame = new CGRect(0, 0, 100, 100);
				scrollViewPlatformView.AdjustedContentInsetValue = UIEdgeInsets.Zero;
				parentPlatformView.AddSubview(scrollViewPlatformView);

				parentPlatformView.SafeAreaInsetsDidChange();
				scrollViewPlatformView.SafeAreaInsetsDidChange();
				parentPlatformView.LayoutSubviews();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(20, 0, 40, 70), scrollView.LastArrangeBounds);

				var initialArrangeCount = scrollView.ArrangeCount;
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(initialArrangeCount, scrollView.ArrangeCount);

				scrollViewPlatformView.AdjustedContentInsetValue = new UIEdgeInsets(10, 20, 30, 40);
				scrollViewPlatformView.AdjustedContentInsetDidChange();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(0, 0, 40, 60), scrollView.LastArrangeBounds);

				var adjustedArrangeCount = scrollView.ArrangeCount;
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(adjustedArrangeCount, scrollView.ArrangeCount);
				Assert.Equal(new Rect(0, 0, 40, 60), scrollView.LastArrangeBounds);

				scrollViewPlatformView.AdjustedContentInsetValue = UIEdgeInsets.Zero;
				scrollViewPlatformView.AdjustedContentInsetDidChange();
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(new Rect(20, 0, 40, 70), scrollView.LastArrangeBounds);

				var restoredArrangeCount = scrollView.ArrangeCount;
				scrollViewPlatformView.LayoutSubviews();

				Assert.Equal(restoredArrangeCount, scrollView.ArrangeCount);
				Assert.Equal(new Rect(20, 0, 40, 70), scrollView.LastArrangeBounds);
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
			public UIEdgeInsets SafeAreaInsetsValue { get; set; } = new(10, 20, 30, 40);

			public int ConvertRectToViewCount { get; private set; }

			public int SafeAreaInsetsReadCount { get; private set; }

			public override UIEdgeInsets SafeAreaInsets
			{
				get
				{
					SafeAreaInsetsReadCount++;
					return SafeAreaInsetsValue;
				}
			}

			public override CGRect ConvertRectToView(CGRect rect, UIView view)
			{
				ConvertRectToViewCount++;
				return base.ConvertRectToView(rect, view);
			}

			public void ResetConvertRectToViewCount() => ConvertRectToViewCount = 0;

			public void ResetSafeAreaInsetsReadCount() => SafeAreaInsetsReadCount = 0;
		}

		sealed class TestSafeAreaScrollViewHandler : ScrollViewHandler
		{
			protected override UIScrollView CreatePlatformView() => new TestSafeAreaMauiScrollView();
		}

		sealed class TestSafeAreaMauiScrollView : MauiScrollView
		{
			public UIEdgeInsets? AdjustedContentInsetValue { get; set; }

			public UIEdgeInsets SafeAreaInsetsValue { get; set; } = new(10, 20, 30, 40);

			public override UIEdgeInsets AdjustedContentInset =>
				AdjustedContentInsetValue ?? base.AdjustedContentInset;

			public override UIEdgeInsets SafeAreaInsets => SafeAreaInsetsValue;
		}

		sealed class RecordingSafeAreaScrollView : ScrollView, ICrossPlatformLayout
		{
			public int ArrangeCount { get; private set; }

			public Rect LastArrangeBounds { get; private set; }

			Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
			{
				ArrangeCount++;
				LastArrangeBounds = bounds;
				return bounds.Size;
			}

			Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint) =>
				new(widthConstraint, heightConstraint);
		}
	}
}
