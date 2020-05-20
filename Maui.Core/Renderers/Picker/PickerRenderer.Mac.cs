using System.Collections.Specialized;
using System.Linq;
using AppKit;
using Foundation;

namespace System.Maui.Platform
{
	public partial class PickerRenderer : AbstractViewRenderer<IPicker, NSPopUpButton>
	{
		protected override NSPopUpButton CreateView()
		{
			var nSPopUpButton = new NSPopUpButton();

			nSPopUpButton.Activated -= ComboBoxSelectionChanged;
			nSPopUpButton.Activated += ComboBoxSelectionChanged;

			((INotifyCollectionChanged)VirtualView.Items).CollectionChanged -= RowsCollectionChanged;
			((INotifyCollectionChanged)VirtualView.Items).CollectionChanged += RowsCollectionChanged;

			return nSPopUpButton;
		}
		protected override void DisposeView(NSPopUpButton nSPopUpButton)
		{
			nSPopUpButton.Activated -= ComboBoxSelectionChanged;
			((INotifyCollectionChanged)VirtualView.Items).CollectionChanged -= RowsCollectionChanged;

			base.DisposeView(nSPopUpButton);
		}

		public static void MapPropertyTitle(IViewRenderer renderer, IPicker picker)
		{
			(renderer as PickerRenderer)?.UpdatePicker();
		}

		public static void MapPropertyTitleColor(IViewRenderer renderer, IPicker picker)
		{
			(renderer as PickerRenderer)?.UpdatePicker();
		}

		public static void MapPropertyTextColor(IViewRenderer renderer, IPicker picker)
		{
			(renderer as PickerRenderer)?.UpdateTextColor();
		}

		public static void MapPropertySelectedIndex(IViewRenderer renderer, IPicker picker)
		{
			(renderer as PickerRenderer)?.UpdatePicker();
		}

		public virtual void UpdatePicker()
		{
			if (TypedNativeView == null || VirtualView == null)
				return;

			var items = VirtualView.Items;

			TypedNativeView.RemoveAllItems();
			TypedNativeView.AddItems(items.ToArray());

			var selectedIndex = VirtualView.SelectedIndex;

			if (items == null || items.Count == 0 || selectedIndex < 0)
				return;

			TypedNativeView.SelectItem(selectedIndex);
		}

		public virtual void UpdateTextColor()
		{
			if (TypedNativeView == null || VirtualView == null)
				return;

			foreach (NSMenuItem it in TypedNativeView.Items())
			{
				it.AttributedTitle = new NSAttributedString();
			}

			var color = VirtualView.Color;
			if (color != Color.Default && TypedNativeView.SelectedItem != null)
			{
				NSAttributedString textWithColor = new NSAttributedString(TypedNativeView.SelectedItem.Title, foregroundColor: color.ToNativeColor(), paragraphStyle: new NSMutableParagraphStyle() { Alignment = NSTextAlignment.Left });
				TypedNativeView.SelectedItem.AttributedTitle = textWithColor;
			}
		}

		void RowsCollectionChanged(object sender, EventArgs e)
		{
			UpdatePicker();
		}

		void ComboBoxSelectionChanged(object sender, EventArgs e)
		{
			VirtualView.SelectedIndex = (int)TypedNativeView.IndexOfSelectedItem;
		}
	}
}