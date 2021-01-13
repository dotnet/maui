using System;
using System.Threading.Tasks;
using ElmSharp;
using Xamarin.Forms.Internals;
using ERect = ElmSharp.Rect;

namespace Xamarin.Forms.Platform.Tizen
{
	enum SwipeDrawerState
	{
		Opend,
		Closed
	}

	public class SwipeViewRenderer : LayoutRenderer
	{
		static readonly double SwipeItemWidth = Forms.ConvertToScaledDP(100);
		static readonly double SwipeItemHeight = Forms.ConvertToScaledDP(40);
		static readonly int MovementThreshold = 1000;
		static readonly uint SwipeAnimationDuration = 120;

		GestureLayer _gestureLayer;
		IVisualElementRenderer _itemsRenderer;

		SwipeView SwipeView => Element as SwipeView;

		bool HasLeftItems => SwipeView.LeftItems?.Count > 0;
		bool HasRightItems => SwipeView.RightItems?.Count > 0;
		bool HasTopItems => SwipeView.TopItems?.Count > 0;
		bool HasBottomItems => SwipeView.BottomItems?.Count > 0;

		SwipeDirection SwipeDirection { get; set; }

		SwipeDrawerState DrawerState { get; set; }

		int MaximumSwipeSize { get; set; }

		bool IsHorizontalSwipe => SwipeDirection == SwipeDirection.Left || SwipeDirection == SwipeDirection.Right;
		bool IsNegativeDirection => SwipeDirection == SwipeDirection.Left || SwipeDirection == SwipeDirection.Up;

