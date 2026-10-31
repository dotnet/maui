using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Platform;
using GtkWidget = Gtk.Widget;

namespace Microsoft.Maui.Controls.Gtk.Items
{
	/// <summary>
	/// The native side of a CollectionView (and the base of the CarouselView): a <c>Gtk.ListView</c> or <c>Gtk.GridView</c> over a C#
	/// <c>GListModel</c> (<see cref="GtkItemsSource"/>), with header, footer and empty view around it. Rows are virtualised by GTK; a
	/// <c>SignalListItemFactory</c> binds each visible row to a MAUI view created from the item template.
	/// </summary>
	/// <remarks>
	/// <para>Header and footer sit above and below the scrolling area (they do not scroll away with the items). Group headers and footers are
	/// rows of a linear list; a grid shows the items of all groups without headers.</para>
	/// <para>Template views are created once per visible row and recycled: a row bound to another item of the same template keeps its view
	/// and only gets a new binding context.</para>
	/// </remarks>
	sealed partial class GtkItemsController : IDisposable
	{
		readonly ItemsView _view;
		readonly IMauiContext _context;
		readonly global::Gtk.Box _root = new(global::Gtk.Orientation.Vertical, 0) { Hexpand = true, Vexpand = true };
		readonly global::Gtk.Stack _stack = new() { Hexpand = true, Vexpand = true, Hhomogeneous = true, Vhomogeneous = true };
		readonly global::Gtk.ScrolledWindow _scroller = new() { Hexpand = true, Vexpand = true, PropagateNaturalHeight = false, PropagateNaturalWidth = false };
		readonly MauiGtkItemHost _emptyHost = new();
		readonly HashSet<MauiGtkItemHost> _bound = new();
		readonly global::Gtk.SignalListItemFactory _factory = new();

		MauiGtkItemHost? _headerHost;
		MauiGtkItemHost? _footerHost;
		GtkItemsSource? _source;
		global::Gtk.ListBase? _list;
		global::Gtk.ISelectionModel? _selection;
		int _spacing;
		bool _updatingSelection;
		bool _disposed;
		double _lastHorizontalOffset;
		double _lastVerticalOffset;
		bool _thresholdReached;
		bool _scrollEvaluationQueued;
		double? _firstItemWidth;
		double? _firstItemHeight;
		int _firstItemViewport;
		GtkRow? _dragRow;

		public GtkItemsController(ItemsView view, IMauiContext context)
		{
			_view = view;
			_context = context;

			_stack.AddNamed(_scroller, "list");
			_stack.AddNamed(_emptyHost, "empty");
			_root.Append(_stack);

			_factory.Setup += OnSetup;
			_factory.Bind += OnBind;
			_factory.Unbind += OnUnbind;
			_factory.Teardown += OnTeardown;

			_scroller.Hadjustment.ValueChanged += OnAdjustmentValueChanged;
			_scroller.Vadjustment.ValueChanged += OnAdjustmentValueChanged;
			_scroller.Hadjustment.Changed += OnAdjustmentBoundsChanged;
			_scroller.Vadjustment.Changed += OnAdjustmentBoundsChanged;
		}

		/// <summary>The widget that is the platform view of the handler.</summary>
		public global::Gtk.Box Root => _root;

		/// <summary>The scrolling area.</summary>
		public global::Gtk.ScrolledWindow Scroller => _scroller;

		/// <summary>The list or grid, or <see langword="null"/> before the first layout update.</summary>
		public global::Gtk.ListBase? List => _list;

		/// <summary>The model that feeds the list, or <see langword="null"/> before the first items source update.</summary>
		public GtkItemsSource? Source => _source;

		/// <summary>The selection model, or <see langword="null"/> before the first update.</summary>
		public global::Gtk.ISelectionModel? Selection => _selection;

		/// <summary>The rows currently bound to a widget (the visible ones), for tests.</summary>
		public IReadOnlyCollection<MauiGtkItemHost> BoundHosts => _bound;

		/// <summary>The header host, or <see langword="null"/> when there is no header.</summary>
		public MauiGtkItemHost? HeaderHost => _headerHost;

		/// <summary>The footer host, or <see langword="null"/> when there is no footer.</summary>
		public MauiGtkItemHost? FooterHost => _footerHost;

		/// <summary>The empty view host.</summary>
		public MauiGtkItemHost EmptyHost => _emptyHost;

		/// <summary>Supplies a fixed row width (the carousel); applied to every row host.</summary>
		public Func<int>? FixedRowWidth { get; set; }

