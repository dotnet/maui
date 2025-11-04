namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32000, "Entry crash when setting Text in TextChanged event handler on Android", PlatformAffected.Android)]
	public class IssueEntryTextChangedCrash : TestContentPage
	{
		private const string entryId = "TestEntry";
		private const string emojiEntryId = "EmojiEntry";
		private const string labelId = "TestLabel";

		protected override void Init()
		{
			var label = new Label
			{
				Text = "Delete the text in the Entry below. The app should not crash.",
				AutomationId = labelId
			};

			var entry = new Entry
			{
				Text = "0",
				Keyboard = Keyboard.Numeric,
				AutomationId = entryId
			};

			entry.TextChanged += OnEntryTextChanged;

			var emojiEntry = new Entry
			{
				Text = "👨‍👩‍👧‍👦",
				AutomationId = emojiEntryId
			};

			emojiEntry.TextChanged += OnEmojiEntryTextChanged;

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children = { label, entry, emojiEntry }
			};
		}

		void OnEntryTextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.NewTextValue))
			{
				((Entry)sender).Text = "0";
			}
		}

		void OnEmojiEntryTextChanged(object sender, TextChangedEventArgs e)
		{
			// Test with multi-codepoint emoji sequence
			if (string.IsNullOrEmpty(e.NewTextValue))
			{
				((Entry)sender).Text = "👨‍👩‍👧‍👦";
			}
		}
	}
}
