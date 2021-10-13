using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Tizen.UIExtensions.NUI;
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView, TCollectionView>
	{
		List<View> _oldViews = new List<View>();

		public CarouselViewRenderer()
		{
			RegisterPropertyHandler(CarouselView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(CarouselView.IsSwipeEnabledProperty, UpdateIsSwipeEnabled);
			RegisterPropertyHandler(CarouselView.PositionProperty, UpdatePositionFromElement);
			RegisterPropertyHandler(CarouselView.CurrentItemProperty, UpdateCurrentItemFromElement);
		}

		protected override TCollectionView CreateNativeControl()
		{
			return new TCollectionView();
		}

		protected override ItemTemplateAdaptor CreateItemAdaptor(ItemsView view)
		{
			return new CarouselViewItemTemplateAdaptor(view);
		}

		protected override ItemTemplateAdaptor CreateDefaultItemAdaptor(ItemsView view)
		{
			return new CarouselViewItemDefaultTemplateAdaptor(view);
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return Element.ItemsLayout;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				Control.Scrolled += OnScrolled;
				Control.ScrollView.ScrollDragStarted += OnDragStart;
				Control.ScrollView.ScrollDragEnded += OnDragStop;
				Control.ScrollView.ScrollAnimationStarted += OnScrollStart;
			}
			Application.Current.Dispatcher.Dispatch(() =>
			{
				if (!IsDisposed)
				{
					UpdatePositionFromElement(false);
					UpdateCurrentItemFromElement(false);
				}
			});
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == nameof(Element.ItemsSource))
			{
				Element.Position = 0;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Control.Scrolled -= OnScrolled;
				Control.ScrollView.ScrollDragStarted -= OnDragStart;
				Control.ScrollView.ScrollDragEnded -= OnDragStop;
				Control.ScrollView.ScrollAnimationStarted -= OnScrollStart;
			}
			base.Dispose(disposing);
		}

		void OnDragStart(object sender, EventArgs e)
		{
			Element.SetIsDragging(true);
			Element.IsScrolling = true;
		}

		void OnDragStop(object sender, EventArgs e)
		{
			Element.SetIsDragging(false);
			Element.IsScrolling = false;
		}

		void OnScrollStart(object sender, EventArgs e)
		{
			Element.IsScrolling = true;
		}

		void OnScrolled(object sender, CollectionViewScrolledEventArgs e)
		{
			var scrolledIndex = e.CenterItemIndex;
			Element.SetValueFromRenderer(CarouselView.PositionProperty, scrolledIndex);
			Element.SetValueFromRenderer(CarouselView.CurrentItemProperty, Control.Adaptor[scrolledIndex]);

			if (0 <= scrolledIndex && scrolledIndex < Control.Count)
				Control.RequestItemSelect(scrolledIndex);

			Element.IsScrolling = false;

			if (Control.Adaptor is ItemTemplateAdaptor adaptor)
			{
				var newViews = new List<View>();
				var carouselPosition = Element.Position;
				var previousPosition = carouselPosition - 1;
				var nextPosition = carouselPosition + 1;

				for (int i = e.FirstVisibleItemIndex; i <= e.LastVisibleItemIndex; i++)
				{
					var itemView = adaptor.GetTemplatedView(i);
					if (itemView == null)
					{
						continue;
					}

					if (i == carouselPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.CurrentItemVisualState);
					}
					else if (i == previousPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.PreviousItemVisualState);
					}
					else if (i == nextPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.NextItemVisualState);
					}
					else
					{
						VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
					}

					newViews.Add(itemView);

					if (!Element.VisibleViews.Contains(itemView))
					{
						Element.VisibleViews.Add(itemView);
					}
				}

				foreach (var itemView in _oldViews)
				{
					if (!newViews.Contains(itemView))
					{
						VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
						if (Element.VisibleViews.Contains(itemView))
						{
							Element.VisibleViews.Remove(itemView);
						}
					}
				}
				_oldViews = newViews;
			}
		}

		void UpdateCurrentItemFromElement(bool isInitializing)
		{
			if (isInitializing)
				return;

			if (Element.CurrentItem != null)
				ScrollTo(Control.Adaptor.GetItemIndex(Element.CurrentItem));
		}

		void UpdatePositionFromElement(bool isInitializing)
		{
			if (isInitializing)
				return;

			ScrollTo(Element.Position);
		}

		void ScrollTo(int position)
		{
			if (Element.IsScrolling)
				return;

			if (position > -1 && position < Control.Adaptor.Count)
			{
				Control.ScrollTo(position, animate: true);
			}
		}

		void UpdateIsSwipeEnabled()
		{
			Control.ScrollView.ScrollEnabled = Element.IsSwipeEnabled;
		}
	}
}
