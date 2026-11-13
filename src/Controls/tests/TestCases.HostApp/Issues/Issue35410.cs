namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35410, "ScrollView content is obscured by the notch after rotating to landscape", PlatformAffected.iOS)]
public class Issue35410 : NavigationPage
{
	public Issue35410()
		: base(new Issue35410Page())
	{
	}
}

class Issue35410Page : ContentPage
{
	public Issue35410Page()
	{
		Title = "D6";

		Content = new ScrollView
		{
			BackgroundColor = Colors.Yellow,
			Content = new VerticalStackLayout
			{
				BackgroundColor = Color.FromArgb("#FFF16C6C"),
				Children =
				{
					new Label
					{
						Text = "1. Enter the string 'hi my name is james. nice to meet you.' exactly (case-sensitive)."
					},
					new Label
					{
						Text = "2. The test fails if the editor does not display 'Hi my name is james. Nice to meet you.' exactly (case-sensitive)."
					},
					CreateEditor(KeyboardFlags.CapitalizeSentence, "Issue35410SentenceEditor"),

					new Label
					{
						Text = "1. Enter the string 'hi my name is james. nice to meet you.' exactly (case-sensitive)."
					},
					new Label
					{
						Text = "2. The test fails if the editor does not display 'Hi My Name Is James. Nice To Meet You.' exactly (case-sensitive)."
					},
					CreateEditor(KeyboardFlags.CapitalizeWord),

					new Label
					{
						Text = "1. Enter the string 'hi my name is james. nice to meet you.' exactly (case-sensitive).",
						AutomationId = "Issue35410NotchSensitiveLabel"
					},
					new Label
					{
						Text = "2. The test fails if the editor does not display 'HI MY NAME IS JAMES. NICE TO MEET YOU.' exactly (case-sensitive)."
					},
					CreateEditor(KeyboardFlags.CapitalizeCharacter),

					new Label
					{
						Text = "1. Enter the string 'hi my name is james. nice to meet you.' exactly (case-sensitive)."
					},
					new Label
					{
						Text = "2. The test fails if the editor does not display 'hi my name is james. nice to meet you.' exactly (case-sensitive)."
					},
					CreateEditor(KeyboardFlags.CapitalizeNone),

					new Label
					{
						Text = "1. Enter the string 'qweqw hi hi' exactly (case-sensitive)."
					},
					new Label
					{
						Text = "2. The test fails if there is no red underline under the string 'qweqw' to indicate a spelling error."
					},
					CreateEditor(KeyboardFlags.Spellcheck),

					new Label
					{
						Text = "1. Enter the string 'qweqw hi hi' exactly (case-sensitive)."
					},
					new Label
					{
						Text = "2. The test fails if there is no red underline under the string 'qweqw' to indicate a spelling error."
					},
					CreateEditor(KeyboardFlags.Spellcheck)
				}
			}
		};
	}

	static Editor CreateEditor(KeyboardFlags keyboardFlags, string automationId = null) =>
		new()
		{
			Keyboard = Keyboard.Create(keyboardFlags),
			AutomationId = automationId
		};
}