		/// <summary>Raised after the rows were rebuilt or changed.</summary>
		public event Action? SourceChanged;

		// ------------------------------------------------------------------ full refresh

		/// <summary>Rebuilds the list widget, model and selection from the current properties of the view.</summary>
		public void Refresh()
		{
			UpdateLayout();
			UpdateScrollBars();
			UpdateHeaderAndFooter();
			UpdateEmptyView();
		}

		// ------------------------------------------------------------------ items source, layout, selection

		/// <summary>Creates the model for the current <see cref="ItemsView.ItemsSource"/> and attaches it to the list.</summary>
		public void UpdateItemsSource()
		{
			if (_disposed)
				return;

			bool grouped = _view is GroupableItemsView { IsGrouped: true };
			bool linear = !(GetLayout() is GridItemsLayout);
			var groupable = _view as GroupableItemsView;

			InvalidateSharedSize();
			var previous = _source;
			previous?.Detach();
			_source = new GtkItemsSource(
				_view.ItemsSource,
				grouped,
				groupHeaders: grouped && linear,
				groupFooters: grouped && linear && groupable?.GroupFooterTemplate is not null);
			_source.RowsChanged += OnRowsChanged;

			// The rows of the previous model are gone.
			if (previous is not null)
				previous.MarkAllRemoved();

			AttachModel();
			Prune();
			UpdateEmptyView();
			SourceChanged?.Invoke();
		}

		/// <summary>
		/// Releases the views of rows that left the model. GTK leaves a removed row's widget bound until it recycles it, which would keep the
		/// template view (and its bindings) alive in the items view's logical tree.
		/// </summary>
		void Prune()
		{
			List<MauiGtkItemHost>? stale = null;
			foreach (var host in _bound)
			{
				if (host.Row is null || host.Row.Removed)
					(stale ??= new()).Add(host);
			}

			if (stale is null)
				return;

			foreach (var host in stale)
			{
				_bound.Remove(host);
				Detach(host);
				host.Row = null;
			}
		}

		void OnRowsChanged(int position, int removed, int added)
		{
			Prune();
			UpdateEmptyView();

			// New rows can move the end of the list away again: the threshold re-arms and is judged after the layout.
			_thresholdReached = false;
			QueueScrollEvaluation(raiseScrolled: false);
			SourceChanged?.Invoke();
		}

		IItemsLayout? GetLayout() => (_view as StructuredItemsView)?.ItemsLayout ?? (_view as CarouselView)?.ItemsLayout;

		/// <summary>(Re)creates the list or grid for <see cref="StructuredItemsView.ItemsLayout"/>.</summary>
		public void UpdateLayout()
		{
			if (_disposed)
				return;

			var layout = GetLayout();
			var orientation = layout is ItemsLayout items && items.Orientation == ItemsLayoutOrientation.Horizontal
				? global::Gtk.Orientation.Horizontal
				: global::Gtk.Orientation.Vertical;

			bool grid = layout is GridItemsLayout;
			_spacing = layout switch
			{
				LinearItemsLayout linear => (int)Math.Round(linear.ItemSpacing),
				GridItemsLayout g => (int)Math.Round(orientation == global::Gtk.Orientation.Vertical ? g.VerticalItemSpacing : g.HorizontalItemSpacing),
				_ => 0,
			};

			bool recreate = grid != (_list is global::Gtk.GridView) || _list is null;
			if (recreate)
			{
				_scroller.Child = null;
				_list = null;
			}

			if (_list is null)
			{
				_list = grid
					? new global::Gtk.GridView(null!, _factory) { SingleClickActivate = false }
					: new global::Gtk.ListView(null!, _factory) { ShowSeparators = false, SingleClickActivate = false };
				_scroller.Child = _list;

				if (_source is not null)
					AttachModel();
			}

			_list.Orientation = orientation;
			InvalidateSharedSize();

			if (_list is global::Gtk.GridView gridView && layout is GridItemsLayout gridLayout)
			{
				uint span = (uint)Math.Max(1, gridLayout.Span);
				gridView.MinColumns = span;
				gridView.MaxColumns = span;
			}

			// Spacing applies to the rows that are bound; the others pick it up when they are.
			foreach (var host in _bound)
				ApplySpacing(host);
		}

		void ApplySpacing(MauiGtkItemHost host)
		{
			int half = Math.Max(0, _spacing) / 2;
			host.MarginTop = half;
			host.MarginBottom = half;
		}

