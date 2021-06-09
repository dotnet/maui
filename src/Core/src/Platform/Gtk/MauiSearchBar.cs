using System;
using Gtk;

namespace Microsoft.Maui
{

	public class MauiSearchBar : Gtk.SearchBar
	{

		public MauiSearchBar() : base()
		{
			Entry = new (string.Empty);
			Child = Entry;
			SearchModeEnabled = true;
		}

		public Gtk.Entry Entry { get; }

	}

}