		SwipeItems CurrentItems { get; set; }


		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			base.OnElementChanged(e);
			Initialize();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_gestureLayer?.Unrealize();
				_gestureLayer = null;
				_itemsRenderer?.Dispose();
				_itemsRenderer = null;
			}
			base.Dispose(disposing);
		}

		void Initialize()
		{
			_gestureLayer?.Unrealize();
			_gestureLayer = new GestureLayer(NativeView);
			_gestureLayer.Attach(NativeView);

			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Move, OnMoved);
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.End, OnEnd);
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Abort, OnEnd);
			SwipeDirection = 0;
			DrawerState = SwipeDrawerState.Closed;
			Control.AllowFocus(true);
			Control.Unfocused += (s, e) =>
			{
				_ = SwipeCloseAsync();
			};
		}

		void OnMoved(GestureLayer.MomentumData moment)
		{
			if (SwipeDirection == 0)
			{
				var direction = SwipeDirectionHelper.GetSwipeDirection(new Point(moment.X1, moment.Y1), new Point(moment.X2, moment.Y2));

				if (HasRightItems && direction == SwipeDirection.Left)
				{
					SwipeDirection = SwipeDirection.Left;
				}
				else if (HasLeftItems && direction == SwipeDirection.Right)
				{
					SwipeDirection = SwipeDirection.Right;
				}
				else if (HasTopItems && direction == SwipeDirection.Down)
				{
					SwipeDirection = SwipeDirection.Down;
				}
				else if (HasBottomItems && direction == SwipeDirection.Up)
				{
					SwipeDirection = SwipeDirection.Up;
				}
				else
				{
					return;
				}

				UpdateItems();
				((ISwipeViewController)Element).SendSwipeStarted(new SwipeStartedEventArgs(SwipeDirection));
			}

			var offset = GetSwipeOffset(moment);

			if (IsNegativeDirection)
			{
				if (offset > 0)
					offset = 0;
			}
			else
			{
				if (offset < 0)
					offset = 0;
			}

			if (Math.Abs(offset) > MaximumSwipeSize)
			{
				offset = MaximumSwipeSize * (offset < 0 ? -1 : 1);
			}

			var toDragBound = NativeView.Geometry;
			if (IsHorizontalSwipe)
			{
				toDragBound.X += offset;
			}
			else
			{
				toDragBound.Y += offset;
			}
			Platform.GetRenderer(SwipeView.Content).NativeView.Geometry = toDragBound;
			((ISwipeViewController)Element).SendSwipeChanging(new SwipeChangingEventArgs(SwipeDirection, Forms.ConvertToScaledDP(offset)));
		}
		async void OnEnd(GestureLayer.MomentumData moment)
		{
			if (SwipeDirection == 0)
				return;

			if (ShouldBeOpen(moment))
			{
				await SwipeOpenAsync();
				if (CurrentItems.Mode == SwipeMode.Execute)
				{
					ExecuteItems(CurrentItems);
					_ = SwipeCloseAsync();
				}
			}
			else
			{
				await SwipeCloseAsync();
			}
		}

		async Task SwipeOpenAsync()
		{
			var opendBound = NativeView.Geometry;
			if (IsHorizontalSwipe)
			{
				opendBound.X += MaximumSwipeSize * (IsNegativeDirection ? -1 : 1);
			}
			else
			{
				opendBound.Y += MaximumSwipeSize * (IsNegativeDirection ? -1 : 1);
			}

			await AnimatedMove(SwipeView.Content, Platform.GetRenderer(SwipeView.Content).NativeView, opendBound, length: SwipeAnimationDuration);
			DrawerState = SwipeDrawerState.Opend;
		}

		async Task SwipeCloseAsync()
		{
			if (SwipeDirection == 0)
				return;

			await AnimatedMove(SwipeView.Content, Platform.GetRenderer(SwipeView.Content).NativeView, NativeView.Geometry, length: SwipeAnimationDuration);

			if (_itemsRenderer != null)
			{
				Control.Children.Remove(_itemsRenderer.NativeView);
				_itemsRenderer.Dispose();
				_itemsRenderer = null;
			}

			((ISwipeViewController)Element).SendSwipeEnded(new SwipeEndedEventArgs(SwipeDirection, DrawerState == SwipeDrawerState.Opend));
			DrawerState = SwipeDrawerState.Closed;
			SwipeDirection = 0;
		}

		bool ShouldBeOpen(GestureLayer.MomentumData data)
		{
			var momentum = IsHorizontalSwipe ? data.HorizontalMomentum : data.VerticalMomentum;

			if (Math.Abs(momentum) > MovementThreshold)
			{
				return IsNegativeDirection ? momentum < 0 : momentum > 0;
			}

			return Math.Abs(GetSwipeOffset(data)) > MaximumSwipeSize / 2.0;
		}

		int GetSwipeOffset(GestureLayer.MomentumData data)
		{
			if (IsHorizontalSwipe)
			{
				return DrawerState == SwipeDrawerState.Closed ? (data.X2 - data.X1) :
					(((IsNegativeDirection ? -1 : 1) * MaximumSwipeSize) - (data.X1 - data.X2));
			}
			else
			{
				return DrawerState == SwipeDrawerState.Closed ? (data.Y2 - data.Y1) :
					(((IsNegativeDirection ? -1 : 1) * MaximumSwipeSize) - (data.Y1 - data.Y2));
			}
		}

		SwipeItems GetSwipedItems()
		{
			SwipeItems items = SwipeView.LeftItems;
			switch (SwipeDirection)
			{
				case SwipeDirection.Right:
					items = SwipeView.LeftItems;
					break;
				case SwipeDirection.Left:
					items = SwipeView.RightItems;
					break;
				case SwipeDirection.Up:
					items = SwipeView.BottomItems;
					break;
				case SwipeDirection.Down:
					items = SwipeView.TopItems;
					break;
			}
			return items;
		}

		void UpdateItems()
		{
			CurrentItems = GetSwipedItems();
			var itemsLayout = new StackLayout
			{
				Spacing = 0,
				Orientation = IsHorizontalSwipe ? StackOrientation.Horizontal : StackOrientation.Vertical,
				FlowDirection = SwipeDirection == SwipeDirection.Left ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
			};

			foreach (var item in CurrentItems)
			{
				View itemView = null;
				if (item is SwipeItem switem)
				{
					itemView = CreateItemView(switem, !IsHorizontalSwipe);
				}
				else if (item is SwipeItemView customItem)
				{
					itemView = CreateItemView(customItem);
				}
				else
				{
					continue;
				}

				var tap = new TapGestureRecognizer();
				tap.Command = item.Command;
				tap.CommandParameter = item.CommandParameter;
				tap.Tapped += (s, e) =>
				{
					if (item is SwipeItem swipeItem)
						swipeItem.OnInvoked();

					if (item is SwipeItemView customSwipeItem)
						customSwipeItem.OnInvoked();

					if (CurrentItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
					{
						Device.BeginInvokeOnMainThread(() =>
						{
							_ = SwipeCloseAsync();
						});
					}
				};
				itemView.GestureRecognizers.Add(tap);

				if (IsHorizontalSwipe)
				{
					itemView.HorizontalOptions = LayoutOptions.Start;
					itemView.VerticalOptions = LayoutOptions.FillAndExpand;
				}
				else
				{
					itemView.VerticalOptions = LayoutOptions.Start;
					itemView.HorizontalOptions = LayoutOptions.FillAndExpand;
				}
				itemsLayout.Children.Add(itemView);
			}

			var itemsRenderer = Platform.GetOrCreateRenderer(itemsLayout);
			(itemsRenderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();
			var measured = itemsLayout.Measure(Element.Width, Element.Height);

			MaximumSwipeSize = Forms.ConvertToScaledPixel(
				IsHorizontalSwipe ?
				Math.Min(measured.Request.Width, Element.Width) :
				Math.Min(measured.Request.Height, Element.Height));

			Control.Children.Add(itemsRenderer.NativeView);

			var itemsGeometry = NativeView.Geometry;
			if (SwipeDirection == SwipeDirection.Up)
			{
				itemsGeometry.Y += (itemsGeometry.Height - MaximumSwipeSize);
			}
			itemsRenderer.NativeView.Geometry = itemsGeometry;
			itemsRenderer.NativeView.StackBelow(Platform.GetRenderer(SwipeView.Content).NativeView);

			_itemsRenderer = itemsRenderer;
		}

		static Task AnimatedMove(IAnimatable animatable, EvasObject target, ERect dest, Easing easing = null, uint length = 120)
		{
			var tcs = new TaskCompletionSource<bool>();

			var dx = target.Geometry.X - dest.X;
			var dy = target.Geometry.Y - dest.Y;

			new Animation((progress) =>
			{
				ERect toMove = dest;
				toMove.X += (int)(dx * (1 - progress));
				toMove.Y += (int)(dy * (1 - progress));
				target.Geometry = toMove;
			}).Commit(animatable, "Move", rate: 60, length: length, easing: easing, finished: (p, e) =>
			  {
				  tcs.SetResult(true);
			  });
			return tcs.Task;
		}

		static View CreateItemView(SwipeItemView item)
		{
			return new StackLayout
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children =
				{
					item.Content
				}
			};
		}

		static View CreateItemView(SwipeItem item, bool horizontal)
		{
			var image = new Image
			{
				Source = item.IconImageSource
			};
			var label = new Label
			{
				Text = item.Text,
				HorizontalTextAlignment = TextAlignment.Center,
				FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)),
			};

			if (horizontal)
			{
				image.VerticalOptions = LayoutOptions.FillAndExpand;
				image.HorizontalOptions = LayoutOptions.Start;

				label.VerticalOptions = LayoutOptions.CenterAndExpand;
				label.HorizontalOptions = LayoutOptions.CenterAndExpand;
				label.VerticalTextAlignment = TextAlignment.Center;
				label.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
			}
			else
			{
				image.VerticalOptions = LayoutOptions.FillAndExpand;
				image.HorizontalOptions = LayoutOptions.FillAndExpand;

				label.VerticalOptions = LayoutOptions.EndAndExpand;
				label.HorizontalOptions = LayoutOptions.CenterAndExpand;
				label.VerticalTextAlignment = TextAlignment.End;
			}

			var layout = new StackLayout
			{
				Padding = 5,
				BackgroundColor = item.BackgroundColor,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Orientation = horizontal ? StackOrientation.Horizontal : StackOrientation.Vertical,
				Children =
				{
					image,
					label
				}
			};
			if (horizontal)
			{
				layout.HeightRequest = SwipeItemHeight;
			}
			else
			{
				layout.WidthRequest = SwipeItemWidth;
			}
			return layout;
		}

		static void ExecuteItems(SwipeItems items)
		{
			foreach (var item in items)
			{
				var cmd = item.Command;
				object parameter = item.CommandParameter;

				if (cmd != null && cmd.CanExecute(parameter))
					cmd.Execute(parameter);

				if (item is SwipeItem swipeItem)
					swipeItem.OnInvoked();

				if (item is SwipeItemView customSwipeItem)
					customSwipeItem.OnInvoked();
			}
		}

	}
}
