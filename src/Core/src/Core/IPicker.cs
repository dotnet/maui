using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IPicker : IView
	{
		string Title { get; }
		IList<string> Items { get; }
		IList ItemsSource { get; }
		int SelectedIndex { get; set; }
		object? SelectedItem { get; set; }
	}
}