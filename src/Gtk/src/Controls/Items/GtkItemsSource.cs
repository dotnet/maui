using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Controls.Gtk.Items
{
	/// <summary>What a row of the flattened list is.</summary>
	enum GtkRowKind
	{
		/// <summary>An item of the items source.</summary>
		Item,

		/// <summary>The header of a group (grouped sources only).</summary>
		GroupHeader,

		/// <summary>The footer of a group (grouped sources only).</summary>
		GroupFooter,
	}

	/// <summary>One row of the flattened list: an item, or the header or footer of a group.</summary>
	sealed class GtkRow
	{
		public GtkRow(GtkRowKind kind, object? data, object? group)
		{
			Kind = kind;
			Data = data;
			Group = group;
		}

		public GtkRowKind Kind { get; }

		/// <summary>The item, or the group object of a header or footer.</summary>
		public object? Data { get; }

		/// <summary>The group the row belongs to (grouped sources), else <see langword="null"/>.</summary>
		public object? Group { get; }

		/// <summary>
		/// <see langword="true"/> once the row left the model. GTK keeps a row's widget bound after its item was removed (it only unbinds
		/// when it recycles the widget), so the owner of the widgets uses this to release the stale ones.
		/// </summary>
		public bool Removed { get; set; }

		/// <summary>The GObject that stands for the row in the GTK list model (created on demand, kept for identity).</summary>
		public MauiRowObject? Native { get; set; }
	}

	/// <summary>The GObject that represents one <see cref="GtkRow"/> in the GTK list model.</summary>
	sealed class MauiRowObject : GLib.Object
	{
		public MauiRowObject(GtkRow row) => Row = row;

		/// <summary>Needed by the GObject wrapper machinery.</summary>
		public MauiRowObject(IntPtr raw)
			: base(raw)
		{
			Row = null!;
		}

		public GtkRow Row { get; }
	}

	/// <summary>
	/// A <c>GListModel</c> written in C# over a MAUI items source: the rows are the (flattened) items, with group headers and footers
	/// for grouped sources, and <c>items-changed</c> follows <see cref="INotifyCollectionChanged"/>. It is the model of a
	/// <c>Gtk.ListView</c>/<c>Gtk.GridView</c> (wrapped by a selection model), so the view virtualises: only visible rows have widgets.
	/// </summary>
	sealed class GtkItemsSource : GLib.Object, GLib.IListModelImplementor
	{
		readonly List<GtkRow> _rows = new();
		readonly List<INotifyCollectionChanged> _observed = new();
		readonly IEnumerable? _source;
		readonly bool _grouped;
		readonly bool _groupHeaders;
		readonly bool _groupFooters;
		GLib.ListModelAdapter? _adapter;
		bool _disposed;

		/// <summary>Creates the model.</summary>
		/// <param name="source">The items source (a list of groups when <paramref name="grouped"/>).</param>
		/// <param name="grouped">Whether <paramref name="source"/> is a collection of groups, each a collection of items.</param>
		/// <param name="groupHeaders">Whether a header row precedes each group.</param>
		/// <param name="groupFooters">Whether a footer row follows each group.</param>
		public GtkItemsSource(IEnumerable? source, bool grouped = false, bool groupHeaders = false, bool groupFooters = false)
		{
			_source = source;
			_grouped = grouped;
			_groupHeaders = groupHeaders;
			_groupFooters = groupFooters;

			Rebuild(raise: false);
			Observe();
		}

		/// <summary>Needed by the GObject wrapper machinery.</summary>
		public GtkItemsSource(IntPtr raw)
			: base(raw)
		{
		}

		/// <summary>The list model that views and selection models are created over.</summary>
		public GLib.ListModelAdapter Model => _adapter ??= new GLib.ListModelAdapter(this);

		/// <summary>The number of rows.</summary>
		public int Count => _rows.Count;

		/// <summary>Whether the model has group header or footer rows (then a row position is not the index of its item).</summary>
		public bool HasGroupRows => _groupHeaders || _groupFooters;

		/// <summary>Raised on the GTK thread after the rows changed: <c>(position, removed, added)</c>.</summary>
		public event Action<int, int, int>? RowsChanged;

		/// <summary>The row at <paramref name="position"/>.</summary>
		/// <param name="position">The row index.</param>
		public GtkRow RowAt(int position) => _rows[position];

		/// <summary>The index of the first item row whose data is <paramref name="item"/>, or -1.</summary>
		/// <param name="item">The item.</param>
		public int IndexOf(object? item)
		{
			for (int i = 0; i < _rows.Count; i++)
			{
				if (_rows[i].Kind == GtkRowKind.Item && Equals(_rows[i].Data, item))
					return i;
			}

			return -1;
		}

		/// <summary>The number of item rows (no group headers or footers).</summary>
		public int ItemCount
		{
			get
			{
				int count = 0;
				foreach (var row in _rows)
				{
					if (row.Kind == GtkRowKind.Item)
						count++;
				}

				return count;
			}
		}

		// -------- GLib.IListModelImplementor

		/// <inheritdoc/>
		public GLib.GType ItemType => GLib.GType.Object;

		/// <inheritdoc/>
		public uint NItems => (uint)_rows.Count;

		/// <inheritdoc/>
		public IntPtr GetItem(uint position)
		{
			if (position >= _rows.Count)
				return IntPtr.Zero;

			var row = _rows[(int)position];
			row.Native ??= new MauiRowObject(row);

			// g_list_model_get_item is transfer-full: the caller owns a reference.
			return row.Native.OwnedHandle;
		}

		// -------- building and observing

		void Rebuild(bool raise)
		{
			int removed = _rows.Count;
			foreach (var old in _rows)
				old.Removed = true;
			_rows.Clear();

			if (_source is not null)
			{
				if (_grouped)
				{
					foreach (var group in _source)
					{
						if (_groupHeaders)
							_rows.Add(new GtkRow(GtkRowKind.GroupHeader, group, group));

						if (group is IEnumerable members)
						{
							foreach (var item in members)
								_rows.Add(new GtkRow(GtkRowKind.Item, item, group));
						}

						if (_groupFooters)
							_rows.Add(new GtkRow(GtkRowKind.GroupFooter, group, group));
					}
				}
				else
				{
					foreach (var item in _source)
						_rows.Add(new GtkRow(GtkRowKind.Item, item, null));
				}
			}

			if (raise)
				Raise(0, removed, _rows.Count);
		}

		void Raise(int position, int removed, int added)
		{
			_adapter?.EmitItemsChanged((uint)position, (uint)removed, (uint)added);
			RowsChanged?.Invoke(position, removed, added);
		}

		void Observe()
		{
			if (_source is INotifyCollectionChanged outer)
				Subscribe(outer);

			if (_grouped && _source is not null)
			{
				foreach (var group in _source)
				{
					if (group is INotifyCollectionChanged inner)
						Subscribe(inner);
				}
			}
		}

		void Subscribe(INotifyCollectionChanged collection)
		{
			collection.CollectionChanged += OnCollectionChanged;
			_observed.Add(collection);
		}

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			// Collection changes may arrive from any thread; the rows belong to the GTK thread.
			GtkRuntime.Post(() => Apply(sender, e));
		}

		void Apply(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (_disposed)
				return;

			// Grouped sources (and groups that changed) are rebuilt as a whole: the row positions of headers and footers shift.
			if (_grouped || !ReferenceEquals(sender, _source))
			{
				Reset();
				return;
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add when e.NewItems is not null && e.NewStartingIndex >= 0 && e.NewStartingIndex <= _rows.Count:
					{
						int index = e.NewStartingIndex;
						foreach (var item in e.NewItems)
							_rows.Insert(index++, new GtkRow(GtkRowKind.Item, item, null));

						Raise(e.NewStartingIndex, 0, e.NewItems.Count);
						break;
					}

				case NotifyCollectionChangedAction.Remove when e.OldItems is not null && e.OldStartingIndex >= 0 && e.OldStartingIndex + e.OldItems.Count <= _rows.Count:
					for (int i = 0; i < e.OldItems.Count; i++)
						_rows[e.OldStartingIndex + i].Removed = true;

					_rows.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
					Raise(e.OldStartingIndex, e.OldItems.Count, 0);
					break;

				case NotifyCollectionChangedAction.Replace when e.NewItems is not null && e.OldItems is not null && e.NewStartingIndex >= 0
					&& e.NewStartingIndex + e.NewItems.Count <= _rows.Count && e.NewItems.Count == e.OldItems.Count:
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						_rows[e.NewStartingIndex + i].Removed = true;
						_rows[e.NewStartingIndex + i] = new GtkRow(GtkRowKind.Item, e.NewItems[i], null);
					}

					Raise(e.NewStartingIndex, e.NewItems.Count, e.NewItems.Count);
					break;

				case NotifyCollectionChangedAction.Move when e.OldItems is not null && e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0
					&& e.OldStartingIndex + e.OldItems.Count <= _rows.Count:
					{
						var moved = _rows.GetRange(e.OldStartingIndex, e.OldItems.Count);
						_rows.RemoveRange(e.OldStartingIndex, moved.Count);
						Raise(e.OldStartingIndex, moved.Count, 0);

						int target = Math.Min(e.NewStartingIndex, _rows.Count);
						_rows.InsertRange(target, moved);
						Raise(target, 0, moved.Count);
						break;
					}

				default:
					Reset();
					break;
			}
		}

		void Reset()
		{
			foreach (var collection in _observed)
				collection.CollectionChanged -= OnCollectionChanged;
			_observed.Clear();

			Rebuild(raise: true);
			Observe();
		}

		/// <summary>Marks every row as removed (the model is being replaced).</summary>
		public void MarkAllRemoved()
		{
			foreach (var row in _rows)
				row.Removed = true;
		}

		/// <summary>Stops observing the source.</summary>
		public void Detach()
		{
			_disposed = true;
			foreach (var collection in _observed)
				collection.CollectionChanged -= OnCollectionChanged;
			_observed.Clear();
		}
	}
}
