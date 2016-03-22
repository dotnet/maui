using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class InputIntentGallery : ContentPage
	{
		public InputIntentGallery ()
		{
			var label = new Label {
				Text = "Custom Not Focused"
			};

			var label2 = new Label {
				Text = ""
			};

			var defaultEntry = new Entry {
				Placeholder = "Default",
				Keyboard = Keyboard.Default
			};

			defaultEntry.Completed += (sender, e) => label2.Text = "Default completed";

			var emailEntry = new Entry {
				Placeholder = "Email Input",
				Keyboard = Keyboard.Email
			};

			emailEntry.Completed += (sender, e) => label2.Text = "Email completed";

			var textEntry = new Entry {
				Placeholder = "Text Input",
				Keyboard = Keyboard.Text
			};

			textEntry.Completed += (sender, e) => label2.Text = "Text completed";

			var urlEntry = new Entry {
				Placeholder = "Url Input",
				Keyboard = Keyboard.Url
			};

			urlEntry.Completed += (sender, e) => label2.Text = "URL completed";

			var numericEntry = new Entry {
				Placeholder = "Numeric Input",
				Keyboard = Keyboard.Numeric
			};

			numericEntry.Completed += (sender, e) => label2.Text = "Numeric completed";

			var telephoneEntry = new Entry {
				Placeholder = "Telephone Input",
				Keyboard = Keyboard.Telephone
			};

			telephoneEntry.Completed += (sender, e) => label2.Text = "Telephone completed";

			var chatEntry = new Entry {
				Placeholder = "Chat Input",
				Keyboard = Keyboard.Chat
			};

			chatEntry.Completed += (sender, e) => label2.Text = "Chat completed";

			var customEntry = new Entry {
				Placeholder = "Custom Entry",
				Keyboard = Keyboard.Create (KeyboardFlags.CapitalizeSentence)
			};

			customEntry.Completed += (sender, e) => label2.Text = "Custom completed";

			customEntry.Focused += (s, e) => {
				label.Text = "Custom Focused";
			};

			Content = new ScrollView {
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Padding = new Thickness (40, 20),
					Children = {
						label,
						label2,
						defaultEntry,
						emailEntry,
						textEntry,
						urlEntry,
						numericEntry,
						telephoneEntry,
						chatEntry,
						customEntry
					}
				}
			};
		}
	}
}
