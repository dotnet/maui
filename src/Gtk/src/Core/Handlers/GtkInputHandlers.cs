using System;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using GtkSearchEntry = Gtk.SearchEntry;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="SearchBarHandler"/> over a <c>Gtk.SearchEntry</c>. Text edits flow back immediately (not through the entry's search delay),
	/// <c>activate</c> raises <c>SearchButtonPressed</c>; the built-in clear icon is the cancel button.
	/// </summary>
	/// <remarks><c>ReturnType</c> and the vertical text alignment have no GTK equivalent. <c>MaxLength</c> is enforced by truncating on change.</remarks>
	public class GtkSearchBarHandler : SearchBarHandler
	{
		readonly GtkUpdateGuard _guard = new();

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkSearchBarHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkSearchBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkSearchEntry { Hexpand = true };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not GtkSearchEntry entry)
				return;

			entry.Changed += OnChanged;
			entry.Activate += OnActivate;
			entry.AddNotification("cursor-position", OnCursorPositionChanged);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkSearchEntry entry)
			{
				entry.Changed -= OnChanged;
				entry.Activate -= OnActivate;
				entry.RemoveNotification("cursor-position", OnCursorPositionChanged);
			}

			base.DisconnectHandler(platformView);
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		ISearchBar? View => ((IElementHandler)this).VirtualView as ISearchBar;

		void OnChanged(object? sender, EventArgs e)
		{
			if (_guard.IsActive || sender is not GtkSearchEntry entry || View is not { } view)
				return;

			var text = entry.Text ?? string.Empty;

			if (view.MaxLength > 0 && text.Length > view.MaxLength)
			{
				text = text.Substring(0, view.MaxLength);
				_guard.Run(() => entry.Text = text);
			}

			if (!string.Equals(view.Text, text, StringComparison.Ordinal))
				view.Text = text;
		}

		void OnActivate(object? sender, EventArgs e) => View?.SearchButtonPressed();

		void OnCursorPositionChanged(object o, GLib.NotifyArgs args)
		{
			if (_guard.IsActive || o is not GtkSearchEntry entry || View is not { } view)
				return;

			if (view.CursorPosition != entry.Position)
				view.CursorPosition = entry.Position;
		}

		static void Guarded(IViewHandler handler, Action action)
		{
			if (handler is GtkSearchBarHandler gtk)
				gtk._guard.Run(action);
			else
				action();
		}

		static void UpdateKeyboard(GtkSearchEntry entry, ISearchBar view)
		{
			entry.InputPurpose = GtkInputExtensions.ToInputPurpose(view.Keyboard);
			entry.InputHints = GtkInputExtensions.ToInputHints(view);
		}

		internal static readonly IPropertyMapper<ISearchBar, ISearchBarHandler> GtkMapper =
			new GtkMapperBuilder<ISearchBar, ISearchBarHandler>(SearchBarHandler.Mapper)
				.AddViewMappings()
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.Text), (h, v, w) => Guarded(h, () =>
				{
					var text = v.Text ?? string.Empty;
					if (!string.Equals(w.Text, text, StringComparison.Ordinal))
						w.Text = text;
				}))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.Placeholder), (h, v, w) => w.PlaceholderText = v.Placeholder ?? string.Empty)
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.PlaceholderColor), (h, v, w) => GtkTextStyle.SetColor(w, v.PlaceholderColor, " placeholder"))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, v.TextColor))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.Font), (h, v, w) => GtkTextStyle.SetFont(w, v.Font, GtkTextStyle.GetFontManager(h)))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w, v.CharacterSpacing))
				.MapGtk<GtkSearchEntry>(nameof(ITextAlignment.HorizontalTextAlignment), (h, v, w) =>
					w.Alignment = GtkTextStyle.ToAlignment(v.HorizontalTextAlignment, v.FlowDirection))
				.MapGtk<GtkSearchEntry>(nameof(ITextAlignment.VerticalTextAlignment), (h, v, w) => { /* the entry always centres its text vertically */ })
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.IsReadOnly), (h, v, w) => w.IsEditable = !v.IsReadOnly)
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.IsTextPredictionEnabled), (h, v, w) => UpdateKeyboard(w, v))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.IsSpellCheckEnabled), (h, v, w) => UpdateKeyboard(w, v))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.Keyboard), (h, v, w) => UpdateKeyboard(w, v))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.ReturnType), (h, v, w) => { /* no GTK equivalent */ })
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.MaxLength), (h, v, w) => Guarded(h, () =>
				{
					var text = w.Text ?? string.Empty;
					if (v.MaxLength > 0 && text.Length > v.MaxLength)
						w.Text = text.Substring(0, v.MaxLength);
				}))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.CursorPosition), (h, v, w) => Guarded(h, () =>
					w.Position = Math.Max(0, Math.Min(v.CursorPosition, (w.Text ?? string.Empty).Length))))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.SelectionLength), (h, v, w) => Guarded(h, () =>
				{
					int length = (w.Text ?? string.Empty).Length;
					int start = Math.Max(0, Math.Min(v.CursorPosition, length));
					int end = Math.Max(start, Math.Min(start + v.SelectionLength, length));
					w.SelectRegion(start, end);
				}))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.CancelButtonColor), (h, v, w) =>
					GtkStyle.For(w).Set("color", v.CancelButtonColor?.ToCss(), " image.right"))
				.MapGtk<GtkSearchEntry>(nameof(ISearchBar.SearchIconColor), (h, v, w) =>
					GtkStyle.For(w).Set("color", v.SearchIconColor?.ToCss(), " image.left"))
				.Build();

		internal static readonly CommandMapper<ISearchBar, ISearchBarHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<ISearchBar, ISearchBarHandler>(SearchBarHandler.CommandMapper).AddViewCommands().Build();
	}

	/// <summary>
	/// A radio button: a <c>Gtk.CheckButton</c> that is a member of a private radio group (so GTK draws a radio indicator). MAUI owns the
	/// grouping and the exclusivity (<c>GroupName</c>/<c>RadioButtonGroup</c>), so every button is its own native group, led by an invisible
	/// leader button that "unchecks" it.
	/// </summary>
	public sealed class MauiGtkRadioButton : global::Gtk.Box
	{
		readonly global::Gtk.CheckButton _leader = new() { Visible = false };
		readonly global::Gtk.CheckButton _button = new();
		bool _updating;

		/// <summary>Creates the radio button.</summary>
		public MauiGtkRadioButton()
			: base(global::Gtk.Orientation.Horizontal, 0)
		{
			Halign = global::Gtk.Align.Start;
			Valign = global::Gtk.Align.Center;
			Append(_leader);
			Append(_button);
			_button.Group = _leader;
			_button.Toggled += (s, e) =>
			{
				if (!_updating)
					Toggled?.Invoke(this, EventArgs.Empty);
			};
		}

		/// <summary>Raised when the user checked the button.</summary>
		public event EventHandler? Toggled;

		/// <summary>The visible radio button.</summary>
		public global::Gtk.CheckButton Button => _button;

		/// <summary>Whether the button is checked.</summary>
		public bool IsChecked
		{
			get => _button.Active;
			set
			{
				if (_button.Active == value)
					return;

				_updating = true;
				try
				{
					// A radio button is unchecked by checking another member of its group: the invisible leader.
					if (value)
						_button.Active = true;
					else
						_leader.Active = true;
				}
				finally
				{
					_updating = false;
				}
			}
		}

		/// <summary>Shows <paramref name="text"/> next to the indicator (replaces any content widget).</summary>
		/// <param name="text">The text, or <see langword="null"/> for none.</param>
		public void SetText(string? text)
		{
			_button.Child = null;
			_button.Label = text ?? string.Empty;
		}

		/// <summary>Shows <paramref name="content"/> next to the indicator, or in place of it when <paramref name="hideIndicator"/> is set.</summary>
		/// <param name="content">The content widget.</param>
		/// <param name="hideIndicator">Hides the native indicator (the content draws its own, as a control template does).</param>
		public void SetContent(global::Gtk.Widget? content, bool hideIndicator)
		{
			_button.Label = string.Empty;
			_button.Child = content;

			if (hideIndicator)
				_button.AddCssClass("maui-no-indicator");
			else
				_button.RemoveCssClass("maui-no-indicator");

			GtkStyle.For(_button).Set("min-width", hideIndicator ? "0" : null, " radio");
			GtkStyle.For(_button).Set("min-height", hideIndicator ? "0" : null, " radio");
			GtkStyle.For(_button).Set("margin", hideIndicator ? "0" : null, " radio");
			GtkStyle.For(_button).Set("border-style", hideIndicator ? "none" : null, " radio");
			GtkStyle.For(_button).Set("background-image", hideIndicator ? "none" : null, " radio");
		}
	}

	/// <summary>
	/// <see cref="RadioButtonHandler"/> over a <see cref="MauiGtkRadioButton"/>. String content is the button label; view content is shown next to the
	/// indicator; content produced by a control template (which draws its own glyph) is shown without the native indicator.
	/// </summary>
	public class GtkRadioButtonHandler : RadioButtonHandler
	{
		readonly GtkUpdateGuard _guard = new();

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkRadioButtonHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkRadioButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkRadioButton();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			if (platformView is MauiGtkRadioButton radio)
				radio.Toggled += OnToggled;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkRadioButton radio)
			{
				radio.Toggled -= OnToggled;
				radio.SetContent(null, false);
			}

			base.DisconnectHandler(platformView);
		}

		void OnToggled(object? sender, EventArgs e)
		{
			if (_guard.IsActive || sender is not MauiGtkRadioButton radio || ((IElementHandler)this).VirtualView is not IRadioButton view)
				return;

			if (view.IsChecked != radio.IsChecked)
				view.IsChecked = radio.IsChecked;
		}

		static void Guarded(IViewHandler handler, Action action)
		{
			if (handler is GtkRadioButtonHandler gtk)
				gtk._guard.Run(action);
			else
				action();
		}

		static void UpdateContent(IRadioButtonHandler handler, IRadioButton view, MauiGtkRadioButton radio)
		{
			var context = handler.MauiContext;
			var presented = view.PresentedContent;

			if (presented is not null && context is not null)
			{
				// The presented content is the content itself when no template is applied, or the template root when one is.
				bool templated = !ReferenceEquals(presented, view.Content);
				radio.SetContent((global::Gtk.Widget)presented.ToPlatform(context), templated);
			}
			else
			{
				radio.SetText(view.Content?.ToString());
			}
		}

		internal static readonly IPropertyMapper<IRadioButton, IRadioButtonHandler> GtkMapper =
			new GtkMapperBuilder<IRadioButton, IRadioButtonHandler>(RadioButtonHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkRadioButton>(nameof(IRadioButton.IsChecked), (h, v, w) => Guarded(h, () => w.IsChecked = v.IsChecked))
				.MapGtk<MauiGtkRadioButton>(nameof(IRadioButton.Content), (h, v, w) => UpdateContent(h, v, w))
				.MapGtk<MauiGtkRadioButton>(nameof(ITextStyle.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, v.TextColor))
				.MapGtk<MauiGtkRadioButton>(nameof(ITextStyle.Font), (h, v, w) => GtkTextStyle.SetFont(w, v.Font, GtkTextStyle.GetFontManager(h)))
				.MapGtk<MauiGtkRadioButton>(nameof(ITextStyle.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w, v.CharacterSpacing))
				.MapGtk<MauiGtkRadioButton>(nameof(IRadioButton.StrokeColor), (h, v, w) => GtkButtonStyle.UpdateStroke(w, v))
				.MapGtk<MauiGtkRadioButton>(nameof(IRadioButton.StrokeThickness), (h, v, w) => GtkButtonStyle.UpdateStroke(w, v))
				.MapGtk<MauiGtkRadioButton>(nameof(IRadioButton.CornerRadius), (h, v, w) => GtkButtonStyle.UpdateCornerRadius(w, v))
				.Build();

		internal static readonly CommandMapper<IRadioButton, IRadioButtonHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IRadioButton, IRadioButtonHandler>(RadioButtonHandler.CommandMapper).AddViewCommands().Build();
	}

	/// <summary>A <c>Gtk.Button</c> that shows a picture, for <c>ImageButton</c>.</summary>
	public sealed class MauiGtkImageButton : global::Gtk.Button
	{
		readonly global::Gtk.Picture _picture = new() { CanShrink = true, ContentFit = global::Gtk.ContentFit.Contain };

		/// <summary>Creates the button.</summary>
		public MauiGtkImageButton()
		{
			Child = _picture;
		}

		/// <summary>The picture shown inside the button.</summary>
		public global::Gtk.Picture Picture => _picture;
	}

	/// <summary>
	/// <see cref="ImageButtonHandler"/> over a <see cref="MauiGtkImageButton"/>: the image source is decoded by <see cref="GtkImageLoader"/>, <c>Aspect</c>
	/// maps to the picture's content fit, <c>Clicked</c>/<c>Pressed</c>/<c>Released</c> come from GTK, the stroke and corner radius are CSS.
	/// </summary>
	public class GtkImageButtonHandler : ImageButtonHandler
	{
		readonly GtkImageLoader _loader = new();
		global::Gtk.GestureClick? _gesture;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkImageButtonHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkImageButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkImageButton();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not MauiGtkImageButton button)
				return;

			button.Clicked += OnClicked;

			_gesture = new global::Gtk.GestureClick { PropagationPhase = global::Gtk.PropagationPhase.Capture };
			_gesture.Pressed += OnPressed;
			_gesture.Released += OnReleased;
			button.AddController(_gesture);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkImageButton button)
			{
				button.Clicked -= OnClicked;
				if (_gesture is not null)
				{
					_gesture.Pressed -= OnPressed;
					_gesture.Released -= OnReleased;
					button.RemoveController(_gesture);
					_gesture = null;
				}
			}

			_loader.Reset();
			base.DisconnectHandler(platformView);
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IImageButton? View => ((IElementHandler)this).VirtualView as IImageButton;

		void OnClicked(object? sender, EventArgs e) => View?.Clicked();

		void OnPressed(object o, global::Gtk.PressedArgs args) => View?.Pressed();

		void OnReleased(object o, global::Gtk.ReleasedArgs args) => View?.Released();

		internal static readonly IPropertyMapper<IImageButton, IImageButtonHandler> GtkMapper =
			new GtkMapperBuilder<IImageButton, IImageButtonHandler>(ImageButtonHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkImageButton>(nameof(IImage.Aspect), (h, v, w) => w.Picture.ContentFit = GtkImageHandler.ToContentFit(v.Aspect))
				.MapGtk<MauiGtkImageButton>(nameof(IImage.IsAnimationPlaying), (h, v, w) => { /* images are static */ })
				.MapGtk<MauiGtkImageButton>(nameof(IImage.Source), (h, v, w) =>
				{
					if (h is GtkImageButtonHandler handler)
						_ = handler._loader.UpdateAsync(h, v, texture => w.Picture.Paintable = texture);
				})
				.MapGtk<MauiGtkImageButton>(nameof(IImageButton.Padding), (h, v, w) => GtkTextStyle.SetPadding(w, v.Padding))
				.MapGtk<MauiGtkImageButton>(nameof(IButtonStroke.StrokeThickness), (h, v, w) => GtkButtonStyle.UpdateStroke(w, v))
				.MapGtk<MauiGtkImageButton>(nameof(IButtonStroke.StrokeColor), (h, v, w) => GtkButtonStyle.UpdateStroke(w, v))
				.MapGtk<MauiGtkImageButton>(nameof(IButtonStroke.CornerRadius), (h, v, w) => GtkButtonStyle.UpdateCornerRadius(w, v))
				// Explicit sizes are applied by the layout (GetDesiredSize/PlatformArrange), not by the widget.
				.MapGtk<MauiGtkImageButton>(nameof(IView.Width), (h, v, w) => { })
				.MapGtk<MauiGtkImageButton>(nameof(IView.Height), (h, v, w) => { })
				.Build();

		internal static readonly CommandMapper<IImageButton, IImageButtonHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IImageButton, IImageButtonHandler>(ImageButtonHandler.CommandMapper).AddViewCommands().Build();
	}
}
