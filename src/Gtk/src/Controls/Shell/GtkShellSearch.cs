using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Controls.Gtk.Items;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Gtk.Platform;
using GtkBox = Gtk.Box;
using GtkListBox = Gtk.ListBox;
using GtkPopover = Gtk.Popover;
using GtkSearchEntry = Gtk.SearchEntry;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	/// <summary>
	/// The search box of a Shell page's <see cref="SearchHandler"/>: a <c>Gtk.SearchEntry</c> bar above the page and, while
	/// <see cref="SearchHandler.ShowsResults"/> is set, a <c>Gtk.ListBox</c> of the handler's suggestions in a popover under the entry.
	/// </summary>
	/// <remarks>
	/// The entry follows <see cref="SearchHandler.Query"/> both ways; Enter confirms the query, and picking a suggestion reports it through
	/// <see cref="ISearchHandlerController.ItemSelected"/>. Suggestions use <see cref="SearchHandler.ItemTemplate"/>, else
	/// <see cref="SearchHandler.DisplayMemberName"/>, else <c>ToString()</c>.
	/// </remarks>
	sealed class GtkShellSearch
	{
		readonly GtkBox _bar = new(global::Gtk.Orientation.Horizontal, 6)
		{
			Visible = false,
			MarginStart = 8,
			MarginEnd = 8,
			MarginTop = 6,
			MarginBottom = 6,
		};

		readonly GtkSearchEntry _entry = new() { Hexpand = true };
		readonly GtkListBox _results = new() { SelectionMode = global::Gtk.SelectionMode.Single };
		readonly List<object> _items = new();
		readonly List<View> _views = new();
		readonly Element _owner;
		readonly Func<IMauiContext?> _context;
		GtkPopover? _popover;
		SearchHandler? _handler;
		INotifyCollectionChanged? _source;
		bool _updating;

		public GtkShellSearch(Element owner, Func<IMauiContext?> context)
		{
			_owner = owner;
			_context = context;
			_entry.Changed += OnEntryChanged;
			_entry.Activate += OnEntryActivated;
			_results.RowSelected += OnRowSelected;
			_bar.Append(_entry);
		}

		/// <summary>The bar to put above the page.</summary>
		public global::Gtk.Widget Bar => _bar;

		/// <summary>The search entry.</summary>
		public GtkSearchEntry Entry => _entry;

		/// <summary>The list of suggestions.</summary>
		public GtkListBox Results => _results;

		/// <summary>The popover that shows the suggestions, or <see langword="null"/> while it has not been needed.</summary>
		public GtkPopover? Popover => _popover;

		/// <summary>The handler currently shown, or <see langword="null"/>.</summary>
		public SearchHandler? Handler => _handler;

		/// <summary>Shows the search box of <paramref name="handler"/> (<see langword="null"/> hides it).</summary>
		public void SetHandler(SearchHandler? handler)
		{
			if (ReferenceEquals(handler, _handler))
			{
				Refresh();
				return;
			}

			if (_handler is not null)
				Detach(_handler);

			_handler = handler;
			if (handler is not null)
				Attach(handler);

			Refresh();
		}

		/// <summary>Disconnects from the handler and drops the suggestions.</summary>
		public void Dispose()
		{
			SetHandler(null);
			_entry.Changed -= OnEntryChanged;
			_entry.Activate -= OnEntryActivated;
			_results.RowSelected -= OnRowSelected;
			_popover?.Unparent();
			_popover = null;
		}

		void Attach(SearchHandler handler)
		{
			handler.PropertyChanged += OnHandlerPropertyChanged;
			((ISearchHandlerController)handler).ListProxyChanged += OnListProxyChanged;
			ObserveSource(handler.ItemsSource);
		}

		void Detach(SearchHandler handler)
		{
			handler.PropertyChanged -= OnHandlerPropertyChanged;
			((ISearchHandlerController)handler).ListProxyChanged -= OnListProxyChanged;
			ObserveSource(null);
			ClearResults();
			HidePopover();
		}

		void ObserveSource(IEnumerable? source)
		{
			if (_source is not null)
				_source.CollectionChanged -= OnSourceChanged;

			_source = source as INotifyCollectionChanged;
			if (_source is not null)
				_source.CollectionChanged += OnSourceChanged;
		}

		void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildResults();

		void OnListProxyChanged(object? sender, ListProxyChangedEventArgs e) => RebuildResults();

		void OnHandlerPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (_handler is null)
				return;

			if (e.PropertyName == SearchHandler.ItemsSourceProperty.PropertyName)
				ObserveSource(_handler.ItemsSource);

			if (e.PropertyName == SearchHandler.ItemTemplateProperty.PropertyName
				|| e.PropertyName == "DisplayMemberName"
				|| e.PropertyName == SearchHandler.ItemsSourceProperty.PropertyName
				|| e.PropertyName == SearchHandler.QueryProperty.PropertyName)
			{
				RebuildResults();
			}

			Refresh();
		}

		/// <summary>Applies the handler's current property values to the entry.</summary>
		void Refresh()
		{
			var handler = _handler;
			bool visible = handler is not null && handler.SearchBoxVisibility != SearchBoxVisibility.Hidden;
			_bar.Visible = visible;

			if (handler is null)
				return;

			_updating = true;
			try
			{
				string query = handler.Query ?? string.Empty;
				if (_entry.Text != query)
					_entry.Text = query;

				_entry.PlaceholderText = handler.Placeholder ?? string.Empty;
				_entry.Sensitive = handler.IsSearchEnabled;
				if (!string.IsNullOrEmpty(handler.AutomationId))
					_entry.Name = handler.AutomationId;
			}
			finally
			{
				_updating = false;
			}

			Microsoft.Maui.Gtk.Platform.GtkTextStyle.SetColor(_entry, handler.TextColor);
			Microsoft.Maui.Gtk.Platform.GtkStyle.For(_bar).SetBackground(handler.BackgroundColor is { } color ? new Microsoft.Maui.Graphics.SolidPaint { Color = color } : null);
			UpdatePopover();
		}

		void OnEntryChanged(object? sender, EventArgs e)
		{
			if (_updating || _handler is null)
				return;

			string text = _entry.Text ?? string.Empty;
			if (_handler.Query != text)
				_handler.Query = text;
		}

		void OnEntryActivated(object? sender, EventArgs e)
		{
			if (_handler is { } handler)
				((ISearchHandlerController)handler).QueryConfirmed();
		}

		void OnRowSelected(object? sender, global::Gtk.RowSelectedArgs args)
		{
			if (_updating || _handler is not { } handler)
				return;

			int index = args.Row?.Index ?? -1;
			if (index < 0 || index >= _items.Count)
				return;

			((ISearchHandlerController)handler).ItemSelected(_items[index]);
			HidePopover();

			// Picking the same suggestion again must select it again.
			_updating = true;
			try
			{
				_results.UnselectAll();
			}
			finally
			{
				_updating = false;
			}
		}

		// ------------------------------------------------------------------ suggestions

		void RebuildResults()
		{
			ClearResults();

			var handler = _handler;
			var context = _context();
			if (handler is null || context is null)
				return;

			var proxy = ((ISearchHandlerController)handler).ListProxy;
			if (proxy is not null)
			{
				for (int i = 0; i < proxy.Count; i++)
				{
					var item = proxy[i];
					_items.Add(item);
					_results.Append(new global::Gtk.ListBoxRow { Child = CreateRow(handler, item, context) });
				}
			}

			UpdatePopover();
		}

		void ClearResults()
		{
			_results.RemoveAll();
			_items.Clear();
			foreach (var view in _views)
				_owner.RemoveLogicalChild(view);
			_views.Clear();
		}

		global::Gtk.Widget CreateRow(SearchHandler handler, object item, IMauiContext context)
		{
			View view;
			if (handler.ItemTemplate is { } template)
			{
				view = (View)template.SelectDataTemplate(item, _owner).CreateContent();
				view.BindingContext = item;
			}
			else
			{
				var label = new Label { Padding = new Thickness(10, 8) };
#pragma warning disable CS0618 // DisplayMemberName is obsolete but still honoured, as on the other platforms.
				string? member = handler.DisplayMemberName;
#pragma warning restore CS0618
				if (!string.IsNullOrEmpty(member))
				{
					label.SetBinding(Label.TextProperty, new Binding(member));
					label.BindingContext = item;
				}
				else
				{
					label.Text = item?.ToString() ?? string.Empty;
				}

				view = label;
			}

			_owner.AddLogicalChild(view);
			_views.Add(view);
			var host = new MauiGtkItemHost();
			host.SetView(view, context);
			return host;
		}

		void UpdatePopover()
		{
			var handler = _handler;
			bool show = handler is not null && handler.ShowsResults && _bar.Visible && _items.Count > 0
				&& !string.IsNullOrEmpty(handler.Query);

			if (!show)
			{
				HidePopover();
				return;
			}

			if (_popover is null)
			{
				var scroller = new global::Gtk.ScrolledWindow
				{
					Child = _results,
					HasFrame = false,
					PropagateNaturalHeight = true,
					MaxContentHeight = 300,
					HscrollbarPolicy = global::Gtk.PolicyType.Never,
				};

				_popover = new GtkPopover { Child = scroller, Autohide = false, Focusable = false, HasArrow = false };
				_popover.Parent = _entry;
			}

			_popover.SetSizeRequest(Math.Max(_entry.Width, 200), -1);
			_popover.Popup();
		}

		void HidePopover()
		{
			if (_popover is { Visible: true })
				_popover.Popdown();
		}
	}
}
