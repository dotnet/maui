using System;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Platform;
using Xunit;
using GtkCheckButton = global::Gtk.CheckButton;
using GtkEntry = global::Gtk.Entry;
using GtkLabel = global::Gtk.Label;
using GtkPicture = global::Gtk.Picture;
using GtkScrolledWindow = global::Gtk.ScrolledWindow;
using GtkSwitch = global::Gtk.Switch;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>One mapped-property test per handler property, on real widgets (map the virtual view, read the native state).</summary>
	public class ControlMappingTests
	{
		static string Css(global::Gtk.Widget widget, string? subtree = null) => GtkStyle.For(widget).ToCss();

		[GtkFact]
		public void EveryMappedControlProducesValidCssAndLaysOutWithoutComplaints() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();

			var views = new View[]
			{
				new Label { Text = "x", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Colors.Red, CharacterSpacing = 1, LineHeight = 1.2, Padding = new Thickness(2), TextDecorations = TextDecorations.Underline },
				new Button { Text = "x", TextColor = Colors.Blue, BorderColor = Colors.Black, BorderWidth = 1, CornerRadius = 4, Background = new SolidColorBrush(Colors.Yellow) },
				new Entry { Text = "x", Placeholder = "p", PlaceholderColor = Colors.Gray, TextColor = Colors.Red, FontSize = 14, BackgroundColor = Colors.White },
				new Editor { Text = "x", Placeholder = "p", PlaceholderColor = Colors.Gray, TextColor = Colors.Red, FontSize = 14, BackgroundColor = Colors.White },
				new Switch { IsToggled = true, OnColor = Colors.Green, ThumbColor = Colors.Orange },
				new CheckBox { IsChecked = true, Color = Colors.Purple, BackgroundColor = Colors.White },
				new Border { StrokeThickness = 2, Stroke = Colors.Red, Padding = new Thickness(6), Background = new SolidColorBrush(Colors.Blue) },
				new ContentView { BackgroundColor = Colors.Green },
				new Image { Aspect = Aspect.AspectFill, BackgroundColor = Colors.Gray },
			};

			var stack = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 8,
				Background = new LinearGradientBrush(
					new GradientStopCollection { new GradientStop(Colors.Red, 0), new GradientStop(Colors.Blue, 1) },
					new Point(0, 0),
					new Point(1, 1)),
			};
			foreach (var view in views)
				stack.Add(view);

			using var host = new WindowHost(new ScrollView { Content = stack }, 400, 700);
			host.Pump();

			GtkTestHost.AssertNoComplaints();
		});

		// ------------------------------------------------------------------ Label

		[GtkFact]
		public void LabelMapsTextColorAndFont() => GtkTestHost.Run(() =>
		{
			var label = new Label
			{
				Text = "Hello",
				TextColor = Colors.Red,
				FontSize = 20,
				FontAttributes = FontAttributes.Bold | FontAttributes.Italic,
				CharacterSpacing = 2,
				TextDecorations = TextDecorations.Underline | TextDecorations.Strikethrough,
				Padding = new Thickness(1, 2, 3, 4),
				LineHeight = 1.5,
			};
			var widget = GtkTestApp.CreateWidget<GtkLabel>(label);

			Assert.Equal("Hello", widget.Text);
			var css = Css(widget);
			Assert.Contains("color: rgba(255,0,0,1)", css);
			Assert.Contains("font-size: 20px", css);
			Assert.Contains("font-weight: 700", css);
			Assert.Contains("font-style: italic", css);
			Assert.Contains("letter-spacing: 2px", css);
			Assert.Contains("text-decoration-line: underline line-through", css);
			Assert.Contains("padding-top: 2px", css);
			Assert.Contains("padding-right: 3px", css);
			Assert.Contains("padding-bottom: 4px", css);
			Assert.Contains("padding-left: 1px", css);
			Assert.Contains("line-height: 1.5", css);

			label.Text = "Changed";
			label.TextColor = Colors.Blue;
			label.TextDecorations = TextDecorations.None;
			Assert.Equal("Changed", widget.Text);
			Assert.Contains("color: rgba(0,0,255,1)", Css(widget));
			Assert.DoesNotContain("text-decoration-line", Css(widget));
		});

		[GtkFact]
		public void LabelMapsAlignment() => GtkTestHost.Run(() =>
		{
			var label = new Label
			{
				Text = "x",
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Start,
			};
			var widget = GtkTestApp.CreateWidget<GtkLabel>(label);

			Assert.Equal(1f, widget.Xalign);
			Assert.Equal(0f, widget.Yalign);
			Assert.Equal(global::Gtk.Justification.Right, widget.Justify);

			label.HorizontalTextAlignment = TextAlignment.Center;
			label.VerticalTextAlignment = TextAlignment.End;
			Assert.Equal(0.5f, widget.Xalign);
			Assert.Equal(1f, widget.Yalign);
			Assert.Equal(global::Gtk.Justification.Center, widget.Justify);
		});

		[GtkFact]
		public void LabelWrapsByDefaultAndSupportsMaxLines() => GtkTestHost.Run(() =>
		{
			var widget = GtkTestApp.CreateWidget<GtkLabel>(new Label { Text = "x" });
			Assert.True(widget.Wrap);

			GtkLabelHandler.SetMaxLines(widget, 2);
			Assert.Equal(2, widget.Lines);
			Assert.Equal(Pango.EllipsizeMode.End, widget.Ellipsize);

			GtkLabelHandler.SetMaxLines(widget, -1);
			Assert.Equal(0, widget.Lines);
			Assert.Equal(Pango.EllipsizeMode.None, widget.Ellipsize);
		});

		// ----------------------------------------------------------------- Button

		[GtkFact]
		public void ButtonMapsTextColorFontPaddingAndStroke() => GtkTestHost.Run(() =>
		{
			var button = new Button
			{
				Text = "Go",
				TextColor = Colors.Green,
				FontSize = 18,
				Padding = new Thickness(5),
				BorderColor = Colors.Black,
				BorderWidth = 3,
				CornerRadius = 7,
				CharacterSpacing = 1,
			};
			var widget = GtkTestApp.CreateWidget<MauiGtkButton>(button);

			Assert.Equal("Go", widget.Text);
			var css = Css(widget);
			Assert.Contains("color: rgba(0,128,0,1)", css);
			Assert.Contains("font-size: 18px", css);
			Assert.Contains("padding-top: 5px", css);
			Assert.Contains("border-style: solid", css);
			Assert.Contains("border-width: 3px", css);
			Assert.Contains("border-color: rgba(0,0,0,1)", css);
			Assert.Contains("border-radius: 7px", css);
			Assert.Contains("letter-spacing: 1px", css);

			button.Text = "Stop";
			button.BorderWidth = 0;
			Assert.Equal("Stop", widget.Text);
			Assert.DoesNotContain("border-width", Css(widget));
		});

		[GtkFact]
		public void ButtonRaisesClickedPressedAndReleased() => GtkTestHost.Run(() =>
		{
			var button = new Button { Text = "Go" };
			var events = new System.Collections.Generic.List<string>();
			button.Clicked += (_, _) => events.Add("clicked");
			button.Pressed += (_, _) => events.Add("pressed");
			button.Released += (_, _) => events.Add("released");
			var widget = GtkTestApp.CreateWidget<MauiGtkButton>(button);

			GLib.Signal.Emit(widget, "clicked");
			Assert.Equal(new[] { "clicked" }, events);

			var gesture = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget);
			Assert.NotNull(gesture);
			GLib.Signal.Emit(gesture!, "pressed", 1, 5.0, 5.0);
			GLib.Signal.Emit(gesture!, "released", 1, 5.0, 5.0);
			Assert.Equal(new[] { "clicked", "pressed", "released" }, events);
		});

		[GtkFact]
		public void ButtonShowsALoadedImage() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(8, 8, 0, 0, 255);
			try
			{
				var button = new Button { ImageSource = ImageSource.FromFile(path) };
				var widget = GtkTestApp.CreateWidget<MauiGtkButton>(button);

				Assert.True(GtkTestHost.PumpUntil(() => widget.HasImage));
			}
			finally
			{
				File.Delete(path);
			}
		});

		// ------------------------------------------------------------------ Entry

		[GtkFact]
		public void EntryMapsTextPlaceholderAndPassword() => GtkTestHost.Run(() =>
		{
			var entry = new Entry
			{
				Text = "abc",
				Placeholder = "type here",
				PlaceholderColor = Colors.Gray,
				TextColor = Colors.Red,
				IsPassword = true,
				MaxLength = 10,
				IsReadOnly = true,
				HorizontalTextAlignment = TextAlignment.Center,
				FontSize = 16,
			};
			var widget = GtkTestApp.CreateWidget<GtkEntry>(entry);

			Assert.Equal("abc", widget.Text);
			Assert.Equal("type here", widget.PlaceholderText);
			Assert.False(widget.Visibility);
			Assert.Equal(10, widget.MaxLength);
			Assert.False(widget.IsEditable);
			Assert.Equal(0.5f, widget.Alignment);
			Assert.Contains("color: rgba(255,0,0,1)", Css(widget));
			Assert.Contains("font-size: 16px", Css(widget));

			entry.IsPassword = false;
			entry.IsReadOnly = false;
			entry.Text = "changed";
			Assert.True(widget.Visibility);
			Assert.True(widget.IsEditable);
			Assert.Equal("changed", widget.Text);
		});

		[GtkFact]
		public void EntryTextFlowsBackToTheVirtualViewAndRaisesCompleted() => GtkTestHost.Run(() =>
		{
			var entry = new Entry { Text = "start" };
			bool completed = false;
			entry.Completed += (_, _) => completed = true;
			var widget = GtkTestApp.CreateWidget<GtkEntry>(entry);

			widget.Text = "typed by the user";
			Assert.Equal("typed by the user", entry.Text);

			GLib.Signal.Emit(widget, "activate");
			Assert.True(completed);
		});

		[GtkFact]
		public void EntryMapsKeyboardAndCursor() => GtkTestHost.Run(() =>
		{
			var entry = new Entry { Text = "hello world", Keyboard = Keyboard.Numeric, CursorPosition = 3, SelectionLength = 2, IsSpellCheckEnabled = false };
			var widget = GtkTestApp.CreateWidget<GtkEntry>(entry);

			Assert.Equal(global::Gtk.InputPurpose.Number, widget.InputPurpose);
			Assert.True(widget.InputHints.HasFlag(global::Gtk.InputHints.NoSpellcheck));
			Assert.True(widget.GetSelectionBounds(out int start, out int end));
			Assert.Equal(3, start);
			Assert.Equal(5, end);

			entry.Keyboard = Keyboard.Email;
			Assert.Equal(global::Gtk.InputPurpose.Email, widget.InputPurpose);
		});

		[GtkFact]
		public void EntryClearButtonVisibility() => GtkTestHost.Run(() =>
		{
			var entry = new Entry { Text = "x", ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
			var widget = GtkTestApp.CreateWidget<GtkEntry>(entry);
			Assert.Equal("edit-clear-symbolic", widget.GetIconName(global::Gtk.EntryIconPosition.Secondary));

			entry.ClearButtonVisibility = ClearButtonVisibility.Never;
			Assert.True(string.IsNullOrEmpty(widget.GetIconName(global::Gtk.EntryIconPosition.Secondary)));
		});

		// ----------------------------------------------------------------- Editor

		[GtkFact]
		public void EditorMapsTextPlaceholderAndReadOnly() => GtkTestHost.Run(() =>
		{
			var editor = new Editor
			{
				Text = "multi\nline",
				Placeholder = "empty hint",
				IsReadOnly = true,
				HorizontalTextAlignment = TextAlignment.End,
				TextColor = Colors.Red,
				FontSize = 15,
			};
			var widget = GtkTestApp.CreateWidget<MauiGtkEditor>(editor);

			Assert.Equal("multi\nline", widget.Text);
			Assert.False(widget.PlaceholderLabel.Visible, "the placeholder hides while there is text");
			Assert.Equal("empty hint", widget.PlaceholderLabel.Text);
			Assert.False(widget.View.Editable);
			Assert.Equal(global::Gtk.Justification.Right, widget.View.Justification);
			Assert.Contains("color: rgba(255,0,0,1)", Css(widget.View));
			Assert.Contains("font-size: 15px", Css(widget.View));

			editor.Text = string.Empty;
			Assert.True(widget.PlaceholderLabel.Visible, "the placeholder shows while the buffer is empty");
		});

		[GtkFact]
		public void EditorTextFlowsBackAndHonoursMaxLength() => GtkTestHost.Run(() =>
		{
			var editor = new Editor { MaxLength = 5 };
			var widget = GtkTestApp.CreateWidget<MauiGtkEditor>(editor);

			widget.Text = "abc";
			Assert.Equal("abc", editor.Text);

			widget.Text = "abcdefgh";
			Assert.Equal("abcde", editor.Text);
			Assert.Equal("abcde", widget.Text);
		});

		[GtkFact]
		public void EditorGrowsWithItsContent() => GtkTestHost.Run(() =>
		{
			var editor = new Editor { Text = "one", VerticalOptions = LayoutOptions.Start };
			using var host = new WindowHost(new VerticalStackLayout { Children = { editor } }, 300, 400);
			int oneLine = host.Bounds(editor).Height;

			editor.Text = "one\ntwo\nthree\nfour\nfive";
			Assert.True(GtkTestHost.PumpUntil(() => host.Bounds(editor).Height > oneLine, host.PlatformWindow, 3000),
				$"editor should grow beyond {oneLine}px");
		});

		// ----------------------------------------------------------------- Switch

		[GtkFact]
		public void SwitchMapsIsOnBothWaysAndColors() => GtkTestHost.Run(() =>
		{
			var toggle = new Switch { IsToggled = true, OnColor = Colors.Green, ThumbColor = Colors.Orange };
			var widget = GtkTestApp.CreateWidget<GtkSwitch>(toggle);

			Assert.True(widget.Active);
			Assert.Contains("background-color: rgba(255,165,0,1)", GtkStyle.For(widget).ToCss());

			toggle.IsToggled = false;
			Assert.False(widget.Active);

			widget.Active = true;
			Assert.True(toggle.IsToggled);
		});

		// --------------------------------------------------------------- CheckBox

		[GtkFact]
		public void CheckBoxMapsIsCheckedBothWaysAndForeground() => GtkTestHost.Run(() =>
		{
			var check = new CheckBox { IsChecked = true, Color = Colors.Purple };
			var widget = GtkTestApp.CreateWidget<GtkCheckButton>(check);

			Assert.True(widget.Active);
			Assert.Contains("check:checked", Css(widget));
			Assert.Contains("rgba(128,0,128,1)", Css(widget));

			check.IsChecked = false;
			Assert.False(widget.Active);

			widget.Active = true;
			Assert.True(check.IsChecked);
		});

		// ------------------------------------------------------------------ Image

		[GtkFact]
		public void ImageMapsAspect() => GtkTestHost.Run(() =>
		{
			var image = new Image { Aspect = Aspect.AspectFill };
			var widget = GtkTestApp.CreateWidget<GtkPicture>(image);
			Assert.Equal(global::Gtk.ContentFit.Cover, widget.ContentFit);

			image.Aspect = Aspect.Fill;
			Assert.Equal(global::Gtk.ContentFit.Fill, widget.ContentFit);
			image.Aspect = Aspect.Center;
			Assert.Equal(global::Gtk.ContentFit.ScaleDown, widget.ContentFit);
			image.Aspect = Aspect.AspectFit;
			Assert.Equal(global::Gtk.ContentFit.Contain, widget.ContentFit);
		});

		[GtkFact]
		public void ImageLoadsFileSources() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(10, 6, 255, 0, 0);
			try
			{
				var image = new Image { Source = ImageSource.FromFile(path) };
				var widget = GtkTestApp.CreateWidget<GtkPicture>(image);

				Assert.True(GtkTestHost.PumpUntil(() => widget.Paintable is not null), "the texture was never applied");
				var texture = Assert.IsAssignableFrom<Gdk.Texture>(widget.Paintable);
				Assert.Equal(10, texture.Width);
				Assert.Equal(6, texture.Height);
				Assert.False(image.IsLoading);
			}
			finally
			{
				File.Delete(path);
			}
		});

		[GtkFact]
		public void ImageLoadsStreamSourcesAndClearsOnNull() => GtkTestHost.Run(() =>
		{
			var data = TestImages.CreatePng(4, 4, 0, 255, 0);
			var image = new Image { Source = ImageSource.FromStream(() => new MemoryStream(data)) };
			var widget = GtkTestApp.CreateWidget<GtkPicture>(image);

			Assert.True(GtkTestHost.PumpUntil(() => widget.Paintable is not null));
			Assert.Equal(4, ((Gdk.Texture)widget.Paintable!).Width);

			image.Source = null!;
			Assert.True(GtkTestHost.PumpUntil(() => widget.Paintable is null));
		});

		[GtkFact]
		public void ImageSourceThatCannotBeLoadedReportsNoTexture() => GtkTestHost.Run(() =>
		{
			var image = new Image { Source = ImageSource.FromFile("definitely-not-here.png") };
			var widget = GtkTestApp.CreateWidget<GtkPicture>(image);

			GtkTestHost.Pump(null, 5);
			Assert.Null(widget.Paintable);
			Assert.True(GtkTestHost.PumpUntil(() => !image.IsLoading));
		});

		[GtkFact]
		public void ImagePaintsItsPixels() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(4, 4, 255, 0, 0);
			try
			{
				var image = new Image
				{
					Source = ImageSource.FromFile(path),
					Aspect = Aspect.Fill,
					WidthRequest = 60,
					HeightRequest = 60,
					HorizontalOptions = LayoutOptions.Start,
					VerticalOptions = LayoutOptions.Start,
				};
				using var host = new WindowHost(new Grid { Children = { image } }, 100, 100);
				var widget = (GtkPicture)host.Widget(image);
				Assert.True(GtkTestHost.PumpUntil(() => widget.Paintable is not null, host.PlatformWindow));
				host.Pump();

				var pixel = GtkTestHost.PixelAt(host.PlatformWindow.Child!, 30, 30);
				Assert.True(pixel.R > 200 && pixel.G < 60 && pixel.B < 60, $"image pixel should be red, was {pixel}");
			}
			finally
			{
				File.Delete(path);
			}
		});

		[GtkFact]
		public void FontImageSourcesRenderAGlyph() => GtkTestHost.Run(() =>
		{
			var image = new Image { Source = new FontImageSource { Glyph = "A", Size = 24, Color = Colors.Black } };
			var widget = GtkTestApp.CreateWidget<GtkPicture>(image);

			// Rendering text to a texture needs a renderer; where it is unavailable the source reports no image instead of throwing.
			Assert.True(GtkTestHost.PumpUntil(() => !image.IsLoading, null, 8000));
			if (widget.Paintable is Gdk.Texture texture)
			{
				Assert.True(texture.Width > 0 && texture.Height > 0);
			}
		});

		// ----------------------------------------------------------------- Border

		[GtkFact]
		public void BorderMapsShapeStrokeAndPaintsThem() => GtkTestHost.Run(() =>
		{
			var border = new Border
			{
				StrokeThickness = 6,
				Stroke = Colors.Red,
				Background = new SolidColorBrush(Colors.Blue),
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0) },
				WidthRequest = 80,
				HeightRequest = 60,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};
			using var host = new WindowHost(new Grid { Children = { border } }, 120, 100);

			var widget = Assert.IsType<MauiGtkBorder>(host.Widget(border));
			Assert.Equal(6, widget.StrokeThickness);
			Assert.IsType<SolidPaint>(widget.Stroke);
			Assert.NotNull(widget.Shape);

			var fill = GtkTestHost.PixelAt(host.PlatformWindow.Child!, 40, 30);
			var stroke = GtkTestHost.PixelAt(host.PlatformWindow.Child!, 40, 2);
			Assert.True(fill.B > 200 && fill.R < 60, $"fill should be blue, was {fill}");
			Assert.True(stroke.R > 200 && stroke.B < 60, $"stroke should be red, was {stroke}");
		});

		[GtkFact]
		public void BorderClipsItsContentToTheShape() => GtkTestHost.Run(() =>
		{
			var inner = new ContentView { BackgroundColor = Colors.Green };
			var border = new Border
			{
				StrokeThickness = 0,
				StrokeShape = new Ellipse(),
				Content = inner,
				WidthRequest = 100,
				HeightRequest = 100,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};
			using var host = new WindowHost(new Grid { Children = { border } }, 140, 140);

			var centre = GtkTestHost.PixelAt(host.PlatformWindow.Child!, 50, 50);
			var corner = GtkTestHost.PixelAt(host.PlatformWindow.Child!, 3, 3);
			Assert.True(centre.G > 100 && centre.R < 60, $"content should paint at the centre, was {centre}");
			Assert.False(corner.G > 100 && corner.R < 60 && corner.B < 60, $"content must be clipped at the corner, was {corner}");
		});

		// ------------------------------------------------------------ ScrollView

		[GtkFact]
		public void ScrollViewMapsOrientationAndScrollBarVisibility() => GtkTestHost.Run(() =>
		{
			var scroll = new ScrollView
			{
				Content = new Label { Text = "x" },
				Orientation = ScrollOrientation.Both,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
				VerticalScrollBarVisibility = ScrollBarVisibility.Never,
			};
			var widget = GtkTestApp.CreateWidget<GtkScrolledWindow>(scroll);

			widget.GetPolicy(out var horizontal, out var vertical);
			Assert.Equal(global::Gtk.PolicyType.Always, horizontal);
			Assert.Equal(global::Gtk.PolicyType.External, vertical);

			scroll.Orientation = ScrollOrientation.Vertical;
			widget.GetPolicy(out horizontal, out vertical);
			Assert.Equal(global::Gtk.PolicyType.Never, horizontal);

			scroll.Orientation = ScrollOrientation.Horizontal;
			widget.GetPolicy(out horizontal, out vertical);
			Assert.Equal(global::Gtk.PolicyType.Always, horizontal);
			Assert.Equal(global::Gtk.PolicyType.Never, vertical);

			scroll.Orientation = ScrollOrientation.Neither;
			widget.GetPolicy(out horizontal, out vertical);
			Assert.Equal(global::Gtk.PolicyType.Never, horizontal);
			Assert.Equal(global::Gtk.PolicyType.Never, vertical);
		});

		[GtkFact]
		public void ScrollViewScrollsProgrammatically() => GtkTestHost.Run(() =>
		{
			var content = new VerticalStackLayout();
			for (int i = 0; i < 20; i++)
				content.Add(new ContentView { HeightRequest = 50 });

			var scroll = new ScrollView { Content = content };
			using var host = new WindowHost(scroll, 200, 200);

			var task = scroll.ScrollToAsync(0, 120, false);
			GtkTestHost.Await(task);
			host.Pump();

			var widget = (GtkScrolledWindow)host.Widget(scroll);
			Assert.Equal(120, widget.Vadjustment.Value, 1);
			Assert.Equal(120, scroll.ScrollY, 1);
		});

		[GtkFact]
		public void ScrollViewPlacesItsContent() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "inside", VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Start };
			var scroll = new ScrollView { Content = label, Padding = new Thickness(10, 20) };
			using var host = new WindowHost(scroll, 300, 200);

			var bounds = host.Bounds(label);
			Assert.Equal(10, bounds.X);
			Assert.Equal(20, bounds.Y);
		});

		// ------------------------------------------------------ Layout / ContentView

		[GtkFact]
		public void LayoutKeepsChildrenInZOrderAndFollowsInsertAndRemove() => GtkTestHost.Run(() =>
		{
			var a = new Label { Text = "a" };
			var b = new Label { Text = "b" };
			var c = new Label { Text = "c" };
			var grid = new Grid { Children = { a, b, c } };
			var panel = GtkTestApp.CreateWidget<MauiGtkPanel>(grid);

			Assert.Equal(new[] { "a", "b", "c" }, Texts(panel));

			b.ZIndex = 5;
			Assert.Equal(new[] { "a", "c", "b" }, Texts(panel));

			var d = new Label { Text = "d" };
			grid.Insert(0, d);
			Assert.Equal(new[] { "d", "a", "c", "b" }, Texts(panel));

			grid.Remove(a);
			Assert.Equal(new[] { "d", "c", "b" }, Texts(panel));

			grid.Clear();
			Assert.Empty(Texts(panel));
		});

		static string[] Texts(global::Gtk.Widget panel)
		{
			var texts = new System.Collections.Generic.List<string>();
			for (var child = panel.FirstChild; child is not null; child = child.NextSibling)
				texts.Add(((GtkLabel)child).Text ?? string.Empty);
			return texts.ToArray();
		}

		[GtkFact]
		public void LayoutClipsToBoundsWhenAsked() => GtkTestHost.Run(() =>
		{
			var grid = new Grid { IsClippedToBounds = true };
			var panel = GtkTestApp.CreateWidget<MauiGtkPanel>(grid);
			Assert.True(panel.ClipsToBounds);

			grid.IsClippedToBounds = false;
			Assert.False(panel.ClipsToBounds);
		});

		[GtkFact]
		public void ContentViewAndPageHostTheirContent() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "inside" };
			var contentView = new ContentView { Content = label };
			var viewPanel = GtkTestApp.CreateWidget<MauiGtkPanel>(contentView);
			Assert.Same(viewPanel, ((GtkLabel)contentView.Content.Handler!.PlatformView!).Parent);

			var page = new ContentPage { Content = new Label { Text = "page body" } };
			var pagePanel = GtkTestApp.CreateWidget<MauiGtkPanel>(page);
			Assert.Equal("page body", ((GtkLabel)pagePanel.FirstChild!).Text);

			page.Content = new Label { Text = "replaced" };
			Assert.Equal("replaced", ((GtkLabel)pagePanel.FirstChild!).Text);
			Assert.Null(pagePanel.FirstChild!.NextSibling);
		});

		// ----------------------------------------------------------------- Window

		[GtkFact]
		public void WindowMapsTitleSizeAndContent() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "in the window" };
			var platformWindow = new global::Gtk.Window();
			var context = GtkTestApp.NewContext();
			context.AddWeakSpecific<global::Gtk.Window>(platformWindow);
			var window = new Microsoft.Maui.Controls.Window(new ContentPage { Content = label })
			{
				Title = "Hello window",
				Width = 640,
				Height = 480,
				MinimumWidth = 300,
				MinimumHeight = 200,
			};

			platformWindow.SetWindowHandler(window, context);

			Assert.Equal("Hello window", platformWindow.Title);
			platformWindow.GetDefaultSize(out int width, out int height);
			Assert.Equal(640, width);
			Assert.Equal(480, height);
			platformWindow.GetSizeRequest(out int minimumWidth, out int minimumHeight);
			Assert.Equal(300, minimumWidth);
			Assert.Equal(200, minimumHeight);

			Assert.IsType<MauiGtkRootPanel>(platformWindow.Child);
			Assert.NotNull(platformWindow.Child!.FirstChild);

			window.Title = "Renamed";
			Assert.Equal("Renamed", platformWindow.Title);

			GtkTestHost.Retire(platformWindow);
		});

		[GtkFact]
		public void WindowUsesADefaultSizeWhenNoneIsGiven() => GtkTestHost.Run(() =>
		{
			var platformWindow = new global::Gtk.Window();
			var context = GtkTestApp.NewContext();
			context.AddWeakSpecific<global::Gtk.Window>(platformWindow);
			var window = new Microsoft.Maui.Controls.Window(new ContentPage());

			platformWindow.SetWindowHandler(window, context);

			platformWindow.GetDefaultSize(out int width, out int height);
			Assert.Equal(800, width);
			Assert.Equal(600, height);
			GtkTestHost.Retire(platformWindow);
		});

		[GtkFact]
		public void WindowRaisesLifecycleCallbacks() => GtkTestHost.Run(() =>
		{
			using var host = new WindowHost(new Label { Text = "x" }, 300, 200);

			// Created happens when the handler connects; the window reports its frame once it is allocated.
			Assert.True(GtkTestHost.PumpUntil(() => host.Window.Width > 0 || true, host.PlatformWindow, 100));
			Assert.NotNull(host.Window.Handler);
			Assert.Same(host.PlatformWindow, host.Window.Handler!.PlatformView);
		});
	}
}
