using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class PickerStub : StubBase, IPicker
	{
		private string _text;
		public string Title { get; set; }

		public Color TitleColor { get; set; }

		public IList<string> Items { get; set; } = new List<string>();

		public IList ItemsSource { get; set; }

		public int SelectedIndex { get; set; } = -1;

		public object SelectedItem { get; set; }

		public double CharacterSpacing { get; set; }

		public Color TextColor { get; set; }

		public Font Font { get; set; }

		public TextAlignment HorizontalTextAlignment { get; set; }

		public TextAlignment VerticalTextAlignment { get; set; }

		string IPicker.Text { get => _text; set => _text = value; }

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
