using System;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;
using GtkSearchEntry = global::Gtk.SearchEntry;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>SearchBar, RadioButton and ImageButton: one test per mapped property, on real widgets.</summary>
	public class InputHandlerTests
	{
		static string Css(global::Gtk.Widget widget) => GtkStyle.For(widget).ToCss();

		// --------------------------------------------------------------- SearchBar

		[GtkFact]
		public void SearchBarMapsTextBothWays() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { Text = "hello" };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);
			Assert.Equal("hello", widget.Text);

			search.Text = "changed";
			Assert.Equal("changed", widget.Text);

			widget.Text = "typed";
			Assert.Equal("typed", search.Text);
		});

		[GtkFact]
		public void SearchBarRaisesSearchButtonPressedOnActivate() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar();
			int pressed = 0;
			search.SearchButtonPressed += (s, e) => pressed++;
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);

			GLib.Signal.Emit(widget, "activate");
			Assert.Equal(1, pressed);
		});

		[GtkFact]
		public void SearchBarMapsPlaceholderAndPlaceholderColor() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { Placeholder = "Search...", PlaceholderColor = Colors.Gray };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);

			Assert.Equal("Search...", widget.PlaceholderText);
			Assert.Contains(" placeholder {", Css(widget));
			Assert.Contains("color: rgba(128,128,128,1)", Css(widget));
		});

		[GtkFact]
		public void SearchBarMapsTextStyle() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { TextColor = Colors.Red, FontSize = 20, FontAttributes = FontAttributes.Italic, CharacterSpacing = 2 };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);

			var css = Css(widget);
			Assert.Contains("color: rgba(255,0,0,1)", css);
			Assert.Contains("font-size: 20px", css);
			Assert.Contains("font-style: italic", css);
			Assert.Contains("letter-spacing: 2px", css);
		});

		[GtkFact]
		public void SearchBarMapsAlignment() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Start };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);
			Assert.Equal(1f, widget.Alignment);

			search.HorizontalTextAlignment = TextAlignment.Center;
			Assert.Equal(0.5f, widget.Alignment);
		});

		[GtkFact]
		public void SearchBarMapsReadOnlyAndInputFlags() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { IsReadOnly = true, Keyboard = Keyboard.Email, IsSpellCheckEnabled = false, IsTextPredictionEnabled = true };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);

			Assert.False(widget.IsEditable);
			Assert.Equal(global::Gtk.InputPurpose.Email, widget.InputPurpose);
			Assert.True(widget.InputHints.HasFlag(global::Gtk.InputHints.NoSpellcheck));
			Assert.True(widget.InputHints.HasFlag(global::Gtk.InputHints.WordCompletion));

			search.IsReadOnly = false;
			Assert.True(widget.IsEditable);
		});

		[GtkFact]
		public void SearchBarMapsReturnTypeWithoutError() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { ReturnType = ReturnType.Done };
			Assert.NotNull(GtkTestApp.CreateWidget<GtkSearchEntry>(search));
		});

		[GtkFact]
		public void SearchBarEnforcesMaxLength() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { MaxLength = 4 };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);

			widget.Text = "abcdefgh";
			Assert.Equal("abcd", widget.Text);
			Assert.Equal("abcd", search.Text);

			search.MaxLength = 2;
			Assert.Equal("ab", widget.Text);
		});

		[GtkFact]
		public void SearchBarMapsCursorPositionAndSelectionLength() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { Text = "hello world", CursorPosition = 3, SelectionLength = 4 };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);

			Assert.True(widget.GetSelectionBounds(out int start, out int end));
			Assert.Equal(3, start);
			Assert.Equal(7, end);
		});

		[GtkFact]
		public void SearchBarMapsCancelAndSearchIconColors() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { CancelButtonColor = Colors.Red, SearchIconColor = Colors.Blue };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);

			var css = Css(widget);
			Assert.Contains(" image.right {", css);
			Assert.Contains("color: rgba(255,0,0,1)", css);
			Assert.Contains(" image.left {", css);
			Assert.Contains("color: rgba(0,0,255,1)", css);
		});

		[GtkFact]
		public void SearchBarMapsBackgroundAndIsEnabled() => GtkTestHost.Run(() =>
		{
			var search = new SearchBar { Background = new SolidColorBrush(Colors.Yellow), IsEnabled = false };
			var widget = GtkTestApp.CreateWidget<GtkSearchEntry>(search);

			Assert.Contains("background-color: rgba(255,255,0,1)", Css(widget));
			Assert.False(widget.Sensitive);
		});

		// ------------------------------------------------------------- RadioButton

		[GtkFact]
		public void RadioButtonMapsIsCheckedBothWays() => GtkTestHost.Run(() =>
		{
			var radio = new RadioButton { Content = "one", IsChecked = true };
			var widget = GtkTestApp.CreateWidget<MauiGtkRadioButton>(radio);
			Assert.True(widget.IsChecked);

			radio.IsChecked = false;
			Assert.False(widget.IsChecked);

			widget.Button.Active = true;
			Assert.True(radio.IsChecked);
		});

		[GtkFact]
		public void RadioButtonMapsStringContentAsLabel() => GtkTestHost.Run(() =>
		{
			var radio = new RadioButton { Content = "Option" };
			var widget = GtkTestApp.CreateWidget<MauiGtkRadioButton>(radio);
			Assert.Equal("Option", widget.Button.Label);

			radio.Content = "Other";
			Assert.Equal("Other", widget.Button.Label);
		});

		[GtkFact]
		public void RadioButtonMapsViewContentNextToTheIndicator() => GtkTestHost.Run(() =>
		{
			var radio = new RadioButton { Content = new Label { Text = "view" }, ControlTemplate = null };
			var widget = GtkTestApp.CreateWidget<MauiGtkRadioButton>(radio);
			Assert.IsType<global::Gtk.Label>(widget.Button.Child);
		});

		[GtkFact]
		public void RadioButtonWithATemplateHidesTheNativeIndicator() => GtkTestHost.Run(() =>
		{
			var radio = new RadioButton
			{
				Content = "templated",
				ControlTemplate = new ControlTemplate(() => new Border { Content = new Label { Text = "own glyph" } }),
			};
			var widget = GtkTestApp.CreateWidget<MauiGtkRadioButton>(radio);

			Assert.NotNull(widget.Button.Child);
			Assert.Contains(" radio {", GtkStyle.For(widget.Button).ToCss());
			Assert.Contains("min-width: 0", GtkStyle.For(widget.Button).ToCss());
		});

		[GtkFact]
		public void RadioButtonMapsTextStyle() => GtkTestHost.Run(() =>
		{
			var radio = new RadioButton { Content = "x", TextColor = Colors.Red, FontSize = 19, CharacterSpacing = 2 };
			var widget = GtkTestApp.CreateWidget<MauiGtkRadioButton>(radio);

			var css = Css(widget);
			Assert.Contains("color: rgba(255,0,0,1)", css);
			Assert.Contains("font-size: 19px", css);
			Assert.Contains("letter-spacing: 2px", css);
		});

		[GtkFact]
		public void RadioButtonMapsStrokeAndCornerRadius() => GtkTestHost.Run(() =>
		{
			var radio = new RadioButton { Content = "x", BorderColor = Colors.Blue, BorderWidth = 3, CornerRadius = 7 };
			var widget = GtkTestApp.CreateWidget<MauiGtkRadioButton>(radio);

			var css = Css(widget);
			Assert.Contains("border-color: rgba(0,0,255,1)", css);
			Assert.Contains("border-width: 3px", css);
			Assert.Contains("border-radius: 7px", css);
		});

		[GtkFact]
		public void RadioButtonMapsBackground() => GtkTestHost.Run(() =>
		{
			var radio = new RadioButton { Content = "x", Background = new SolidColorBrush(Colors.Yellow) };
			var widget = GtkTestApp.CreateWidget<MauiGtkRadioButton>(radio);
			Assert.Contains("background-color: rgba(255,255,0,1)", Css(widget));
		});

		// --------------------------------------------------------------- ImageButton

		[GtkFact]
		public void ImageButtonRaisesClickedPressedAndReleased() => GtkTestHost.Run(() =>
		{
			var button = new ImageButton();
			int clicked = 0, pressed = 0, released = 0;
			button.Clicked += (s, e) => clicked++;
			button.Pressed += (s, e) => pressed++;
			button.Released += (s, e) => released++;
			var widget = GtkTestApp.CreateWidget<MauiGtkImageButton>(button);

			GLib.Signal.Emit(widget, "clicked");
			var gesture = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!;
			GLib.Signal.Emit(gesture, "pressed", 1, 1.0, 1.0);
			GLib.Signal.Emit(gesture, "released", 1, 1.0, 1.0);

			Assert.Equal(1, clicked);
			Assert.Equal(1, pressed);
			Assert.Equal(1, released);
		});

		[GtkFact]
		public void ImageButtonMapsSource() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(8, 8, 0, 0, 255);
			try
			{
				var button = new ImageButton { Source = ImageSource.FromFile(path) };
				var widget = GtkTestApp.CreateWidget<MauiGtkImageButton>(button);

				Assert.True(GtkTestHost.PumpUntil(() => widget.Picture.Paintable is not null), "the texture was never applied");

				button.Source = null;
				Assert.True(GtkTestHost.PumpUntil(() => widget.Picture.Paintable is null));
			}
			finally
			{
				File.Delete(path);
			}
		});

		[GtkFact]
		public void ImageButtonMapsAspect() => GtkTestHost.Run(() =>
		{
			var button = new ImageButton { Aspect = Aspect.AspectFill };
			var widget = GtkTestApp.CreateWidget<MauiGtkImageButton>(button);
			Assert.Equal(global::Gtk.ContentFit.Cover, widget.Picture.ContentFit);

			button.Aspect = Aspect.Fill;
			Assert.Equal(global::Gtk.ContentFit.Fill, widget.Picture.ContentFit);
		});

		[GtkFact]
		public void ImageButtonMapsIsAnimationPlayingWithoutError() => GtkTestHost.Run(() =>
		{
			var handler = GtkTestApp.CreateHandler<GtkImageButtonHandler>(new ImageButton());
			handler.UpdateValue(nameof(IImage.IsAnimationPlaying));
			Assert.IsType<MauiGtkImageButton>(handler.PlatformView);
		});

		[GtkFact]
		public void ImageButtonMapsPadding() => GtkTestHost.Run(() =>
		{
			var button = new ImageButton { Padding = new Thickness(1, 2, 3, 4) };
			var widget = GtkTestApp.CreateWidget<MauiGtkImageButton>(button);

			var css = Css(widget);
			Assert.Contains("padding-left: 1px", css);
			Assert.Contains("padding-top: 2px", css);
			Assert.Contains("padding-right: 3px", css);
			Assert.Contains("padding-bottom: 4px", css);
		});

		[GtkFact]
		public void ImageButtonMapsStrokeAndCornerRadius() => GtkTestHost.Run(() =>
		{
			var button = new ImageButton { BorderColor = Colors.Red, BorderWidth = 2, CornerRadius = 9 };
			var widget = GtkTestApp.CreateWidget<MauiGtkImageButton>(button);

			var css = Css(widget);
			Assert.Contains("border-color: rgba(255,0,0,1)", css);
			Assert.Contains("border-width: 2px", css);
			Assert.Contains("border-radius: 9px", css);

			button.BorderWidth = 0;
			Assert.DoesNotContain("border-width", Css(widget));
		});

		[GtkFact]
		public void ImageButtonMapsBackground() => GtkTestHost.Run(() =>
		{
			var button = new ImageButton { Background = new SolidColorBrush(Colors.Yellow) };
			var widget = GtkTestApp.CreateWidget<MauiGtkImageButton>(button);
			Assert.Contains("background-color: rgba(255,255,0,1)", Css(widget));
		});

		[GtkFact]
		public void InputControlsLayOutWithoutComplaints() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();

			var stack = new VerticalStackLayout
			{
				Spacing = 6,
				Children =
				{
					new SearchBar { Placeholder = "p", Text = "t" },
					new RadioButton { Content = "r", IsChecked = true },
					new ImageButton { WidthRequest = 32, HeightRequest = 32 },
				},
			};

			using var host = new WindowHost(stack, 400, 300);
			host.Pump();

			foreach (var child in stack.Children)
				Assert.True(host.Bounds((IView)child).Height > 0, child.GetType().Name + " has no height");

			GtkTestHost.AssertNoComplaints();
		});
	}
}
