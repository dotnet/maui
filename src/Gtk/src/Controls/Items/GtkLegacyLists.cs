#pragma warning disable CS0618 // ListView, TableView and the cells are obsolete in favour of CollectionView, but still supported.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Handlers;
using GtkBox = Gtk.Box;

namespace Microsoft.Maui.Controls.Gtk.Items
{
	/// <summary>Turns the cells of the legacy <see cref="ListView"/> and <see cref="TableView"/> into views.</summary>
	static class GtkCellViews
	{
		/// <summary>The view that shows <paramref name="cell"/>; text, detail, switch and entry state stay bound to the cell.</summary>
		/// <param name="cell">The cell (its binding context is already set).</param>
		public static View ToView(Cell cell)
		{
			switch (cell)
			{
				case ViewCell viewCell when viewCell.View is { } view:
					return view;

				case EntryCell entryCell:
					{
						var label = new Label { VerticalOptions = LayoutOptions.Center, WidthRequest = 120 };
						label.SetBinding(Label.TextProperty, new Binding(nameof(EntryCell.Label), BindingMode.OneWay, source: entryCell));
						label.SetBinding(Label.TextColorProperty, new Binding(nameof(EntryCell.LabelColor), BindingMode.OneWay, source: entryCell));
						var entry = new Entry { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Center };
						entry.SetBinding(Entry.TextProperty, new Binding(nameof(EntryCell.Text), BindingMode.TwoWay, source: entryCell));
						entry.SetBinding(Entry.PlaceholderProperty, new Binding(nameof(EntryCell.Placeholder), BindingMode.OneWay, source: entryCell));
						entry.SetBinding(Entry.KeyboardProperty, new Binding(nameof(EntryCell.Keyboard), BindingMode.OneWay, source: entryCell));
						return Row(label, entry);
					}

				case SwitchCell switchCell:
					{
						var label = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Fill };
						label.SetBinding(Label.TextProperty, new Binding(nameof(SwitchCell.Text), BindingMode.OneWay, source: switchCell));
						var toggle = new Switch { VerticalOptions = LayoutOptions.Center };
						toggle.SetBinding(Switch.IsToggledProperty, new Binding(nameof(SwitchCell.On), BindingMode.TwoWay, source: switchCell));
						toggle.SetBinding(Switch.OnColorProperty, new Binding(nameof(SwitchCell.OnColor), BindingMode.OneWay, source: switchCell));
						return Row(label, toggle);
					}

				case TextCell textCell:
					{
						var text = new Label { LineBreakMode = LineBreakMode.TailTruncation };
						text.SetBinding(Label.TextProperty, new Binding(nameof(TextCell.Text), BindingMode.OneWay, source: textCell));
						text.SetBinding(Label.TextColorProperty, new Binding(nameof(TextCell.TextColor), BindingMode.OneWay, source: textCell));
						var detail = new Label { FontSize = 12, LineBreakMode = LineBreakMode.TailTruncation };
						detail.SetBinding(Label.TextProperty, new Binding(nameof(TextCell.Detail), BindingMode.OneWay, source: textCell));
						detail.SetBinding(Label.TextColorProperty, new Binding(nameof(TextCell.DetailColor), BindingMode.OneWay, source: textCell));

						var column = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { text, detail } };

						if (textCell is ImageCell imageCell)
						{
							var image = new Image { WidthRequest = 40, HeightRequest = 40, VerticalOptions = LayoutOptions.Center };
							image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageCell.ImageSource), BindingMode.OneWay, source: imageCell));
							return Row(image, column);
						}

						return column;
					}

				default:
					return new Label { Text = cell.BindingContext?.ToString() ?? string.Empty };
			}
		}

		static HorizontalStackLayout Row(View first, View second) =>
			new() { Spacing = 12, VerticalOptions = LayoutOptions.Center, Children = { first, second } };
	}

	/// <summary>
	/// The handler of the legacy <see cref="ListView"/> on GTK 4: a <c>Gtk.ListView</c> over the same C# <c>GListModel</c> as the
	/// CollectionView, with each row a cell (view cell, text, image, switch or entry cell) shown as a view.
	/// </summary>
	/// <remarks>
	/// Supported: items source and cell templates, selection (<see cref="ListView.SelectedItem"/>, <c>ItemSelected</c>, <c>ItemTapped</c>),
	/// header and footer, grouping (a bold header row with the group's string), row height and separators.
	/// Not supported: context actions, pull to refresh, uneven row heights beyond what the cell measures.
	/// </remarks>
	public class GtkListViewHandler : ViewHandler<ListView, GtkBox>
	{
		readonly global::Gtk.SignalListItemFactory _factory = new();
		readonly global::Gtk.ScrolledWindow _scroller = new() { Hexpand = true, Vexpand = true, PropagateNaturalHeight = false };
		readonly HashSet<MauiGtkItemHost> _bound = new();
		GtkItemsSource? _source;
		global::Gtk.ListView? _list;
		global::Gtk.SingleSelection? _selection;
		MauiGtkItemHost? _headerHost;
		MauiGtkItemHost? _footerHost;
		bool _updating;

		/// <summary>Creates the handler with the Controls mapper.</summary>
		public GtkListViewHandler()
			: base(ControlsMapper)
		{
		}

		/// <summary>Creates the handler with a custom mapper.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the Controls one.</param>
		public GtkListViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? ControlsMapper)
		{
		}

		internal GtkItemsSource? Source => _source;

		internal global::Gtk.ListView? List => _list;

		internal global::Gtk.ScrolledWindow Scroller => _scroller;

		internal global::Gtk.SingleSelection? Selection => _selection;

		internal IReadOnlyCollection<MauiGtkItemHost> BoundHosts => _bound;

		internal MauiGtkItemHost? HeaderHost => _headerHost;

		internal MauiGtkItemHost? FooterHost => _footerHost;

		/// <inheritdoc/>
		protected override GtkBox CreatePlatformView()
		{
			var box = new GtkBox(global::Gtk.Orientation.Vertical, 0) { Hexpand = true, Vexpand = true };
			box.Append(_scroller);

			_factory.Setup += (_, args) => ((global::Gtk.ListItem)args.Object).Child = new MauiGtkItemHost();
			_factory.Bind += OnBind;
			_factory.Unbind += (_, args) =>
			{
				if (((global::Gtk.ListItem)args.Object).Child is MauiGtkItemHost host)
				{
					_bound.Remove(host);
					Release(host);
				}
			};
			_factory.Teardown += (_, args) =>
			{
				if (((global::Gtk.ListItem)args.Object).Child is MauiGtkItemHost host)
				{
					_bound.Remove(host);
					Release(host);
					host.Clear();
				}
			};

			_list = new global::Gtk.ListView(null!, _factory) { SingleClickActivate = true, ShowSeparators = true };
			_list.Activate += OnActivate;
			_scroller.Child = _list;
			return box;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(GtkBox platformView)
		{
			_source?.Detach();
			foreach (var host in new List<MauiGtkItemHost>(_bound))
				Release(host);
			_bound.Clear();
			base.DisconnectHandler(platformView);
		}

		void Release(MauiGtkItemHost host)
		{
			if (host.View is Element element)
				VirtualView.RemoveLogicalChild(element);
		}

		bool IsGrouped => VirtualView.IsGroupingEnabled;

		internal void UpdateItemsSource()
		{
			_source?.Detach();
			_source = new GtkItemsSource(VirtualView.ItemsSource, IsGrouped, groupHeaders: IsGrouped);

			var selection = new global::Gtk.SingleSelection(_source.Model) { Autoselect = false, CanUnselect = true };
			selection.Selected = uint.MaxValue;
			selection.SelectionChanged += OnSelectionChanged;
			_selection = selection;
			_list!.Model = selection;

			UpdateSelectedItem();
		}

		internal void UpdateSelectedItem()
		{
			if (_updating || _selection is null || _source is null)
				return;

			_updating = true;
			try
			{
				int index = VirtualView.SelectedItem is { } item ? _source.IndexOf(item) : -1;
				if (index >= 0)
					_selection.SelectItem((uint)index, true);
				else
					_selection.UnselectAll();
			}
			finally
			{
				_updating = false;
			}
		}

		void OnSelectionChanged(object? sender, global::Gtk.SelectionChangedArgs args)
		{
			if (_updating || _selection is null || _source is null)
				return;

			uint position = _selection.Selected;
			if (position == uint.MaxValue || position >= _source.Count || _source.RowAt((int)position).Kind != GtkRowKind.Item)
				return;

			_updating = true;
			try
			{
				VirtualView.SelectedItem = _source.RowAt((int)position).Data;
			}
			finally
			{
				_updating = false;
			}
		}

		void OnActivate(object? sender, global::Gtk.ActivateArgs args)
		{
			if (_source is null || args.Position >= _source.Count)
				return;

			var row = _source.RowAt((int)args.Position);
			if (row.Kind != GtkRowKind.Item)
				return;

			foreach (var host in _bound)
			{
				if (host.ListItem?.Position == args.Position && host.Template is null && host.Tag is Cell cell)
				{
					NotifyTapped(row, cell);
					return;
				}
			}
		}

		void NotifyTapped(GtkRow row, Cell cell)
		{
			var controller = (IListViewController)VirtualView;
			if (IsGrouped && row.Group is not null)
			{
				int group = 0, within = 0;
				for (int i = 0; i < _source!.Count; i++)
				{
					var other = _source.RowAt(i);
					if (ReferenceEquals(other, row))
						break;

					if (other.Kind == GtkRowKind.GroupHeader)
					{
						group++;
						within = 0;
					}
					else if (other.Kind == GtkRowKind.Item)
					{
						within++;
					}
				}

				controller.NotifyRowTapped(Math.Max(0, group - 1), within, cell);
			}
			else
			{
				int index = 0;
				for (int i = 0; i < _source!.Count; i++)
				{
					if (ReferenceEquals(_source.RowAt(i), row))
						break;
					index++;
				}

				controller.NotifyRowTapped(index, cell);
			}

			cell.GetType().GetMethod("OnTapped", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.Invoke(cell, null);
		}

		void OnBind(object? sender, global::Gtk.BindArgs args)
		{
			var item = (global::Gtk.ListItem)args.Object;
			if (item.Child is not MauiGtkItemHost host || GLib.Object.GetObject(item.Item) is not MauiRowObject rowObject)
				return;

			Release(host);
			host.ListItem = item;

			var row = rowObject.Row;
			View content;
			Cell? cell = null;

			if (row.Kind == GtkRowKind.GroupHeader)
			{
				content = new Label
				{
					Text = GroupText(row.Data),
					FontAttributes = FontAttributes.Bold,
					Padding = new Thickness(8, 6),
				};
			}
			else
			{
				var template = VirtualView.ItemTemplate?.SelectDataTemplate(row.Data, VirtualView);
				cell = template?.CreateContent() as Cell ?? new TextCell { Text = row.Data?.ToString() };
				cell.BindingContext = row.Data;
				VirtualView.AddLogicalChild(cell);
				content = GtkCellViews.ToView(cell);
			}

			if (VirtualView.RowHeight > 0 && !VirtualView.HasUnevenRows)
				content.HeightRequest = VirtualView.RowHeight;

			// A view cell's view belongs to the cell (that is where its binding context comes from); other views belong to the list.
			if (cell is not ViewCell)
				VirtualView.AddLogicalChild(content);
			content.IsPlatformStateConsistent = true;
			host.SetView(content, MauiContext!);
			host.Tag = cell;
			if (cell is not null)
			{
				host.MarginStart = host.MarginEnd = 8;
				host.MarginTop = host.MarginBottom = 6;
			}
			else
			{
				host.MarginStart = host.MarginEnd = host.MarginTop = host.MarginBottom = 0;
			}
			_bound.Add(host);
		}

		string GroupText(object? group)
		{
			if (group is null)
				return string.Empty;

			if (VirtualView.GroupDisplayBinding is { } binding)
			{
				var probe = new Label { BindingContext = group };
				probe.SetBinding(Label.TextProperty, binding.Clone());
				return probe.Text ?? string.Empty;
			}

			return group.ToString() ?? string.Empty;
		}

		internal void UpdateHeaderAndFooter()
		{
			_headerHost = UpdateSlot(_headerHost, VirtualView.Header, VirtualView.HeaderTemplate, atStart: true);
			_footerHost = UpdateSlot(_footerHost, VirtualView.Footer, VirtualView.FooterTemplate, atStart: false);
		}

		MauiGtkItemHost? UpdateSlot(MauiGtkItemHost? host, object? content, DataTemplate? template, bool atStart)
		{
			View? view = content switch
			{
				null => null,
				View v => v,
				_ when template is not null => (template.SelectDataTemplate(content, VirtualView).CreateContent() as ViewCell)?.View
					?? template.SelectDataTemplate(content, VirtualView).CreateContent() as View,
				_ => new Label { Text = content.ToString(), Padding = new Thickness(8, 6) },
			};

			if (view is not null && template is not null && content is not View)
				view.BindingContext = content;

			if (view is null)
			{
				if (host is not null)
				{
					host.Clear();
					PlatformView.Remove(host);
				}

				return null;
			}

			if (host is null)
			{
				host = new MauiGtkItemHost();
				if (atStart)
					PlatformView.Prepend(host);
				else
					PlatformView.Append(host);
			}

			VirtualView.AddLogicalChild(view);
			view.IsPlatformStateConsistent = true;
			host.SetView(view, MauiContext!);
			return host;
		}

		internal void UpdateSeparators() => _list!.ShowSeparators = VirtualView.SeparatorVisibility != SeparatorVisibility.None;

		internal static readonly PropertyMapper<ListView, GtkListViewHandler> ControlsMapper = CreateMapper();

		static PropertyMapper<ListView, GtkListViewHandler> CreateMapper()
		{
			var stock = new PropertyMapper<ListView, GtkListViewHandler>(ViewHandler.ViewMapper);
			var mapper = new GtkMapperBuilder<ListView, GtkListViewHandler>(stock)
				.AddViewMappings()
				.MapGtk<GtkBox>(nameof(ListView.ItemsSource), (h, v, w) => h.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ListView.ItemTemplate), (h, v, w) => h.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ListView.IsGroupingEnabled), (h, v, w) => h.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ListView.GroupDisplayBinding), (h, v, w) => h.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ListView.RowHeight), (h, v, w) => h.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ListView.HasUnevenRows), (h, v, w) => h.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ListView.SelectedItem), (h, v, w) => h.UpdateSelectedItem())
				.MapGtk<GtkBox>(nameof(ListView.Header), (h, v, w) => h.UpdateHeaderAndFooter())
				.MapGtk<GtkBox>(nameof(ListView.HeaderTemplate), (h, v, w) => h.UpdateHeaderAndFooter())
				.MapGtk<GtkBox>(nameof(ListView.Footer), (h, v, w) => h.UpdateHeaderAndFooter())
				.MapGtk<GtkBox>(nameof(ListView.FooterTemplate), (h, v, w) => h.UpdateHeaderAndFooter())
				.MapGtk<GtkBox>(nameof(ListView.SeparatorVisibility), (h, v, w) => h.UpdateSeparators())
				.Build();

			return (PropertyMapper<ListView, GtkListViewHandler>)mapper;
		}
	}

	/// <summary>
	/// The handler of the legacy <see cref="TableView"/> on GTK 4: a scrolled column with a bold title per section and one row per cell.
	/// Cells are shown as views (see <see cref="GtkCellViews"/>); tapping a cell runs its tap behaviour.
	/// </summary>
	public class GtkTableViewHandler : ViewHandler<TableView, global::Gtk.ScrolledWindow>
	{
		readonly GtkBox _content = new(global::Gtk.Orientation.Vertical, 0) { Hexpand = true };
		readonly List<MauiGtkItemHost> _hosts = new();

		/// <summary>Creates the handler with the Controls mapper.</summary>
		public GtkTableViewHandler()
			: base(ControlsMapper)
		{
		}

		/// <summary>Creates the handler with a custom mapper.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the Controls one.</param>
		public GtkTableViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? ControlsMapper)
		{
		}

		internal IReadOnlyList<MauiGtkItemHost> Hosts => _hosts;

		/// <inheritdoc/>
		protected override global::Gtk.ScrolledWindow CreatePlatformView() =>
			new() { Hexpand = true, Vexpand = true, PropagateNaturalHeight = false, Child = _content };

		/// <inheritdoc/>
		protected override void DisconnectHandler(global::Gtk.ScrolledWindow platformView)
		{
			Clear();
			base.DisconnectHandler(platformView);
		}

		void Clear()
		{
			foreach (var host in _hosts)
			{
				if (host.View is Element element)
					VirtualView?.RemoveLogicalChild(element);
				host.Clear();
				_content.Remove(host);
			}

			_hosts.Clear();
		}

		internal void UpdateRoot()
		{
			Clear();

			var root = VirtualView.Root;
			if (root is null)
				return;

			foreach (var section in root)
			{
				if (!string.IsNullOrEmpty(section.Title))
					Add(new Label { Text = section.Title, FontAttributes = FontAttributes.Bold, Padding = new Thickness(8, 12, 8, 4) }, null);

				foreach (var cell in section)
				{
					VirtualView.AddLogicalChild(cell);
					var view = GtkCellViews.ToView(cell);
					if (VirtualView.RowHeight > 0 && !VirtualView.HasUnevenRows)
						view.HeightRequest = VirtualView.RowHeight;
					Add(view, cell);
				}
			}
		}

		void Add(View view, Cell? cell)
		{
			VirtualView.AddLogicalChild(view);
			view.IsPlatformStateConsistent = true;

			var host = new MauiGtkItemHost { Tag = cell };
			host.SetView(view, MauiContext!);

			if (cell is not null)
			{
				host.MarginStart = host.MarginEnd = 8;
				host.MarginTop = host.MarginBottom = 6;
				var click = new global::Gtk.GestureClick();
				click.Released += (_, _) =>
					cell.GetType().GetMethod("OnTapped", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.Invoke(cell, null);
				host.AddController(click);
			}

			_content.Append(host);
			_hosts.Add(host);
		}

		internal static readonly PropertyMapper<TableView, GtkTableViewHandler> ControlsMapper = CreateMapper();

		static PropertyMapper<TableView, GtkTableViewHandler> CreateMapper()
		{
			var stock = new PropertyMapper<TableView, GtkTableViewHandler>(ViewHandler.ViewMapper);
			var mapper = new GtkMapperBuilder<TableView, GtkTableViewHandler>(stock)
				.AddViewMappings()
				.MapGtk<global::Gtk.ScrolledWindow>(nameof(TableView.Root), (h, v, w) => h.UpdateRoot())
				.MapGtk<global::Gtk.ScrolledWindow>(nameof(TableView.RowHeight), (h, v, w) => h.UpdateRoot())
				.MapGtk<global::Gtk.ScrolledWindow>(nameof(TableView.HasUnevenRows), (h, v, w) => h.UpdateRoot())
				.Build();

			return (PropertyMapper<TableView, GtkTableViewHandler>)mapper;
		}
	}
}