		/// <summary>Maps the scroll bar visibility properties to the scroller policies.</summary>
		public void UpdateScrollBars()
		{
			static global::Gtk.PolicyType ToPolicy(ScrollBarVisibility visibility) => visibility switch
			{
				ScrollBarVisibility.Always => global::Gtk.PolicyType.Always,
				ScrollBarVisibility.Never => global::Gtk.PolicyType.Never,
				_ => global::Gtk.PolicyType.Automatic,
			};

			_scroller.HscrollbarPolicy = ToPolicy(_view.HorizontalScrollBarVisibility);
			_scroller.VscrollbarPolicy = ToPolicy(_view.VerticalScrollBarVisibility);
		}

		void AttachModel()
		{
			if (_list is null || _source is null)
				return;

			if (_selection is global::Gtk.SingleSelection single)
				single.SelectionChanged -= OnSelectionChanged;
			else if (_selection is global::Gtk.MultiSelection multi)
				multi.SelectionChanged -= OnSelectionChanged;

			var selectable = _view as SelectableItemsView;
			var mode = selectable?.SelectionMode ?? SelectionMode.None;

			switch (mode)
			{
				case SelectionMode.Single:
					var singleSelection = new global::Gtk.SingleSelection(_source.Model) { Autoselect = false, CanUnselect = true };
					singleSelection.Selected = uint.MaxValue;
					singleSelection.SelectionChanged += OnSelectionChanged;
					_selection = singleSelection;
					break;
				case SelectionMode.Multiple:
					var multiSelection = new global::Gtk.MultiSelection(_source.Model);
					multiSelection.SelectionChanged += OnSelectionChanged;
					_selection = multiSelection;
					break;
				default:
					_selection = new global::Gtk.NoSelection(_source.Model);
					break;
			}

			if (_list is global::Gtk.ListView listView)
				listView.Model = _selection;
			else if (_list is global::Gtk.GridView gridView)
				gridView.Model = _selection;

			UpdateSelectionFromView();
		}

		/// <summary>Switches the selection model for the current <see cref="SelectableItemsView.SelectionMode"/>.</summary>
		public void UpdateSelectionMode() => AttachModel();

		/// <summary>Selects the rows of <see cref="SelectableItemsView.SelectedItem"/>/<see cref="SelectableItemsView.SelectedItems"/> natively.</summary>
		public void UpdateSelectionFromView()
		{
			if (_updatingSelection || _selection is null || _source is null || _view is not SelectableItemsView selectable)
				return;

			_updatingSelection = true;
			try
			{
				if (_selection is global::Gtk.SingleSelection single)
				{
					int index = selectable.SelectedItem is { } item ? _source.IndexOf(item) : -1;
					// Unselect through the invalid position: SingleSelection does not implement unselect_all before GTK 4.18, so UnselectAll is a no-op there.
					if (index >= 0)
						single.SelectItem((uint)index, true);
					else
						single.Selected = uint.MaxValue;
				}
				else if (_selection is global::Gtk.MultiSelection multi)
				{
					multi.UnselectAll();
					foreach (var item in selectable.SelectedItems)
					{
						int index = _source.IndexOf(item);
						if (index >= 0)
							multi.SelectItem((uint)index, false);
					}
				}
			}
			finally
			{
				_updatingSelection = false;
			}

			UpdateSelectionStates();
		}

		void OnSelectionChanged(object? sender, global::Gtk.SelectionChangedArgs args)
		{
			if (_updatingSelection || _source is null || _view is not SelectableItemsView selectable)
				return;

			_updatingSelection = true;
			try
			{
				if (_selection is global::Gtk.SingleSelection single)
				{
					uint position = single.Selected;
					object? item = position != uint.MaxValue && position < _source.Count && _source.RowAt((int)position).Kind == GtkRowKind.Item
						? _source.RowAt((int)position).Data
						: null;

					selectable.SelectedItem = item;
				}
				else if (_selection is global::Gtk.MultiSelection multi)
				{
					var chosen = new List<object>();
					for (int i = 0; i < _source.Count; i++)
					{
						var row = _source.RowAt(i);
						if (row.Kind == GtkRowKind.Item && multi.IsSelected((uint)i) && row.Data is not null)
							chosen.Add(row.Data);
					}

					var current = selectable.SelectedItems;
					current.Clear();
					foreach (var item in chosen)
						current.Add(item);
				}
			}
			finally
			{
				_updatingSelection = false;
			}

			UpdateSelectionStates();
		}

