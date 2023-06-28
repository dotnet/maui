using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform;
using Tizen.UIExtensions.NUI;
using IMeasurable = Tizen.UIExtensions.Common.IMeasurable;
using Size = Microsoft.Maui.Graphics.Size;
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using TItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TableViewRenderer : ViewRenderer<TableView, TCollectionView>, IMeasurable
	{
		List<Cell> _items = new List<Cell>();
		DataTemplateSelector _dataTemplateSelector = new TableViewTemplateSelector();

		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new TCollectionView());
				Control.SelectionMode = CollectionViewSelectionMode.SingleAlways;
			}

			if (e.OldElement != null)
			{
				e.OldElement.ModelChanged -= OnRootPropertyChanged;
			}

			if (e.NewElement != null)
			{
				e.NewElement.ModelChanged += OnRootPropertyChanged;
			}
			base.OnElementChanged(e);
			Control.LayoutManager = new LinearLayoutManager(false, TItemSizingStrategy.MeasureAllItems);
			ApplyTableRoot();
		}

		TSize IMeasurable.Measure(double availableWidth, double availableHeight)
		{
			if (Control.Adaptor == null || Control.LayoutManager == null || Control.LayoutManager.GetScrollCanvasSize().Height == 0 || Control.LayoutManager.GetScrollCanvasSize().Width == 0)
			{
				var scaled = Devices.DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
				var size = new Size(availableWidth, availableHeight);
				if (size.Width == double.PositiveInfinity)
					size.Width = scaled.Width;
				if (size.Height == double.PositiveInfinity)
					size.Height = scaled.Height;
				return size.ToPixel();
			}

			var canvasSize = Control.LayoutManager.GetScrollCanvasSize();
			canvasSize.Width = Math.Min(canvasSize.Width, availableWidth.ToScaledPixel());
			canvasSize.Height = Math.Min(canvasSize.Height, availableHeight.ToScaledPixel());
			return canvasSize;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Element.ModelChanged -= OnRootPropertyChanged;
				}
			}

			base.Dispose(disposing);
		}

		void ApplyTableRoot()
		{
			_items.Clear();
			foreach (TableSection ts in Element.Root)
			{
				if (!string.IsNullOrEmpty(ts.Title))
				{
					_items.Add(new SectionCell
					{
						Text = ts.Title,
						TextColor = ts.TextColor,
					});
				}
				foreach (var cell in ts)
				{
					_items.Add(cell);
				}
			}

			var adaptor = new TableViewAdaptor(Element, _items, _dataTemplateSelector);

			adaptor.SelectionChanged += OnSelected;

			Control.Adaptor = adaptor;
		}

		void OnSelected(object sender, CollectionViewSelectionChangedEventArgs e)
		{
			var selected = e.SelectedItems.FirstOrDefault();
			if (selected != null && !(selected is SectionCell))
			{
				Element.Model.RowSelected(selected);
			}
		}

		void OnRootPropertyChanged(object sender, EventArgs e)
		{
			ApplyTableRoot();
		}

		class TableViewTemplateSelector : DataTemplateSelector
		{
			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				return new DataTemplate(() => CellContentFactory.CreateContent(item));
			}
		}
	}
}
