using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using WSwipeBehaviorOnInvoked = Microsoft.UI.Xaml.Controls.SwipeBehaviorOnInvoked;
using WSwipeControl = Microsoft.UI.Xaml.Controls.SwipeControl;
using WSwipeItems = Microsoft.UI.Xaml.Controls.SwipeItems;
using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;
using WSwipeMode = Microsoft.UI.Xaml.Controls.SwipeMode;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class SwipeViewRenderer : ViewRenderer<SwipeView, WSwipeControl>
	{
		bool _isDisposed;
		Dictionary<WSwipeItem, SwipeItem> _leftItems;
		Dictionary<WSwipeItem, SwipeItem> _rightItems;
		Dictionary<WSwipeItem, SwipeItem> _topItems;
		Dictionary<WSwipeItem, SwipeItem> _bottomItems;

		public SwipeViewRenderer()
		{
			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<SwipeView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				e.NewElement.CloseRequested += OnCloseRequested;
				e.NewElement.LeftItems.CollectionChanged += OnSwipeItemsChanged;
				e.NewElement.RightItems.CollectionChanged += OnSwipeItemsChanged;
				e.NewElement.TopItems.CollectionChanged += OnSwipeItemsChanged;
				e.NewElement.BottomItems.CollectionChanged += OnSwipeItemsChanged;

				if (Control == null)
				{
					SetNativeControl(new WSwipeControl());
				}

				UpdateContent();
				UpdateSwipeItems();
				UpdateBackgroundColor();
			}

			if (e.OldElement != null)
			{
				e.OldElement.CloseRequested -= OnCloseRequested;
				e.OldElement.LeftItems.CollectionChanged -= OnSwipeItemsChanged;
				e.OldElement.RightItems.CollectionChanged -= OnSwipeItemsChanged;
				e.OldElement.TopItems.CollectionChanged -= OnSwipeItemsChanged;
				e.OldElement.BottomItems.CollectionChanged -= OnSwipeItemsChanged;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.PropertyName == ContentView.ContentProperty.PropertyName)
				UpdateContent();
			else if (changedProperty.IsOneOf(SwipeView.LeftItemsProperty, SwipeView.RightItemsProperty, SwipeView.TopItemsProperty, SwipeView.BottomItemsProperty))
				UpdateSwipeItems();
			else if (changedProperty.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			if (disposing)
			{
				if (Element != null)
				{
					Element.CloseRequested -= OnCloseRequested;

					if (Element.LeftItems != null)
					{
						Element.LeftItems.CollectionChanged -= OnSwipeItemsChanged;
						Element.LeftItems.PropertyChanged -= OnSwipeItemsPropertyChanged;
					}

					if (Element.RightItems != null)
					{
						Element.RightItems.CollectionChanged -= OnSwipeItemsChanged;
						Element.RightItems.PropertyChanged -= OnSwipeItemsPropertyChanged;
					}

					if (Element.TopItems != null)
					{
						Element.TopItems.CollectionChanged -= OnSwipeItemsChanged;
						Element.TopItems.PropertyChanged -= OnSwipeItemsPropertyChanged;
					}

					if (Element.BottomItems != null)
					{
						Element.BottomItems.CollectionChanged -= OnSwipeItemsChanged;
						Element.BottomItems.PropertyChanged -= OnSwipeItemsPropertyChanged;
					}
				}

				if (_leftItems != null)
					DisposeSwipeItems(_leftItems);

				if (_rightItems != null)
					DisposeSwipeItems(_rightItems);

				if (_topItems != null)
					DisposeSwipeItems(_topItems);

				if (_bottomItems != null)
					DisposeSwipeItems(_bottomItems);
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		protected override void UpdateBackgroundColor()
		{
			Color backgroundColor = Element.BackgroundColor;

			if (Control != null)
			{
				Control.Background = backgroundColor.IsDefault ? null : backgroundColor.ToBrush();
			}

			base.UpdateBackgroundColor();
		}

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (Control.Parent != null)
				return base.MeasureOverride(availableSize);
			else
			{
				if (Element == null || availableSize.Width * availableSize.Height == 0)
					return new Windows.Foundation.Size(0, 0);

				Element.IsInNativeLayout = true;

				double width = Math.Max(0, Element.Width);
				double height = Math.Max(0, Element.Height);
				var result = new Windows.Foundation.Size(width, height);

				if (Control != null)
				{
					double w = Element.Width;
					double h = Element.Height;

					if (w == -1)
						w = availableSize.Width;

					if (h == -1)
						h = availableSize.Height;

					w = Math.Max(0, w);
					h = Math.Max(0, h);

					// SwipeLayout sometimes crashes when Measure if not previously fully loaded into the VisualTree.
					Control.Loaded += (sender, args) => { Control.Measure(new Windows.Foundation.Size(w, h)); };
				}

				Element.IsInNativeLayout = false;

				return result;
			}
		}

		void OnSwipeItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateSwipeItems();
		}

		void OnSwipeItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var formsSwipeItems = sender as SwipeItems;

			if (e.PropertyName == SwipeItems.ModeProperty.PropertyName)
				UpdateSwipeMode(formsSwipeItems);
			else if (e.PropertyName == SwipeItems.SwipeBehaviorOnInvokedProperty.PropertyName)
				UpdateSwipeBehaviorOnInvoked(formsSwipeItems);
		}

		void OnSwipeItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var formsSwipeItem = sender as SwipeItem;

			if (e.IsOneOf(
				MenuItem.TextProperty,
				MenuItem.IconImageSourceProperty,
				MenuItem.CommandProperty,
				MenuItem.CommandParameterProperty,
				VisualElement.BackgroundColorProperty))
				UpdateSwipeItem(formsSwipeItem);
		}

		void UpdateContent()
		{
			if (Element.Content == null)
				return;

			var renderer = Element.Content.GetOrCreateRenderer();
			Control.Content = renderer?.ContainerElement;
		}

		void UpdateSwipeItems()
		{
			if (IsValidSwipeItems(Element.LeftItems))
				Control.LeftItems = CreateSwipeItems(SwipeDirection.Left);

			if (IsValidSwipeItems(Element.RightItems))
				Control.RightItems = CreateSwipeItems(SwipeDirection.Right);

			if (IsValidSwipeItems(Element.TopItems))
				Control.TopItems = CreateSwipeItems(SwipeDirection.Up);

			if (IsValidSwipeItems(Element.BottomItems))
				Control.BottomItems = CreateSwipeItems(SwipeDirection.Down);
		}

		void UpdateSwipeMode(SwipeItems swipeItems)
		{
			var windowsSwipeItems = GetWindowsSwipeItems(swipeItems);

			if (windowsSwipeItems != null)
				windowsSwipeItems.Mode = GetSwipeMode(swipeItems.Mode);
		}

		void UpdateSwipeBehaviorOnInvoked(SwipeItems swipeItems)
		{
			var windowsSwipeItems = GetWindowsSwipeItems(swipeItems);

			if (windowsSwipeItems != null)
				foreach (var windowSwipeItem in windowsSwipeItems.ToList())
					windowSwipeItem.BehaviorOnInvoked = GetSwipeBehaviorOnInvoked(swipeItems.SwipeBehaviorOnInvoked);
		}

		void UpdateSwipeItem(SwipeItem formsSwipeItem)
		{
			if (formsSwipeItem == null)
				return;

			var windowsSwipeItem = GetWindowsSwipeItem(formsSwipeItem);

			if (windowsSwipeItem != null)
			{
				windowsSwipeItem.Text = formsSwipeItem.Text;
				windowsSwipeItem.IconSource = formsSwipeItem.IconImageSource.ToWindowsIconSource();
				windowsSwipeItem.Background = formsSwipeItem.BackgroundColor.ToBrush();

				var textColor = GetSwipeItemColor(formsSwipeItem.BackgroundColor);
				windowsSwipeItem.Foreground = textColor.ToBrush();
			}
		}

		bool IsValidSwipeItems(SwipeItems swipeItems)
		{
			return swipeItems != null && swipeItems.Count > 0;
		}

		void DisposeSwipeItems(Dictionary<WSwipeItem, SwipeItem> list)
		{
			if (list != null)
			{
				foreach (var item in list)
				{
					if (item.Key != null)
						item.Key.Invoked -= OnSwipeItemInvoked;
					if (item.Value != null)
						item.Value.PropertyChanged -= OnSwipeItemPropertyChanged;
				}

				list.Clear();
				list = null;
			}
		}

		WSwipeItems CreateSwipeItems(SwipeDirection swipeDirection)
		{
			var swipeItems = new WSwipeItems();

			SwipeItems items = null;

			switch (swipeDirection)
			{
				case SwipeDirection.Left:
					DisposeSwipeItems(_leftItems);
					items = Element.LeftItems;
					_leftItems = new Dictionary<WSwipeItem, SwipeItem>();
					break;
				case SwipeDirection.Right:
					DisposeSwipeItems(_rightItems);
					items = Element.RightItems;
					_rightItems = new Dictionary<WSwipeItem, SwipeItem>();
					break;
				case SwipeDirection.Up:
					DisposeSwipeItems(_topItems);
					items = Element.TopItems;
					_topItems = new Dictionary<WSwipeItem, SwipeItem>();
					break;
				case SwipeDirection.Down:
					DisposeSwipeItems(_bottomItems);
					items = Element.BottomItems;
					_bottomItems = new Dictionary<WSwipeItem, SwipeItem>();
					break;
			}

			items.PropertyChanged += OnSwipeItemsPropertyChanged;
			swipeItems.Mode = GetSwipeMode(items.Mode);

			foreach (var item in items)
			{
				if (item is SwipeItem formsSwipeItem)
				{
					var textColor = GetSwipeItemColor(formsSwipeItem.BackgroundColor);

					var windowsSwipeItem = new WSwipeItem
					{
						Background = formsSwipeItem.BackgroundColor.IsDefault ? null : formsSwipeItem.BackgroundColor.ToBrush(),
						Foreground = textColor.ToBrush(),
						IconSource = formsSwipeItem.IconImageSource.ToWindowsIconSource(),
						Text = !string.IsNullOrEmpty(formsSwipeItem.Text) ? formsSwipeItem.Text : string.Empty,
						BehaviorOnInvoked = GetSwipeBehaviorOnInvoked(items.SwipeBehaviorOnInvoked)
					};

					formsSwipeItem.PropertyChanged += OnSwipeItemPropertyChanged;
					windowsSwipeItem.Invoked += OnSwipeItemInvoked;

					swipeItems.Add(windowsSwipeItem);

					FillSwipeItemsCache(swipeDirection, windowsSwipeItem, formsSwipeItem);
				}
			}

			return swipeItems;
		}

		void FillSwipeItemsCache(SwipeDirection swipeDirection, WSwipeItem windowsSwipeItem, SwipeItem formsSwipeItem)
		{
			switch (swipeDirection)
			{
				case SwipeDirection.Left:
					_leftItems.Add(windowsSwipeItem, formsSwipeItem);
					break;
				case SwipeDirection.Right:
					_rightItems.Add(windowsSwipeItem, formsSwipeItem);
					break;
				case SwipeDirection.Up:
					_topItems.Add(windowsSwipeItem, formsSwipeItem);
					break;
				case SwipeDirection.Down:
					_bottomItems.Add(windowsSwipeItem, formsSwipeItem);
					break;
			}
		}

		void OnSwipeItemInvoked(WSwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
		{
			var windowsSwipeItem = sender;
			var formsSwipeItem = GetFormsSwipeItem(windowsSwipeItem);
			formsSwipeItem?.OnInvoked();
		}

		WSwipeItems GetWindowsSwipeItems(SwipeItems swipeItems)
		{
			if (swipeItems == Element.LeftItems)
				return Control.LeftItems;

			if (swipeItems == Element.RightItems)
				return Control.RightItems;

			if (swipeItems == Element.TopItems)
				return Control.TopItems;

			if (swipeItems == Element.BottomItems)
				return Control.BottomItems;

			return null;
		}

		WSwipeItem GetWindowsSwipeItem(SwipeItem swipeItem)
		{
			if (_leftItems != null)
				return _leftItems.FirstOrDefault(x => x.Value.Equals(swipeItem)).Key;

			if (_rightItems != null)
				return _rightItems.FirstOrDefault(x => x.Value.Equals(swipeItem)).Key;

			if (_topItems != null)
				return _topItems.FirstOrDefault(x => x.Value.Equals(swipeItem)).Key;

			if (_bottomItems != null)
				return _bottomItems.FirstOrDefault(x => x.Value.Equals(swipeItem)).Key;

			return null;
		}

		SwipeItem GetFormsSwipeItem(WSwipeItem swipeItem)
		{
			if (_leftItems != null)
			{
				_leftItems.TryGetValue(swipeItem, out SwipeItem formsSwipeItem);

				if (formsSwipeItem != null)
					return formsSwipeItem;
			}

			if (_rightItems != null)
			{
				_rightItems.TryGetValue(swipeItem, out SwipeItem formsSwipeItem);

				if (formsSwipeItem != null)
					return formsSwipeItem;
			}

			if (_topItems != null)
			{
				_topItems.TryGetValue(swipeItem, out SwipeItem formsSwipeItem);

				if (formsSwipeItem != null)
					return formsSwipeItem;
			}

			if (_bottomItems != null)
			{
				_bottomItems.TryGetValue(swipeItem, out SwipeItem formsSwipeItem);

				if (formsSwipeItem != null)
					return formsSwipeItem;
			}

			return null;
		}

		WSwipeMode GetSwipeMode(SwipeMode swipeMode)
		{
			switch (swipeMode)
			{
				case SwipeMode.Execute:
					return WSwipeMode.Execute;
				case SwipeMode.Reveal:
					return WSwipeMode.Reveal;
			}

			return WSwipeMode.Reveal;
		}

		WSwipeBehaviorOnInvoked GetSwipeBehaviorOnInvoked(SwipeBehaviorOnInvoked swipeBehaviorOnInvoked)
		{
			switch (swipeBehaviorOnInvoked)
			{
				case SwipeBehaviorOnInvoked.Auto:
					return WSwipeBehaviorOnInvoked.Auto;
				case SwipeBehaviorOnInvoked.Close:
					return WSwipeBehaviorOnInvoked.Close;
				case SwipeBehaviorOnInvoked.RemainOpen:
					return WSwipeBehaviorOnInvoked.RemainOpen;
			}

			return WSwipeBehaviorOnInvoked.Auto;
		}

		Color GetSwipeItemColor(Color backgroundColor)
		{
			var luminosity = 0.2126 * backgroundColor.R + 0.7152 * backgroundColor.G + 0.0722 * backgroundColor.B;

			return luminosity < 0.75 ? Color.White : Color.Black;
		}

		void OnCloseRequested(object sender, EventArgs e)
		{
			if (Control == null)
				return;

			Control.Close();
		}
	}
}