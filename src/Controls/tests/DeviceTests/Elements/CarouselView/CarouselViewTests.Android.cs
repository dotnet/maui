using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CarouselViewTests
	{
		[Theory(DisplayName = "Position Initializes Correctly")]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(2)]
		public async Task PositionInitializesCorrectly(int position)
		{
			SetupBuilder();

			var expected = position;

			ObservableCollection<int> data = new ObservableCollection<int>()
			{
				1,
				2,
				3
			};

			var template = new DataTemplate(() =>
			{
				var label = new Label();

				return new Grid()
				{
					label
				};
			});

			var carouselView = new CarouselView()
			{
				ItemTemplate = template,
				ItemsSource = data,
				Position = expected
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async (handler) =>
			{
				await handler.PlatformView.WaitForLayoutOrNonZeroSize();

				var platformPosition = GetPlatformPosition(handler);
				Assert.True(CheckPosition(platformPosition, position));
			});
		}

		RecyclerView GetPlatformCarouselView(CarouselViewHandler carouselViewHandler) =>
			carouselViewHandler.PlatformView;

		int GetPlatformPosition(CarouselViewHandler carouselViewHandler)
		{
			var recyclerView = GetPlatformCarouselView(carouselViewHandler);

			var linearLayoutManager = recyclerView.GetLayoutManager() as LinearLayoutManager;

			if (linearLayoutManager is not null)
			{
				return linearLayoutManager.FindFirstCompletelyVisibleItemPosition();
			}

			return -1;
		}

		bool CheckPosition(int positionA, int positionB)
		{

			string s1 = positionA.ToString();
			string s2 = positionB.ToString();

			int n1 = s1.Length;
			int n2 = s2.Length;

			if (n1 < n2)
			{
				return false;
			}

			for (int i = 0; i < n2; i++)
			{
				if (s1[n1 - i - 1] != s2[n2 - i - 1])
				{
					return false;
				}
			}

			return true;
		}

		[Fact(DisplayName = "IndicatorView Provides Correct TalkBack Accessibility Description")]
		public async Task IndicatorViewProvidesCorrectTalkBackAccessibilityDescription()
		{
			SetupBuilder();

			var carouselView = new CarouselView
			{
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3" }
			};

			var indicatorView = new IndicatorView();
			carouselView.IndicatorView = indicatorView;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async (handler) =>
			{
				await handler.PlatformView.WaitForLayoutOrNonZeroSize();

				var indicatorHandler = CreateHandler<IndicatorViewHandler>(indicatorView);
				var mauiPageControl = indicatorHandler.PlatformView as MauiPageControl;
				
				Assert.NotNull(mauiPageControl);
				Assert.True(mauiPageControl.ChildCount > 0);

				var firstIndicator = mauiPageControl.GetChildAt(0) as ImageView;
				Assert.NotNull(firstIndicator);
				Assert.Equal("Item 1 of 3, selected", firstIndicator.ContentDescription);
			});
		}
	}
}