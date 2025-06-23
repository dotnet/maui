namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Implementation of Label TextType", PlatformAffected.All)]
public class LabelTextType : TestContentPage
{
	protected override void Init()
	{
		var label = new Label
		{
			AutomationId = "TextTypeLabel",
			Text = "<h1>Hello World!</h1>"
		};

		var button = new Button
		{
			AutomationId = "ToggleTextTypeButton",
			Text = "Toggle HTML/Plain"
		};

		button.Clicked += (s, a) =>
		{
			label.TextType = label.TextType == TextType.Html ? TextType.Text : TextType.Html;
		};


		Label htmlLabel = new Label() { TextType = TextType.Html };
		Label normalLabel = new Label();
		Label nullLabel = new Label() { TextType = TextType.Html };

		Button toggle = new Button()
		{
			Text = "Toggle some more things",
			Command = new Command(() =>
			{
				htmlLabel.Text = $"<b>{DateTime.UtcNow}</b>";
				normalLabel.Text = $"<b>{DateTime.UtcNow}</b>";

				if (string.IsNullOrWhiteSpace(nullLabel.Text))
				{
					nullLabel.Text = "hi there";
				}
				else
				{
					nullLabel.Text = null;
				}
			})
		};


		var stacklayout = new StackLayout()
		{
			label,
			button,
			htmlLabel,
			normalLabel,
			nullLabel,
			toggle,
		};

		Content = stacklayout;
	}
}
