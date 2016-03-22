using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.None, 0, "Default colors toggle test", PlatformAffected.All)]
	public class DefaultColorToggleTest : ContentPage
	{
		Button _buttonColorDefaultToggle;
		Button _buttonColorInitted;
		Label _labelColorDefaultToggle;
		Label _labelColorInitted;

		public DefaultColorToggleTest ()
		{
			_buttonColorDefaultToggle = new Button {
				Text = "Default Button Color"
			};

			_buttonColorInitted = new Button {
				Text = "I should be red",
				TextColor = Color.Red
			};

			_labelColorDefaultToggle = new Label {
				Text = "Default Label Color"
			};

			_labelColorInitted = new Label {
				Text = "I should be blue",
				TextColor = Color.Blue
			};

			_buttonColorDefaultToggle.Clicked += (s, e) => {
				if (_buttonColorDefaultToggle.TextColor == Color.Default) {
					_buttonColorDefaultToggle.TextColor = Color.Red;
					_buttonColorDefaultToggle.Text = "Custom Button Color";
				} else {
					_buttonColorDefaultToggle.TextColor = Color.Default;
					_buttonColorDefaultToggle.Text = "Default Button Color";
				}
				
			};

			_labelColorDefaultToggle.GestureRecognizers.Add (new TapGestureRecognizer{Command = new Command (o=>{
				if (_labelColorDefaultToggle.TextColor == Color.Default) {
					_labelColorDefaultToggle.TextColor = Color.Green;
					_labelColorDefaultToggle.Text = "Custom Label Color";
				} else {
					_labelColorDefaultToggle.TextColor = Color.Default;
					_labelColorDefaultToggle.Text = "Default Label Color";
				}
			})});

			var entryTextColorDefaultToggle = new Entry () { Text = "Default Entry Text Color" };
			var entryTextColorInit = new Entry () { Text = "Should Be Red", TextColor = Color.Red };
			var entryToggleButton = new Button () { Text = "Toggle Entry Color" };
			entryToggleButton.Clicked += (sender, args) => {
				if (entryTextColorDefaultToggle.TextColor.IsDefault) {
					entryTextColorDefaultToggle.TextColor = Color.Fuchsia;
					entryTextColorDefaultToggle.Text = "Should Be Fuchsia";
				} else {
					entryTextColorDefaultToggle.TextColor = Color.Default;
					entryTextColorDefaultToggle.Text = "Default Entry Text Color";
				}
			};

			var entryPlaceholderColorDefaultToggle = new Entry () { Placeholder = "Default Placeholder Color" };
			var entryPlaceholderColorInit = new Entry () { Placeholder = "Should Be Lime", PlaceholderColor = Color.Lime };
			var entryPlaceholderToggleButton = new Button () { Text = "Toggle Placeholder Color" };
			entryPlaceholderToggleButton.Clicked += (sender, args) => {
				if (entryPlaceholderColorDefaultToggle.PlaceholderColor.IsDefault) {
					entryPlaceholderColorDefaultToggle.PlaceholderColor = Color.Lime;
					entryPlaceholderColorDefaultToggle.Placeholder = "Should Be Lime";
				} else {
					entryPlaceholderColorDefaultToggle.PlaceholderColor = Color.Default;
					entryPlaceholderColorDefaultToggle.Placeholder = "Default Placeholder Color";
				}
			};

			var passwordColorDefaultToggle = new Entry () { IsPassword = true, Text = "Default Password Color" };
			var passwordColorInit = new Entry () { IsPassword = true, Text = "Should Be Red", TextColor = Color.Red };
			var passwordToggleButton = new Button () { Text = "Toggle Password Box (Default)" };
			passwordToggleButton.Clicked += (sender, args) => {
				if (passwordColorDefaultToggle.TextColor.IsDefault) {
					passwordColorDefaultToggle.TextColor = Color.Red;
					passwordToggleButton.Text = "Toggle Password Box (Red)";
				} else {
					passwordColorDefaultToggle.TextColor = Color.Default;
					passwordToggleButton.Text = "Toggle Password Box (Default)";
				}
			};

			var searchbarTextColorDefaultToggle = new Entry () { Text = "Default SearchBar Text Color" };
			var searchbarTextColorToggleButton = new Button () { Text = "Toggle SearchBar Color" };
			searchbarTextColorToggleButton.Clicked += (sender, args) => {
				if (searchbarTextColorDefaultToggle.TextColor.IsDefault) {
					searchbarTextColorDefaultToggle.TextColor = Color.Fuchsia;
					searchbarTextColorDefaultToggle.Text = "Should Be Fuchsia";
				} else {
					searchbarTextColorDefaultToggle.TextColor = Color.Default;
					searchbarTextColorDefaultToggle.Text = "Default SearchBar Text Color";
				}
			};

			var searchbarPlaceholderColorDefaultToggle = new Entry () { Placeholder = "Default Placeholder Color" };
			var searchbarPlaceholderToggleButton = new Button () { Text = "Toggle Placeholder Color" };
			searchbarPlaceholderToggleButton .Clicked += (sender, args) => {
				if (searchbarPlaceholderColorDefaultToggle.PlaceholderColor.IsDefault) {
					searchbarPlaceholderColorDefaultToggle.PlaceholderColor = Color.Lime;
					searchbarPlaceholderColorDefaultToggle.Placeholder = "Should Be Lime";
				} else {
					searchbarPlaceholderColorDefaultToggle.PlaceholderColor = Color.Default;
					searchbarPlaceholderColorDefaultToggle.Placeholder = "Default Placeholder Color";
				}
			};

			Title = "Test Color Toggle Page";

			Content = new ScrollView () {
				Content = new StackLayout {
					Children = {
						_buttonColorDefaultToggle,
						_buttonColorInitted,
						_labelColorDefaultToggle,
						_labelColorInitted,
						entryTextColorDefaultToggle,
						entryToggleButton,
						entryTextColorInit,
						entryPlaceholderColorDefaultToggle,
						entryPlaceholderToggleButton,
						entryPlaceholderColorInit,
						passwordColorDefaultToggle,
						passwordToggleButton,
						passwordColorInit,
						searchbarTextColorDefaultToggle,
						searchbarTextColorToggleButton,
						searchbarPlaceholderColorDefaultToggle,
						searchbarPlaceholderToggleButton 
					}
				}
			};
		}

	}
}
