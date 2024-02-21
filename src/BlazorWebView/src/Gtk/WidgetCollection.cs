using System.Collections.ObjectModel;
using Gtk;

namespace Microsoft.AspNetCore.Components.WebView.Gtk;

/// <summary>
/// A collection of <see cref="global::Gtk.Widget"/> items.
/// </summary>
public class WidgetCollection : ObservableCollection<Widget>
{

	public WidgetCollection(Widget owner)
	{ }

	public virtual bool IsReadOnly => true;

	public new virtual void Add(Widget? value)
	{
		if (value != null) base.Add(value);
	}

	public new virtual void Clear() => base.Clear();

	public new virtual void Remove(Widget? value)
	{
		if (value != null) base.Remove(value);
	}

	public virtual void SetChildIndex(Widget child, int newIndex)
	{ }

}