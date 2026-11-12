using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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

		[Theory(DisplayName = "Position Initializes Correctly When ItemsSource Creates New Items On Each Enumeration")]
		[InlineData(0, true)]
		[InlineData(1, true)]
		[InlineData(2, true)]
		[InlineData(0, false)]
		[InlineData(1, false)]
		[InlineData(2, false)]
		public async Task PositionInitializesCorrectlyWhenItemsSourceCreatesNewItems(int position, bool loop)
		{
			// https://github.com/dotnet/maui/issues/32037
			SetupBuilder();

			// Each enumeration creates new item instances, so the CurrentItem picked from one
			// enumeration is not found in the next one
			var data = Enumerable.Range(0, 3).Select(index => new CarouselItem(index));

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
				Loop = loop,
				Position = position
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async (handler) =>
			{
				await handler.PlatformView.WaitForLayoutOrNonZeroSize();

				Assert.Equal(position, carouselView.Position);
				var currentItem = Assert.IsType<CarouselItem>(carouselView.CurrentItem);
				Assert.Equal(position, currentItem.Index);

				var platformPosition = GetPlatformPosition(handler);
				Assert.True(CheckPosition(platformPosition, position));
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task SelectingCurrentItemInsideCurrentItemChangedUpdatesRenderedItem(bool loop)
		{
			SetupBuilder();

			var items = new[] { "A", "B", "C" };
			var carouselView = new CarouselView
			{
				ItemsSource = items,
				Loop = loop,
				IsScrollAnimated = false,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");
					return label;
				})
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var recyclerView = handler.PlatformView;
				await recyclerView.WaitForLayoutOrNonZeroSize();

				Label GetCenteredLabel()
				{
					var cell = recyclerView.FindChildViewUnder(
						recyclerView.Width / 2f, recyclerView.Height / 2f) as ItemContentView;
					return cell?.View as Label;
				}

				await AssertEventually(
					() => carouselView.Position == 0
						&& Equals(carouselView.CurrentItem, items[0])
						&& Equals(GetCenteredLabel()?.BindingContext, items[0]),
					message: "The carousel should initially display A.");

				bool redirected = false;
				carouselView.CurrentItemChanged += (_, args) =>
				{
					if (!redirected && Equals(args.CurrentItem, items[1]))
					{
						redirected = true;
						carouselView.CurrentItem = items[2];
					}
				};

				// Use the next native adapter position, including the virtual range in loop mode.
				// This exercises renderer synchronization without a MAUI ScrollTo request in flight.
				var initialCell = recyclerView.FindChildViewUnder(
					recyclerView.Width / 2f, recyclerView.Height / 2f);
				Assert.NotNull(initialCell);
				var initialAdapterPosition = recyclerView.GetChildAdapterPosition(initialCell);
				Assert.NotEqual(RecyclerView.NoPosition, initialAdapterPosition);
				recyclerView.SmoothScrollToPosition(initialAdapterPosition + 1);

				var synchronized = await Wait(
					() => redirected
						&& Equals(carouselView.CurrentItem, items[2])
						&& carouselView.Position == 2
						&& recyclerView.ScrollState == RecyclerView.ScrollStateIdle
						&& Equals(GetCenteredLabel()?.BindingContext, items[2])
						&& GetCenteredLabel()?.Text == items[2]);

				var finalLabel = GetCenteredLabel();
				Assert.True(synchronized,
					$"Loop={loop}, Redirected={redirected}, CurrentItem={carouselView.CurrentItem}, " +
					$"Position={carouselView.Position}, ScrollState={recyclerView.ScrollState}, " +
					$"CenteredItem={finalLabel?.BindingContext}, RenderedText={finalLabel?.Text}. " +
					"Expected CurrentItem C, Position 2, and C rendered at the center after scrolling settles.");

				Assert.True(redirected);
				Assert.Equal(items[2], carouselView.CurrentItem);
				Assert.Equal(2, carouselView.Position);
				var centeredLabel = Assert.IsType<Label>(GetCenteredLabel());
				Assert.Equal(items[2], centeredLabel.BindingContext);
				Assert.Equal(items[2], centeredLabel.Text);
			});
		}

		class CarouselItem
		{
			public CarouselItem(int index) => Index = index;

			public int Index { get; }
		}

		[Fact(DisplayName = "Vertical Drag On Horizontal CarouselView Is Not Intercepted")]
		public async Task VerticalDragOnHorizontalCarouselIsNotIntercepted()
		{
			SetupBuilder();

			var data = new ObservableCollection<string> { "Item 1", "Item 2", "Item 3" };

			var template = new DataTemplate(() => new Grid { new Label() });

			var carouselView = new CarouselView
			{
				ItemTemplate = template,
				ItemsSource = data,
				IsSwipeEnabled = true,
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async (handler) =>
			{
				var recyclerView = handler.PlatformView;
				await recyclerView.WaitForLayoutOrNonZeroSize();

				// A vertical-dominant drag with a horizontal component large enough that the base
				// RecyclerView would otherwise treat it as a horizontal page swipe. The fix must
				// detect the off-axis gesture and delegate it to nested scrollable content, so
				// OnInterceptTouchEvent returns false. Without the fix, the carousel intercepts it.
				bool intercepted = SimulateDragIntercept(recyclerView, deltaX: 80, deltaY: 400);

				Assert.False(intercepted);
			});
		}

		// Dispatches a synthetic down/move/up gesture to the RecyclerView's touch-interception
		// pipeline and reports whether the move was intercepted by the carousel.
		static bool SimulateDragIntercept(RecyclerView recyclerView, float deltaX, float deltaY)
		{
			const float startX = 200f;
			const float startY = 200f;
			long downTime = global::Android.OS.SystemClock.UptimeMillis();

			var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, startX, startY, 0);
			recyclerView.OnInterceptTouchEvent(down);
			down.Recycle();

			var move = MotionEvent.Obtain(downTime, downTime + 16, MotionEventActions.Move, startX + deltaX, startY + deltaY, 0);
			bool intercepted = recyclerView.OnInterceptTouchEvent(move);
			move.Recycle();

			var up = MotionEvent.Obtain(downTime, downTime + 32, MotionEventActions.Up, startX + deltaX, startY + deltaY, 0);
			recyclerView.OnInterceptTouchEvent(up);
			up.Recycle();

			return intercepted;
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
				Assert.Equal(3, mauiPageControl.ChildCount);

				// Verify the selected indicator (position 0)
				var firstIndicator = mauiPageControl.GetChildAt(0) as ImageView;
				Assert.NotNull(firstIndicator);
				Assert.Equal("Item 1 of 3, selected", firstIndicator.ContentDescription);

				// Verify non-selected indicators
				var secondIndicator = mauiPageControl.GetChildAt(1) as ImageView;
				Assert.NotNull(secondIndicator);
				Assert.Equal("Item 2 of 3", secondIndicator.ContentDescription);

				var thirdIndicator = mauiPageControl.GetChildAt(2) as ImageView;
				Assert.NotNull(thirdIndicator);
				Assert.Equal("Item 3 of 3", thirdIndicator.ContentDescription);

				// Change the selected position and verify descriptions update correctly
				await InvokeOnMainThreadAsync(() => carouselView.Position = 1);
				await handler.PlatformView.WaitForLayoutOrNonZeroSize();

				Assert.Equal("Item 1 of 3", (mauiPageControl.GetChildAt(0) as ImageView)?.ContentDescription);
				Assert.Equal("Item 2 of 3, selected", (mauiPageControl.GetChildAt(1) as ImageView)?.ContentDescription);
				Assert.Equal("Item 3 of 3", (mauiPageControl.GetChildAt(2) as ImageView)?.ContentDescription);
			});
		}
	}
}