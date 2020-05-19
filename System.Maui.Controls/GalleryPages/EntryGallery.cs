using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class EntryGallery : ContentPage
	{
		public EntryGallery ()
		{
			var label = new Label { Text = "Enter something in Normal" };
			var label2 = new Label { Text = "No typing has happened in Normal yet" };
			var normal = new Entry { Placeholder = "Normal" };
			var password = new Entry { Placeholder = "Password" };
			var numericPassword = new Entry { Placeholder = "Numeric Password" };
			var activation = new Entry { Placeholder = "Activation" };
			var disabled = new Entry { Placeholder = "Disabled" };
			var transparent = new Entry { Placeholder = "Transparent" };

			var isFocusedlabel = new Label { 
				Text = "Focus an Entry"
			};

			var changeKeyboardType = new Entry { 
				Placeholder = "Keyboard.Default",
				Keyboard = Keyboard.Default
			};

			changeKeyboardType.Completed += (sender, e) => {
				changeKeyboardType.Placeholder = "Keyboard.Numeric";
				changeKeyboardType.Keyboard = Keyboard.Numeric;
			};

			normal.TextChanged += (s, e) => label2.Text = "You typed in normal";

			normal.Focused += (s, e) => isFocusedlabel.Text = "Normal Focused";
			normal.Completed += (s, e) => { label.Text = normal.Text; };
			password.Focused += (s, e) => isFocusedlabel.Text = "Password Focused";
			numericPassword.Focused += (s, e) => isFocusedlabel.Text = "Numeric Password Focused";
			activation.Focused += (s, e) => isFocusedlabel.Text = "Activation Focused";
			disabled.Focused += (s, e) => isFocusedlabel.Text = "Disabled Focused";
			transparent.Focused += (s, e) => isFocusedlabel.Text = "Transparent Focused";
			changeKeyboardType.Focused += (s, e) => isFocusedlabel.Text = "Keyboard.Default Focused";

			var toggleColorButton = new Button { Text = "Toggle Text Color" };
			var changeSecureButton = new Button { Text = "Toggle Secure" };
			var changePlaceholderButton = new Button { Text = "Change Placeholder" };
			var focusNormalButton = new Button { Text = "Focus First" };

			transparent.Opacity = 0.5;
			password.IsPassword = true;
			numericPassword.IsPassword = true;
			numericPassword.Keyboard = Keyboard.Numeric;
			activation.Completed += (sender, e) => activation.Text = "Activated";
			disabled.IsEnabled = false;

			toggleColorButton.Clicked += (sender, e) => {
				if (normal.TextColor == Color.Default) {
					normal.TextColor = Color.Red;
					password.TextColor = Color.Red;
					numericPassword.TextColor = Color.Red;
				} else {
					normal.TextColor = Color.Default;
					password.TextColor = Color.Default;
					numericPassword.TextColor = Color.Default;
				}
			};

			changeSecureButton.Clicked += (sender, e) => { 
				password.IsPassword = !password.IsPassword;
				numericPassword.IsPassword = !numericPassword.IsPassword;			
			};

			int i = 1;
			changePlaceholderButton.Clicked += (sender, e) => { normal.Placeholder = "Placeholder " + i++.ToString (); };

			focusNormalButton.Clicked += (sender, args) => normal.Focus ();

			Content = new ScrollView {
				Content = new StackLayout {
					Padding = new Thickness (80),
					Children = {
						label,
						label2,
						normal,
						password,
						numericPassword,
						disabled,
						isFocusedlabel,
						activation,
						transparent,
						changeKeyboardType,
						toggleColorButton,
						changeSecureButton,
						changePlaceholderButton,
						focusNormalButton,
					}
				}
			};
		}
	}
}