		void UpdateSelectionStates()
		{
			if (_selection is null)
				return;

			foreach (var host in _bound)
			{
				if (host.View is VisualElement element && host.ListItem is { } item)
				{
					bool selected = _selection.IsSelected(item.Position);
					VisualStateManager.GoToState(element, selected ? VisualStateManager.CommonStates.Selected : VisualStateManager.CommonStates.Normal);
				}
			}
		}

		// ------------------------------------------------------------------ factory

		void OnSetup(object? sender, global::Gtk.SetupArgs args)
		{
			var item = (global::Gtk.ListItem)args.Object;
			var host = new MauiGtkItemHost { FixedWidth = FixedRowWidth, ListItem = item, SharedSize = SharedSizeFor, ContentMeasured = OnHostMeasured };
			item.Child = host;

			if (_view is ReorderableItemsView)
				InstallReorder(host);
		}

		void OnBind(object? sender, global::Gtk.BindArgs args)
		{
			var item = (global::Gtk.ListItem)args.Object;
			if (item.Child is not MauiGtkItemHost host || GLib.Object.GetObject(item.Item) is not MauiRowObject rowObject)
				return;

			host.FixedWidth = FixedRowWidth;
			host.ListItem = item;
			Bind(host, rowObject.Row);
			host.Row = rowObject.Row;
			ApplySpacing(host);
			_bound.Add(host);
			UpdateSelectionStates();
		}

		void OnUnbind(object? sender, global::Gtk.UnbindArgs args)
		{
			if (((global::Gtk.ListItem)args.Object).Child is MauiGtkItemHost host)
			{
				_bound.Remove(host);
				Detach(host);
				host.Row = null;
			}
		}

		void OnTeardown(object? sender, global::Gtk.TeardownArgs args)
		{
			if (((global::Gtk.ListItem)args.Object).Child is MauiGtkItemHost host)
			{
				_bound.Remove(host);
				Detach(host);
				host.Clear();
				host.Template = null;
				host.Kind = null;
			}
		}

		void Detach(MauiGtkItemHost host)
		{
			if (host.View is Element element)
				_view.RemoveLogicalChild(element);
		}

		DataTemplate? TemplateFor(GtkRow row) => row.Kind switch
		{
			GtkRowKind.GroupHeader => (_view as GroupableItemsView)?.GroupHeaderTemplate,
			GtkRowKind.GroupFooter => (_view as GroupableItemsView)?.GroupFooterTemplate,
			_ => _view.ItemTemplate,
		};

		void Bind(MauiGtkItemHost host, GtkRow row)
		{
			var template = TemplateFor(row)?.SelectDataTemplate(row.Data, _view);

			if (template is not null && host.View is View existing && host.Kind == row.Kind && ReferenceEquals(host.Template, template))
			{
				// Recycled row of the same template: only the data changes.
				existing.BindingContext = row.Data;
				_view.AddLogicalChild(existing);
				return;
			}

			View content;
			if (template is not null)
			{
				content = template.CreateContent() as View
					?? throw new InvalidOperationException($"{template} did not create a View for {row.Kind}.");
				content.BindingContext = row.Data;
			}
			else
			{
				content = DefaultContent(row);
			}

			_view.AddLogicalChild(content);
			content.IsPlatformStateConsistent = true;
			host.SetView(content, _context);
			host.Template = template;
			host.Kind = row.Kind;
		}

		static View DefaultContent(GtkRow row)
		{
			var label = new Label { Text = row.Data?.ToString() ?? string.Empty, Padding = new Thickness(8, 6) };
			if (row.Kind != GtkRowKind.Item)
				label.FontAttributes = FontAttributes.Bold;
			return label;
		}

		// ------------------------------------------------------------------ header, footer, empty view

		/// <summary>Rebuilds the header and footer above and below the scrolling area.</summary>
		public void UpdateHeaderAndFooter()
		{
			if (_view is not StructuredItemsView structured)
				return;

			_headerHost = UpdateSlot(_headerHost, structured.Header, structured.HeaderTemplate, atStart: true);
			_footerHost = UpdateSlot(_footerHost, structured.Footer, structured.FooterTemplate, atStart: false);
		}

		MauiGtkItemHost? UpdateSlot(MauiGtkItemHost? host, object? content, DataTemplate? template, bool atStart)
		{
			var view = CreateAuxiliaryView(content, template);

			if (view is null)
			{
				if (host is not null)
				{
					Detach(host);
					host.Clear();
					_root.Remove(host);
				}

				return null;
			}

			if (host is null)
			{
				host = new MauiGtkItemHost();
				if (atStart)
					_root.Prepend(host);
				else
					_root.Append(host);
			}

			Detach(host);
			_view.AddLogicalChild(view);
			view.IsPlatformStateConsistent = true;
			host.SetView(view, _context);
			return host;
		}

