using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class PickerStub : StubBase, IPicker
	{
		public string Title { get; set; }

		public Color TitleColor { get; set; }

		public IList<string> Items { get; set; } = new List<string>();

		IList _itemsSource;

		public IList ItemsSource
		{
			get => _itemsSource;
			set => SetProperty(ref _itemsSource, value, onChanged: OnItemsSourceChanged);
		}

		void OnItemsSourceChanged(IList oldValue, IList newValue)
		{
			if (ItemsSource == null)
				return;

			Items.Clear();

			foreach (object item in ItemsSource)
				Items.Add(item.ToString());
		}

		public int SelectedIndex { get; set; } = -1;

		public object SelectedItem { get; set; }

		public double CharacterSpacing { get; set; }

		public Color TextColor { get; set; }

		public Font Font { get; set; }

		public TextAlignment HorizontalTextAlignment { get; set; }

		public TextAlignment VerticalTextAlignment { get; set; }

		int IItemDelegate<string>.GetCount() => Items?.Count ?? ItemsSource?.Count ?? 0;

		string IItemDelegate<string>.GetItem(int index)
		{
			if (index < 0)
				return "";
			if (index < Items?.Count)
				return Items[index];
			if (index < ItemsSource?.Count)
				return ItemsSource[index]?.ToString() ?? "";
			return "";
		}
	}
}
