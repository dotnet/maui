using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class EditorGallery : ContentPage
	{
		public EditorGallery ()
		{
			var cellTemplate = new DataTemplate (typeof (TextCell));
			cellTemplate.SetBinding (TextCell.TextProperty, new Binding ("Title"));

			var list = new ListView {
				ItemsSource = new ContentPage [] {
					new EditorGalleryPage ("Default Keyboard", Keyboard.Default),
					new EditorGalleryPage ("Chat Keyboard", Keyboard.Chat),
					new EditorGalleryPage ("Text Keyboard", Keyboard.Text),
					new EditorGalleryPage ("Url Keyboard", Keyboard.Url),
					new EditorGalleryPage ("Numeric Keyboard", Keyboard.Numeric),
					new EditableEditorPage ("Enabled", true),
					new EditableEditorPage ("Disabled", false),

				},
				ItemTemplate = cellTemplate
			};

			list.ItemSelected += (sender, arg) => {
				if (list.SelectedItem != null) {
					Navigation.PushAsync ((ContentPage)list.SelectedItem);
					list.SelectedItem = null;
				}
			};

			Content = list;
		}
	}

	internal class EditableEditorPage : ContentPage
	{
		public EditableEditorPage (string title, bool enabled)
		{
			Title = "Editable " + enabled.ToString ();
			Padding = new Thickness (20);
			var editor = new Editor {
				Text = Title,
				IsEnabled = enabled,
				HeightRequest = 75,
			};

			var disableButton = new Button {
				Text = "Disable Editor",
			};

			var enableButton = new Button {
				Text = "Enable Editor",
			};

			disableButton.Clicked += (object sender, EventArgs e) => {
				editor.IsEnabled = false;
			};

			enableButton.Clicked += (object sender, EventArgs e) => {
				editor.IsEnabled = true;
			};

			Content = new StackLayout {
				Children = { editor, disableButton, enableButton, }
			};
		}
	}

	public class EditorGalleryPage : ContentPage
	{
		public EditorGalleryPage (string title, Keyboard keyboard)
		{
			Title = title;
			BackgroundColor = Color.Red;
			Padding = new Thickness (20);

			var label = new Label {
				Text = "Nothing entered"
			};

			var label2 = new Label {
				Text = ""
			};

			var editor = new Editor {
				HeightRequest = 75,
				Keyboard = keyboard,
				Text = "PlaceHolder",
			};
					
			editor.Completed += (sender, e) => {
				label.Text = "Entered : " + editor.Text;
			};

			editor.TextChanged += (sender, e) => {
				label2.Text += "x";
			};

			var unfocus = new Button {
				Text = "Unfocus",
			};

			var focus = new Button {
				Text = "Focus",
			};
				
			unfocus.Clicked += (sender, e) => {
				editor.Unfocus();
			};

			focus.Clicked += (sender, e) => {
				editor.Focus();
			};

			Content = new StackLayout {
				Children = {
					label,
					label2,
					editor,
					focus,
					unfocus,
				}
			};
		}
	}
}
