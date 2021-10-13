using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Tizen.UIExtensions.NUI;
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using TCollectionViewScrolledEventArgs = Tizen.UIExtensions.NUI.CollectionViewScrolledEventArgs;
using TItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;
using TScrollToPosition = Tizen.UIExtensions.Common.ScrollToPosition;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public abstract class ItemsViewRenderer<TItemsView, TNative> : ViewRenderer<TItemsView, TNative>
		where TItemsView : ItemsView
		where TNative : TCollectionView
	{
		INotifyCollectionChanged _observableSource;

		protected IItemsLayout ItemsLayout { get; private set; }

		public ItemsViewRenderer()
		{
			RegisterPropertyHandler(ItemsView.ItemsSourceProperty, UpdateItemsSource);
			RegisterPropertyHandler(ItemsView.ItemTemplateProperty, UpdateAdaptor);
		}

		protected abstract TNative CreateNativeControl();

		protected virtual ItemTemplateAdaptor CreateItemAdaptor(ItemsView view)
		{
			return new ItemTemplateAdaptor(view);
		}

		protected virtual ItemTemplateAdaptor CreateDefaultItemAdaptor(ItemsView view)
		{
			return new ItemDefaultTemplateAdaptor(view);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TItemsView> e)
		{
			if (Control == null)
			{
				SetNativeControl(CreateNativeControl());
				Control.Scrolled += OnScrolled;
			}

			if (e.NewElement != null)
			{
				e.NewElement.ScrollToRequested += OnScrollToRequest;
			}

			base.OnElementChanged(e);
			ItemsLayout = GetItemsLayout();
			UpdateAdaptor(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					ItemsLayout.PropertyChanged -= OnLayoutPropertyChanged;
					Control.Scrolled -= OnScrolled;

					// Remove all child that created by ItemTemplate
					foreach (var child in Element.LogicalChildrenInternal.ToList())
					{
						Element.RemoveLogicalChild(child);
					}
				}
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected void UpdateItemsLayout()
		{
			if (ItemsLayout != null)
			{
				ItemsLayout.PropertyChanged -= OnLayoutPropertyChanged;
			}
			ItemsLayout = GetItemsLayout();
			if (ItemsLayout != null)
			{
				Control.LayoutManager = ItemsLayout.ToLayoutManager((Element as CollectionView)?.ItemSizingStrategy ?? ItemSizingStrategy.MeasureFirstItem);
				ItemsLayout.PropertyChanged += OnLayoutPropertyChanged;
			}
		}

		protected override void AddChild(Element child)
		{
			// empty on purpose
		}
		protected override void RemoveChild(VisualElement view)
		{
			// empty on purpose
		}

		protected virtual void OnLayoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LinearItemsLayout.ItemSpacing)
				|| e.PropertyName == nameof(GridItemsLayout.VerticalItemSpacing)
				|| e.PropertyName == nameof(GridItemsLayout.HorizontalItemSpacing))
			{
				UpdateItemsLayout();
			}
			else if (e.PropertyName == nameof(GridItemsLayout.Span))
			{
				((GridLayoutManager)(Control.LayoutManager)).UpdateSpan(((GridItemsLayout)sender).Span);
			}
		}

		protected abstract IItemsLayout GetItemsLayout();

		protected void UpdateAdaptor(bool initialize)
		{
			if (!initialize)
			{
				if (Control.Adaptor is ItemTemplateAdaptor old)
				{
					old.SelectionChanged -= OnItemSelectedFromUI;
				}

				if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
				{
					Control.Adaptor = EmptyItemAdaptor.Create(Element);
				}
				else if (Element.ItemTemplate == null)
				{
					Control.Adaptor = CreateDefaultItemAdaptor(Element);
				}
				else
				{
					Control.Adaptor = CreateItemAdaptor(Element);
				}

				if (Control.Adaptor is ItemTemplateAdaptor adaptor)
				{
					adaptor.SelectionChanged += OnItemSelectedFromUI;
				}
			}
		}

		protected virtual void OnItemSelectedFromUI(object sender, CollectionViewSelectionChangedEventArgs e)
		{
		}

		void OnScrolled(object sender, TCollectionViewScrolledEventArgs e)
		{
			Element.SendScrolled(new ItemsViewScrolledEventArgs
			{
				HorizontalDelta = Forms.ConvertToScaledDP(e.HorizontalDelta),
				HorizontalOffset = Forms.ConvertToScaledDP(e.HorizontalOffset),
				VerticalDelta = Forms.ConvertToScaledDP(e.VerticalDelta),
				VerticalOffset = Forms.ConvertToScaledDP(e.VerticalOffset),
				FirstVisibleItemIndex = e.FirstVisibleItemIndex,
				CenterItemIndex = e.CenterItemIndex,
				LastVisibleItemIndex = e.LastVisibleItemIndex,
			});

			if (Element.RemainingItemsThreshold >= 0)
			{
				if (Control.Adaptor.Count - 1 - e.LastVisibleItemIndex <= Element.RemainingItemsThreshold)
					Element.SendRemainingItemsThresholdReached();
			}
		}

		void OnScrollToRequest(object sender, ScrollToRequestEventArgs e)
		{
			if (e.Mode == ScrollToMode.Position)
			{
				Control.ScrollTo(e.Index, (TScrollToPosition)e.ScrollToPosition, e.IsAnimated);
			}
			else
			{
				Control.ScrollTo(e.Item, (TScrollToPosition)e.ScrollToPosition, e.IsAnimated);
			}
		}

		void UpdateItemsSource(bool initialize)
		{
			if (Element.ItemsSource is INotifyCollectionChanged collectionChanged)
			{
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
				_observableSource = collectionChanged;
				_observableSource.CollectionChanged += OnCollectionChanged;
			}
			UpdateAdaptor(initialize);
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
			{
				Control.Adaptor = EmptyItemAdaptor.Create(Element);
			}
			else
			{
				if (Control.Adaptor is EmptyItemAdaptor)
				{
					UpdateAdaptor(false);
				}
			}
		}
	}

	static class ItemsLayoutExtension
	{
		public static ICollectionViewLayoutManager ToLayoutManager(this IItemsLayout layout, ItemSizingStrategy sizing = ItemSizingStrategy.MeasureFirstItem)
		{
			switch (layout)
			{
				case LinearItemsLayout listItemsLayout:
					return new LinearLayoutManager(listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal, (TItemSizingStrategy)sizing, Forms.ConvertToScaledPixel(listItemsLayout.ItemSpacing));
				case GridItemsLayout gridItemsLayout:
					return new GridLayoutManager(gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal,
												 gridItemsLayout.Span,
												 (TItemSizingStrategy)sizing,
												 Forms.ConvertToScaledPixel(gridItemsLayout.VerticalItemSpacing),
												 Forms.ConvertToScaledPixel(gridItemsLayout.HorizontalItemSpacing));
				default:
					break;
			}
			return new LinearLayoutManager(false);
		}
	}
}
