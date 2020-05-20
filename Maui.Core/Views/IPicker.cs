using System.Collections;
using System.Collections.Generic;

namespace System.Maui
{
	public interface IPicker : IText
	{
		string Title { get; }
		Color TitleColor { get; }
		Color TextColor { get; }
		IList<string> Items { get; }
		IList ItemsSource { get; }
		int SelectedIndex { get; set; }
		object SelectedItem { get; }
	}
}