		View? CreateAuxiliaryView(object? content, DataTemplate? template)
		{
			if (content is null)
				return null;

			if (content is View view)
				return view;

			if (template is not null)
			{
				var created = template.SelectDataTemplate(content, _view).CreateContent() as View;
				if (created is not null)
					created.BindingContext = content;
				return created;
			}

			return new Label { Text = content.ToString(), Padding = new Thickness(8, 6) };
		}

		/// <summary>Shows the empty view instead of the list while there are no items.</summary>
		public void UpdateEmptyView()
		{
			var view = CreateAuxiliaryView(_view.EmptyView, _view.EmptyViewTemplate);

			if (!ReferenceEquals(_emptyHost.View, view))
			{
				Detach(_emptyHost);
				if (view is null)
				{
					_emptyHost.Clear();
				}
				else
				{
					_view.AddLogicalChild(view);
					view.IsPlatformStateConsistent = true;
					_emptyHost.SetView(view, _context);
				}
			}

			bool empty = view is not null && (_source is null || _source.ItemCount == 0);
			_stack.VisibleChild = empty ? _emptyHost : _scroller;
		}

		// ------------------------------------------------------------------ scrolling

		/// <summary>Scrolls so that <paramref name="args"/>' index or item is visible, honouring its position hint.</summary>
		/// <param name="args">The request.</param>
		public void ScrollTo(ScrollToRequestEventArgs args)
		{
			if (_list is null || _source is null)
				return;

			int row = args.Mode == ScrollToMode.Position ? RowOfIndex(args.Index, args.GroupIndex) : RowOfItem(args.Item, args.Group);
			if (row < 0 || row >= _source.Count)
				return;

			var flags = global::Gtk.ListScrollFlags.None;
			if (_list is global::Gtk.ListView listView)
				listView.ScrollTo((uint)row, flags, null!);
			else if (_list is global::Gtk.GridView gridView)
				gridView.ScrollTo((uint)row, flags, null!);

			if (args.ScrollToPosition != ScrollToPosition.MakeVisible)
				AlignRow(_source.RowAt(row), args.ScrollToPosition, attemptsLeft: 30);
		}

		int RowOfIndex(int index, int groupIndex)
		{
			if (_source is null)
				return -1;

			if (groupIndex < 0)
			{
				// A flat index counts item rows only.
				int seen = 0;
				for (int i = 0; i < _source.Count; i++)
				{
					if (_source.RowAt(i).Kind != GtkRowKind.Item)
						continue;

					if (seen++ == index)
						return i;
				}

				return -1;
			}

			int group = -1;
			object? current = null;
			int within = -1;
			for (int i = 0; i < _source.Count; i++)
			{
				var row = _source.RowAt(i);
				if (!ReferenceEquals(row.Group, current))
				{
					current = row.Group;
					group++;
					within = -1;
				}

				if (row.Kind == GtkRowKind.Item)
					within++;

				if (group == groupIndex && (index < 0 ? row.Kind == GtkRowKind.GroupHeader || row.Kind == GtkRowKind.Item : within == index && row.Kind == GtkRowKind.Item))
					return i;
			}

			return -1;
		}

		int RowOfItem(object? item, object? group)
		{
			if (_source is null)
				return -1;

			for (int i = 0; i < _source.Count; i++)
			{
				var row = _source.RowAt(i);
				if (row.Kind == GtkRowKind.Item && Equals(row.Data, item) && (group is null || ReferenceEquals(row.Group, group)))
					return i;
			}

			return -1;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_scroller.Hadjustment.ValueChanged -= OnAdjustmentValueChanged;
			_scroller.Vadjustment.ValueChanged -= OnAdjustmentValueChanged;
			_scroller.Hadjustment.Changed -= OnAdjustmentBoundsChanged;
			_scroller.Vadjustment.Changed -= OnAdjustmentBoundsChanged;
			_source?.Detach();

			foreach (var host in new List<MauiGtkItemHost>(_bound))
				Detach(host);
			_bound.Clear();

			if (_headerHost is not null)
				Detach(_headerHost);
			if (_footerHost is not null)
				Detach(_footerHost);
			Detach(_emptyHost);
			_emptyHost.Clear();
		}
	}
}
