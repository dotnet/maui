using System;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkEntry = Gtk.Entry;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="EntryHandler"/> over a <c>Gtk.Entry</c>. Text and cursor changes flow back to the virtual view and
	/// <c>activate</c> raises <c>Completed</c>.
	/// </summary>
	public class GtkEntryHandler : EntryHandler
	{
		const string ClearIcon = "edit-clear-symbolic";

		bool _updatingFromVirtualView;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkEntryHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkEntryHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkEntry();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not GtkEntry entry)
				return;

			entry.Changed += OnChanged;
			entry.Activated += OnActivated;
			entry.IconPress += OnIconPress;
			entry.AddNotification("cursor-position", OnCursorPositionChanged);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkEntry entry)
			{
				entry.Changed -= OnChanged;
				entry.Activated -= OnActivated;
				entry.IconPress -= OnIconPress;
				entry.RemoveNotification("cursor-position", OnCursorPositionChanged);
			}

			base.DisconnectHandler(platformView);
		}

		void OnChanged(object? sender, EventArgs e)
		{
			if (_updatingFromVirtualView || sender is not GtkEntry entry || View is not { } view)
				return;

			var text = entry.Text ?? string.Empty;
			if (!string.Equals(view.Text, text, StringComparison.Ordinal))
				view.Text = text;
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IEntry? View => ((IElementHandler)this).VirtualView as IEntry;

		void OnActivated(object? sender, EventArgs e) => View?.Completed();

		void OnIconPress(object o, global::Gtk.IconPressArgs args)
		{
			if (o is GtkEntry entry && args.IconPos == global::Gtk.EntryIconPosition.Secondary)
				entry.Text = string.Empty;
		}

		void OnCursorPositionChanged(object o, GLib.NotifyArgs args)
		{
			if (_updatingFromVirtualView || ((IElementHandler)this).PlatformView is not GtkEntry entry || View is not { } view)
				return;

			if (view.CursorPosition != entry.Position)
				view.CursorPosition = entry.Position;
		}

		internal static readonly IPropertyMapper<IEntry, IEntryHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IEntry, IEntryHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IEntry, IEntryHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IEntry, IEntryHandler>(EntryHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IEntry, IEntryHandler>(EntryHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IEntry, IEntryHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IEntry, IEntryHandler>(EntryHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IEntry, IEntryHandler>(EntryHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IEntry, IEntryHandler> mapper, GtkCommandMapperBuilder<IEntry, IEntryHandler> commands)
		{
			mapper.MapGtk<GtkEntry>(nameof(IEntry.Text), (h, v, w) => Guard(h, () =>
			{
				var text = v.Text ?? string.Empty;
				if (!string.Equals(w.Text, text, StringComparison.Ordinal))
					w.Text = text;
			}));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, v.TextColor));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.Font), (h, v, w) => GtkTextStyle.SetFont(w, v.Font, GtkTextStyle.GetFontManager(h)));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w, v.CharacterSpacing));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.Background), (h, v, w) => GtkStyle.For(w).SetBackground(v.Background));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.Placeholder), (h, v, w) => w.PlaceholderText = v.Placeholder ?? string.Empty);
			mapper.MapGtk<GtkEntry>(nameof(IEntry.PlaceholderColor), (h, v, w) => GtkTextStyle.SetColor(w, v.PlaceholderColor, " placeholder"));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.IsPassword), (h, v, w) => w.Visibility = !v.IsPassword);
			mapper.MapGtk<GtkEntry>(nameof(IEntry.IsReadOnly), (h, v, w) => w.IsEditable = !v.IsReadOnly);
			mapper.MapGtk<GtkEntry>(nameof(IEntry.MaxLength), (h, v, w) =>
				w.MaxLength = v.MaxLength > 0 && v.MaxLength < ushort.MaxValue ? v.MaxLength : 0);
			mapper.MapGtk<GtkEntry>(nameof(IEntry.HorizontalTextAlignment), (h, v, w) =>
				w.Alignment = GtkTextStyle.ToAlignment(v.HorizontalTextAlignment, v.FlowDirection));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.VerticalTextAlignment), (h, v, w) => { /* GtkEntry always centres its text vertically */ });
			mapper.MapGtk<GtkEntry>(nameof(IEntry.Keyboard), (h, v, w) => GtkInputExtensions.UpdateKeyboard(w, v.Keyboard, v));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.IsTextPredictionEnabled), (h, v, w) => GtkInputExtensions.UpdateKeyboard(w, v.Keyboard, v));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.IsSpellCheckEnabled), (h, v, w) => GtkInputExtensions.UpdateKeyboard(w, v.Keyboard, v));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.ReturnType), (h, v, w) => { /* no GTK equivalent */ });
			mapper.MapGtk<GtkEntry>(nameof(IEntry.ClearButtonVisibility), (h, v, w) => UpdateClearButton(w, v));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.CursorPosition), (h, v, w) => Guard(h, () =>
			{
				int position = Math.Max(0, Math.Min(v.CursorPosition, (w.Text ?? string.Empty).Length));
				w.Position = position;
			}));
			mapper.MapGtk<GtkEntry>(nameof(IEntry.SelectionLength), (h, v, w) => Guard(h, () =>
			{
				int length = (w.Text ?? string.Empty).Length;
				int start = Math.Max(0, Math.Min(v.CursorPosition, length));
				int end = Math.Max(start, Math.Min(start + v.SelectionLength, length));
				w.SelectRegion(start, end);
			}));
		}

		static void Guard(IEntryHandler handler, Action action)
		{
			if (handler is GtkEntryHandler gtk)
			{
				bool previous = gtk._updatingFromVirtualView;
				gtk._updatingFromVirtualView = true;
				try
				{
					action();
				}
				finally
				{
					gtk._updatingFromVirtualView = previous;
				}
			}
			else
			{
				action();
			}
		}

		static void UpdateClearButton(GtkEntry entry, IEntry view)
		{
			bool show = view.ClearButtonVisibility == ClearButtonVisibility.WhileEditing;
			entry.SetIconFromIconName(global::Gtk.EntryIconPosition.Secondary, show ? ClearIcon : null);
		}
	}
}
