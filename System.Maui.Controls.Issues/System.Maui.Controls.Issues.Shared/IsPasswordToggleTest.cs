using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0, "IsPassword toggle test", PlatformAffected.All)]
	public class IsPasswordToggleTest : TestContentPage
	{
		protected override void Init ()
		{
			var entry = new Entry {
				Text = "Setec Astronomy",
				FontFamily = "Comic Sans MS",
				HorizontalTextAlignment = TextAlignment.Center,
				Keyboard = Keyboard.Chat
			};

			var label = new Label ();
			var binding = new Binding ("Text") { Source = entry };

			var otherEntry = new Entry ();
			var otherBinding = new Binding ("Text") { Source = entry, Mode = BindingMode.TwoWay };
			otherEntry.SetBinding (Entry.TextProperty, otherBinding);

			label.SetBinding (Label.TextProperty, binding);

			var explanation = new Label() {Text = @"The Text value of the entry at the top should appear in the label and entry below, regardless of whether 'IsPassword' is on. 
Changes to the value in the entry below should be reflected in the entry at the top."};

			var button = new Button { Text = "Toggle IsPassword" };
			button.Clicked += (sender, args) => { entry.IsPassword = !entry.IsPassword; };

			Content = new StackLayout {
				Children = { entry, button, explanation, label, otherEntry }
			};
		}
	}
}
