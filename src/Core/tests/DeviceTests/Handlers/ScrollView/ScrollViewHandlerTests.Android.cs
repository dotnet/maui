using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using static Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewHandlerTests : CoreHandlerTestBase<ScrollViewHandler, ScrollViewStub>
	{
		[Fact]
		public async Task ContentInitializesCorrectly()
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{

				var entry = new EntryStub() { Text = "In a ScrollView" };
				var entryHandler = Activator.CreateInstance<EntryHandler>();
				entryHandler.SetMauiContext(MauiContext);
				entryHandler.SetVirtualView(entry);
				entry.Handler = entryHandler;

				var scrollView = new ScrollViewStub()
				{
					Content = entry
				};

				var scrollViewHandler = CreateHandler(scrollView);

				for (int n = 0; n < scrollViewHandler.PlatformView.ChildCount; n++)
				{
					var platformView = scrollViewHandler.PlatformView.GetChildAt(n);

					// ScrollView on Android uses an intermediate ContentViewGroup to handle measurement/arrangement/padding
					if (platformView is ContentViewGroup contentViewGroup)
					{
						for (int i = 0; i < contentViewGroup.ChildCount; i++)
						{
							if (contentViewGroup.GetChildAt(i) is AppCompatEditText)
							{
								return true;
							}
						}
					}
				}

				return false; // No AppCompatEditText
			});

			Assert.True(result, $"Expected (but did not find) a {nameof(AppCompatEditText)} child of the {nameof(NestedScrollView)}.");
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, true)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, false)]
		public async Task HorizontalVisibilityInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Horizontal,
					HorizontalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);


				return ((MauiHorizontalScrollView)scrollViewHandler.PlatformView.GetChildAt(0)).HorizontalScrollBarEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, true)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, false)]
		public async Task VerticalVisibilityInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Vertical,
					VerticalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);


				return ((MauiScrollView)scrollViewHandler.PlatformView).VerticalScrollBarEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, false)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, true)]
		public async Task VerticalScrollbarFadingInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Vertical,
					VerticalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);

				return ((MauiScrollView)scrollViewHandler.PlatformView).ScrollbarFadingEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, false)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, true)]
		public async Task HorizontalScrollbarFadingInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Horizontal,
					HorizontalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);

				return ((MauiHorizontalScrollView)scrollViewHandler.PlatformView.GetChildAt(0)).ScrollbarFadingEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, false)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, true)]
		public async Task VerticalandHorizontalScrollbarFadingInitializesCorrectlyOnBothOrientation(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Both,
					HorizontalScrollBarVisibility = visibility,
					VerticalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);

				var horizontalScrollView = (MauiHorizontalScrollView)scrollViewHandler.PlatformView.GetChildAt(0);
				var verticalScrollView = (MauiScrollView)scrollViewHandler.PlatformView;

				return horizontalScrollView.ScrollbarFadingEnabled && verticalScrollView.ScrollbarFadingEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task MauiScrollViewGetsFullHeightInHorizontalOrientation()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var sv = new MauiScrollView(MauiContext.Context);
				sv.SetOrientation(ScrollOrientation.Horizontal);
				var content = new Button(MauiContext.Context);
				sv.SetContent(content);

				var hsv = sv.FindViewWithTag("Microsoft.Maui.Android.HorizontalScrollView") as MauiHorizontalScrollView;

				Assert.NotNull(hsv);

				sv.Measure(
					MeasureSpec.MakeMeasureSpec(1000, global::Android.Views.MeasureSpecMode.Exactly),
					MeasureSpec.MakeMeasureSpec(1000, global::Android.Views.MeasureSpecMode.Exactly));

				sv.Layout(0, 0, 1000, 1000);

				var measuredWidth = hsv.MeasuredWidth;
				var measuredHeight = hsv.MeasuredHeight;

				Assert.Equal(1000, measuredWidth);
				Assert.Equal(1000, measuredHeight);
			});
		}
	}
}
