using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

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
	}
}
