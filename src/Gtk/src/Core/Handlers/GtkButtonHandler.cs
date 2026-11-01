using System;
using System.Globalization;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// A <c>Gtk.Button</c> whose child is a box with an optional image and a text label, so a MAUI button can show both.
	/// </summary>
	public class MauiGtkButton : global::Gtk.Button
	{
		readonly global::Gtk.Image _image = new() { Visible = false };
		readonly global::Gtk.Label _label = new();

		/// <summary>Creates the button.</summary>
		public MauiGtkButton()
		{
			var box = new global::Gtk.Box(global::Gtk.Orientation.Horizontal, 6)
			{
				Halign = global::Gtk.Align.Center,
				Valign = global::Gtk.Align.Center,
			};
			box.Append(_image);
			box.Append(_label);
			Child = box;
		}

		/// <summary>The text shown next to the image.</summary>
		public string Text
		{
			get => _label.Text ?? string.Empty;
			set
			{
				_label.Text = value;
				_label.Visible = !string.IsNullOrEmpty(value) || !_image.Visible;
			}
		}

		/// <summary>The embedded label (for tests and styling).</summary>
		public global::Gtk.Label TextLabel => _label;

		/// <summary>Whether an image is currently shown.</summary>
		public bool HasImage => _image.Visible;

		/// <summary>Sets (or clears) the image texture.</summary>
		/// <param name="texture">The texture, or <see langword="null"/>.</param>
		public void SetImage(Gdk.Texture? texture)
		{
			_image.Paintable = texture;
			_image.Visible = texture is not null;
			_label.Visible = !string.IsNullOrEmpty(_label.Text) || texture is null;
		}
	}

	/// <summary>
	/// <see cref="ButtonHandler"/> over a <see cref="MauiGtkButton"/>. <c>Clicked</c>, <c>Pressed</c> and <c>Released</c> come from GTK signals;
	/// the stroke becomes a CSS border and the image source is loaded with <see cref="GtkImageLoader"/>.
	/// </summary>
	public class GtkButtonHandler : ButtonHandler
	{
		readonly GtkImageLoader _imageLoader = new();
		global::Gtk.GestureClick? _gesture;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkButtonHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkButton();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not MauiGtkButton button)
				return;

			button.Clicked += OnClicked;

			// Pressed/Released are observed by a gesture in the capture phase that never claims the sequence,
			// so the button's own click handling is undisturbed.
			_gesture = new global::Gtk.GestureClick { PropagationPhase = global::Gtk.PropagationPhase.Capture };
			_gesture.Pressed += OnPressed;
			_gesture.Released += OnReleased;
			button.AddController(_gesture);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkButton button)
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

			_imageLoader.Reset();
			base.DisconnectHandler(platformView);
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IButton? View => ((IElementHandler)this).VirtualView as IButton;

		void OnClicked(object? sender, EventArgs e) => View?.Clicked();

		void OnPressed(object o, global::Gtk.PressedArgs args) => View?.Pressed();

		void OnReleased(object o, global::Gtk.ReleasedArgs args) => View?.Released();

		internal static readonly IPropertyMapper<IButton, IButtonHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IButton, IButtonHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IButton, IButtonHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IButton, IButtonHandler>(ButtonHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IButton, IButtonHandler>(ButtonHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IButton, IButtonHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IButton, IButtonHandler>(ButtonHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IButton, IButtonHandler>(ButtonHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IButton, IButtonHandler> mapper, GtkCommandMapperBuilder<IButton, IButtonHandler> commands)
		{
			mapper.MapGtk<MauiGtkButton>(nameof(IText.Text), (h, v, w) => w.Text = (v as IText)?.Text ?? string.Empty);
			mapper.MapGtk<MauiGtkButton>(nameof(ITextStyle.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, (v as ITextStyle)?.TextColor));
			mapper.MapGtk<MauiGtkButton>(nameof(ITextStyle.CharacterSpacing), (h, v, w) =>
				GtkTextStyle.SetCharacterSpacing(w, (v as ITextStyle)?.CharacterSpacing ?? 0));
			mapper.MapGtk<MauiGtkButton>(nameof(ITextStyle.Font), (h, v, w) =>
			{
				if (v is ITextStyle style)
					GtkTextStyle.SetFont(w, style.Font, GtkTextStyle.GetFontManager(h));
			});
			mapper.MapGtk<MauiGtkButton>(nameof(IButton.Background), (h, v, w) => GtkBackgroundImage.Update(h, w, v.Background));
			mapper.MapGtk<MauiGtkButton>(nameof(IButton.Padding), (h, v, w) => GtkTextStyle.SetPadding(w, v.Padding));
			mapper.MapGtk<MauiGtkButton>(nameof(IButtonStroke.StrokeThickness), (h, v, w) => UpdateStroke(w, v));
			mapper.MapGtk<MauiGtkButton>(nameof(IButtonStroke.StrokeColor), (h, v, w) => UpdateStroke(w, v));
			mapper.MapGtk<MauiGtkButton>(nameof(IButtonStroke.CornerRadius), (h, v, w) =>
				GtkStyle.For(w).Set("border-radius", v.CornerRadius >= 0 ? v.CornerRadius.ToString(CultureInfo.InvariantCulture) + "px" : null));
			mapper.MapGtk<MauiGtkButton>(nameof(IImage.Source), (h, v, w) =>
			{
				if (h is GtkButtonHandler handler)
					handler.UpdateImage(w, v as IImageSourcePart);
			});
		}

		void UpdateImage(MauiGtkButton button, IImageSourcePart? part)
		{
			_ = _imageLoader.UpdateAsync(this, part, texture => button.SetImage(texture));
		}

		static void UpdateStroke(MauiGtkButton button, IButton view)
		{
			var style = GtkStyle.For(button);
			double thickness = view.StrokeThickness;

			if (thickness > 0 && view.StrokeColor is { } color)
			{
				style.Set("border-style", "solid");
				style.Set("border-width", thickness.ToString("0.##", CultureInfo.InvariantCulture) + "px");
				style.Set("border-color", color.ToCss());
			}
			else
			{
				style.ClearPrefix("border-style");
				style.ClearPrefix("border-width");
				style.ClearPrefix("border-color");
			}
		}
	}
}
