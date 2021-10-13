using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Tizen.UIExtensions.NUI;
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using TItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;
using TScrollToPosition = Tizen.UIExtensions.Common.ScrollToPosition;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ListViewRenderer : ViewRenderer<ListView, TCollectionView>
	{
		bool _isUpdateFromUI;

		public ListViewRenderer()
		{
			RegisterPropertyHandler(ListView.ItemsSourceProperty, UpdateItemsSource);
			RegisterPropertyHandler(ListView.ItemTemplateProperty, UpdateAdaptor);
			RegisterPropertyHandler(ListView.RowHeightProperty, UpdateAdaptor);
			RegisterPropertyHandler(ListView.SelectedItemProperty, UpdateSelectedItem);
			RegisterPropertyHandler(ListView.HasUnevenRowsProperty, UpdateLayoutManager);
			RegisterPropertyHandler(ListView.SelectionModeProperty, UpdateSelectionMode);
			RegisterPropertyHandler(nameof(Element.HeaderElement), UpdateAdaptor);
			RegisterPropertyHandler(nameof(Element.FooterElement), UpdateAdaptor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new TCollectionView());
				Control.Scrolled += OnScrolled;
			}
			Element.ScrollToRequested += OnScrollToRequested;
			base.OnElementChanged(e);
			UpdateLayoutManager(false);
			UpdateAdaptor(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Control.Scrolled -= OnScrolled;
				Element.ScrollToRequested -= OnScrollToRequested;
				if (Control.Adaptor is ItemTemplateAdaptor old)
				{
					old.SelectionChanged -= OnItemSelectedFromUI;
				}
			}
			base.Dispose(disposing);
		}

		protected override void AddChild(Element child)
		{
			// empty on purpose
		}
		protected override void RemoveChild(VisualElement view)
		{
			// empty on purpose
		}

		protected void UpdateAdaptor(bool initialize)
		{
			if (!initialize)
			{
				if (Control.Adaptor is ItemTemplateAdaptor old)
				{
					old.SelectionChanged -= OnItemSelectedFromUI;
				}

				DataTemplate template;
				if (Element.ItemTemplate is DataTemplateSelector selector)
				{
					template = new CellWrapperTemplateSelector(selector);
				}
				else if (Element.ItemTemplate != null)
				{
					template = new CellWrapperTemplate(Element.ItemTemplate);
				}
				else
				{
					template = new CellWrapperTemplate(new DataTemplate(() =>
					{
						var label = new TextCell();
						label.SetBinding(TextCell.TextProperty, new Binding("."));
						return label;
					}));
				}
				var adaptor = new ListViewAdaptor(Element, Element.ItemsSource, template);
				Control.Adaptor = adaptor;

				adaptor.SelectionChanged += OnItemSelectedFromUI;
			}
		}

		void OnItemSelectedFromUI(object sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems.Count > 0)
			{
				_isUpdateFromUI = true;
				Element.NotifyRowTapped(Control.Adaptor.GetItemIndex(e.SelectedItems[0]));
				_isUpdateFromUI = false;
			}
		}

		void UpdateItemsSource(bool init)
		{
			UpdateAdaptor(init);
		}

		void UpdateLayoutManager(bool init)
		{
			if (init)
				return;
			Control.LayoutManager = new LinearLayoutManager(false, Element.HasUnevenRows ? TItemSizingStrategy.MeasureAllItems : TItemSizingStrategy.MeasureFirstItem, 2);
		}

		void UpdateSelectionMode()
		{
			Control.SelectionMode = Element.SelectionMode == ListViewSelectionMode.Single ? CollectionViewSelectionMode.SingleAlways : CollectionViewSelectionMode.None;
		}

		void UpdateSelectedItem()
		{
			if (_isUpdateFromUI)
				return;

			if (Element.SelectedItem == null)
			{
				foreach (var item in Control.SelectedItems.ToList())
				{
					Control.RequestItemUnselect(Control.Adaptor.GetItemIndex(item));
				}
			}
			else
			{
				Control.RequestItemSelect(Control.Adaptor.GetItemIndex(Element.SelectedItem));
			}
		}

		void OnScrolled(object sender, CollectionViewScrolledEventArgs e)
		{
			var x = Forms.ConvertToScaledDP(e.HorizontalOffset);
			var y = Forms.ConvertToScaledDP(e.VerticalOffset);
			Element.SendScrolled(new ScrolledEventArgs(x, y));
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			var scrollArgs = (ITemplatedItemsListScrollToRequestedEventArgs)e;

			var index = Control.Adaptor.GetItemIndex(scrollArgs.Item);
			Control.ScrollTo(index, (TScrollToPosition)e.Position, e.ShouldAnimate);
		}
	}
}
