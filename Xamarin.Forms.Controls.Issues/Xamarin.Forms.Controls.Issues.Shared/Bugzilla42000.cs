using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42000, "Unable to use comma (\", \") as decimal point", PlatformAffected.Android)]
	public class Bugzilla42000 : ContentPage
	{
		public Bugzilla42000()
		{
			var instructions = new Label
			{
				Text =
					@"Change your system language settings and verify that you can type the correct decimal separator into the Entry and Editor controls below. 
If your language is set to English (United States), you should be able to type '2.5', but not '2.5.3' or '2,5'. 
If your language is set to Deutsch (Deutschland), you should be able to type '2,5', but not '2,5,3' or '2.5'. 
"
			};

			var entrylabel = new Label { Text = "Entry:" };
			var entry = new Entry { Keyboard = Keyboard.Numeric };

			var editorlabel = new Label { Text = "Editor:" };
			var editor = new Editor { Keyboard = Keyboard.Numeric };

			var customRendererInstructions = new Label
			{
				Margin = new Thickness(0, 40, 0, 0),
				Text = @"The two entries below demonstrate disabling decimal separators and negative numbers, respectively. 
In the first one, neither '.' nor ',' should be typeable.
In the second, the '-' should not be typeable."
			};

			var entryNoDecimal = new _42000NumericEntryNoDecimal { Keyboard = Keyboard.Numeric };
			var entryNoNegative = new _42000NumericEntryNoNegative { Keyboard = Keyboard.Numeric };

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					instructions,
					entrylabel,
					entry,
					editorlabel,
					editor,
					customRendererInstructions,
					entryNoDecimal,
					entryNoNegative
				}
			};
		}

		[Preserve(AllMembers = true)]
		public class _42000NumericEntryNoDecimal : Entry
		{
		}

		[Preserve(AllMembers = true)]
		public class _42000NumericEntryNoNegative : Entry
		{
		}
	}
}