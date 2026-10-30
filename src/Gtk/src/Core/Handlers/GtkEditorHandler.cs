using System;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// The platform view of the editor: a scrolled <c>Gtk.TextView</c> with a placeholder label overlaid while the buffer is empty.
	/// It grows with its text (natural size propagation) until the layout constrains it, then scrolls.
	/// </summary>
	public class MauiGtkEditor : global::Gtk.Overlay
	{
		readonly global::Gtk.ScrolledWindow _scroller = new();
		readonly global::Gtk.TextView _view = new();
		readonly global::Gtk.Label _placeholder = new() { Halign = global::Gtk.Align.Start, Valign = global::Gtk.Align.Start, CanTarget = false };

		/// <summary>Creates the editor.</summary>
		public MauiGtkEditor()
		{
			_view.WrapMode = global::Gtk.WrapMode.WordChar;
			_view.LeftMargin = _view.RightMargin = 6;
			_view.TopMargin = _view.BottomMargin = 6;

			_scroller.PropagateNaturalHeight = true;
			_scroller.SetPolicy(global::Gtk.PolicyType.Never, global::Gtk.PolicyType.Automatic);
			_scroller.Child = _view;

			_placeholder.MarginStart = 6;
			_placeholder.MarginTop = 6;
			_placeholder.AddCssClass("dim-label");

			Child = _scroller;
			AddOverlay(_placeholder);

			_view.Buffer.Changed += (_, _) => UpdatePlaceholderVisibility();
			UpdatePlaceholderVisibility();
		}

		/// <summary>The text view.</summary>
		public global::Gtk.TextView View => _view;

		/// <summary>The placeholder label.</summary>
		public global::Gtk.Label PlaceholderLabel => _placeholder;

		/// <summary>The scrolled window around the text view.</summary>
		public global::Gtk.ScrolledWindow Scroller => _scroller;

		/// <summary>The editor text.</summary>
		public string Text
		{
			get => _view.Buffer.Text ?? string.Empty;
			set
			{
				if (!string.Equals(_view.Buffer.Text, value, StringComparison.Ordinal))
					_view.Buffer.Text = value;
			}
		}

		void UpdatePlaceholderVisibility() =>
			_placeholder.Visible = _view.Buffer.CharCount == 0 && !string.IsNullOrEmpty(_placeholder.Text);

		/// <summary>Sets the placeholder text.</summary>
		/// <param name="text">The placeholder.</param>
		public void SetPlaceholder(string? text)
		{
			_placeholder.Text = text ?? string.Empty;
			UpdatePlaceholderVisibility();
		}
	}

	/// <summary>
	/// <see cref="EditorHandler"/> over a <see cref="MauiGtkEditor"/>.
	/// </summary>
	public class GtkEditorHandler : EditorHandler
	{
		bool _updatingFromVirtualView;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkEditorHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkEditorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkEditor();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is MauiGtkEditor editor)
			{
				editor.View.Buffer.Changed += OnChanged;
				var focus = new global::Gtk.EventControllerFocus();
				focus.Leave += OnFocusLeave;
				editor.View.AddController(focus);
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkEditor editor)
				editor.View.Buffer.Changed -= OnChanged;

			base.DisconnectHandler(platformView);
		}

		IEditor? View => ((IElementHandler)this).VirtualView as IEditor;

		void OnFocusLeave(object? sender, EventArgs e) => View?.Completed();

		void OnChanged(object? sender, EventArgs e)
		{
			if (_updatingFromVirtualView || ((IElementHandler)this).PlatformView is not MauiGtkEditor editor || View is not { } view)
				return;

			var text = editor.Text;
			int max = view.MaxLength;
			if (max > 0 && max < int.MaxValue && text.Length > max)
			{
				text = text.Substring(0, max);
				_updatingFromVirtualView = true;
				try
				{
					editor.Text = text;
				}
				finally
				{
					_updatingFromVirtualView = false;
				}
			}

			if (!string.Equals(view.Text, text, StringComparison.Ordinal))
				view.Text = text;
		}

		internal static readonly IPropertyMapper<IEditor, IEditorHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IEditor, IEditorHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IEditor, IEditorHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IEditor, IEditorHandler>(EditorHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IEditor, IEditorHandler>(EditorHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IEditor, IEditorHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IEditor, IEditorHandler>(EditorHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IEditor, IEditorHandler>(EditorHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IEditor, IEditorHandler> mapper, GtkCommandMapperBuilder<IEditor, IEditorHandler> commands)
		{
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.Text), (h, v, w) => Guard(h, () => w.Text = v.Text ?? string.Empty));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.TextColor), (h, v, w) => GtkTextStyle.SetColor(w.View, v.TextColor));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.Font), (h, v, w) =>
			{
				var fonts = GtkTextStyle.GetFontManager(h);
				GtkTextStyle.SetFont(w.View, v.Font, fonts);
				GtkTextStyle.SetFont(w.PlaceholderLabel, v.Font, fonts);
			});
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w.View, v.CharacterSpacing));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.Background), (h, v, w) => GtkStyle.For(w.View).SetBackground(v.Background, " text"));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.Placeholder), (h, v, w) => w.SetPlaceholder(v.Placeholder));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.PlaceholderColor), (h, v, w) => GtkTextStyle.SetColor(w.PlaceholderLabel, v.PlaceholderColor));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.IsReadOnly), (h, v, w) => w.View.Editable = !v.IsReadOnly);
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.MaxLength), (h, v, w) =>
			{
				// Truncate the virtual view's text (the Text mapping then updates the widget); never push the widget's text back.
				int max = v.MaxLength;
				var text = v.Text;
				if (max >= 0 && max < int.MaxValue && text is not null && text.Length > max)
					v.Text = text.Substring(0, max);
			});
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.HorizontalTextAlignment), (h, v, w) =>
				w.View.Justification = v.HorizontalTextAlignment switch
				{
					TextAlignment.Center => global::Gtk.Justification.Center,
					TextAlignment.End => global::Gtk.Justification.Right,
					_ => global::Gtk.Justification.Left,
				});
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.VerticalTextAlignment), (h, v, w) => { /* the text view fills its allocation from the top */ });
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.Keyboard), (h, v, w) => GtkInputExtensions.UpdateKeyboard(w.View, v.Keyboard, v));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.IsTextPredictionEnabled), (h, v, w) => GtkInputExtensions.UpdateKeyboard(w.View, v.Keyboard, v));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.IsSpellCheckEnabled), (h, v, w) => GtkInputExtensions.UpdateKeyboard(w.View, v.Keyboard, v));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.CursorPosition), (h, v, w) => Guard(h, () => UpdateSelection(w, v)));
			mapper.MapGtk<MauiGtkEditor>(nameof(IEditor.SelectionLength), (h, v, w) => Guard(h, () => UpdateSelection(w, v)));
		}

		static void UpdateSelection(MauiGtkEditor editor, IEditor view)
		{
			var buffer = editor.View.Buffer;
			int length = buffer.CharCount;
			int start = Math.Max(0, Math.Min(view.CursorPosition, length));
			int end = Math.Max(start, Math.Min(start + view.SelectionLength, length));

			buffer.SelectRange(buffer.GetIterAtOffset(start), buffer.GetIterAtOffset(end));
		}

		static void Guard(IEditorHandler handler, Action action)
		{
			if (handler is GtkEditorHandler gtk)
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
	}
}
