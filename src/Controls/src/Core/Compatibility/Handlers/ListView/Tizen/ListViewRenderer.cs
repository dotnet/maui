#nullable disable
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Tizen.NUI;
using Tizen.UIExtensions.NUI;
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using TItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;
using TScrollToPosition = Tizen.UIExtensions.Common.ScrollToPosition;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ListViewRenderer : ViewRenderer<ListView, TCollectionView>
	{
		public static PropertyMapper<ListView, ListViewRenderer> Mapper =
			new PropertyMapper<ListView, ListViewRenderer>(VisualElementRendererMapper);

		public static CommandMapper<ListView, ListViewRenderer> CommandMapper =
			new CommandMapper<ListView, ListViewRenderer>(VisualElementRendererCommandMapper);

		bool _isUpdateFromUI;

		public ListViewRenderer() : base(Mapper, CommandMapper)
		{
			AutoPackage = false;
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

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == ListView.ItemsSourceProperty.PropertyName)
				UpdateItemsSource(false);
			else if (e.PropertyName == ListView.ItemTemplateProperty.PropertyName)
				UpdateAdaptor(false);
			else if (e.PropertyName == ListView.RowHeightProperty.PropertyName)
				UpdateAdaptor(false);
			else if (e.PropertyName == ListView.SelectedItemProperty.PropertyName)
				UpdateSelectedItem();
			else if (e.PropertyName == ListView.HasUnevenRowsProperty.PropertyName)
				UpdateLayoutManager(false);
			else if (e.PropertyName == ListView.SelectionModeProperty.PropertyName)
				UpdateSelectionMode();
			else if (e.PropertyName == "HeaderElement")
				UpdateAdaptor(false);
			else if (e.PropertyName == "FooterElement")
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
					template = new CellWrapperTemplate(Element.ItemTemplate, Element);
				}
				else
				{
					template = new CellWrapperTemplate(new DataTemplate(() =>
					{
						var label = new TextCell();
						label.SetBinding(TextCell.TextProperty, static (object source) => source);
						return label;
					}), Element);
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
			var x = DPExtensions.ConvertToScaledDP(e.HorizontalOffset);
			var y = DPExtensions.ConvertToScaledDP(e.VerticalOffset);
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
