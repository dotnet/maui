#if MACCATALYST
using UIKit;
using Microsoft.Maui.Platform;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19500, "[iOS] Editor is not be able to scroll if IsReadOnly is true", PlatformAffected.iOS)]
public partial class Issue19500 : TestContentPage
{
	const string BaseEditorText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla porttitor mauris non ornare ultrices. Ut semper ultrices justo eget semper.Ut imperdiet dolor ut vestibulum molestie. Duis a libero ex. Etiam mi urna, lobortis sed tincidunt in, tempus eget magna. Aenean quis malesuada eros. Phasellus felis eros, condimentum et tortor sed, condimentum convallis turpis. Sed in varius metus, at auctor orci. Maecenas luctus nibh nibh, nec aliquam est fermentum in. Etiam consectetur lectus erat, sed placerat sapien rutrum eu. Suspendisse tincidunt fermentum tempor.Maecenas egestas neque nec lacinia fringilla.";

	protected override void Init()
	{
		var rootLayout = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 10
		};

		var descriptionLabel = CreateLabel(
				"This test case is to verify that the Editor is scrollable when IsReadOnly is set to true.",
				"descriptionLabel");

		var yPositionLabel = CreateLabel("0", "yPositionLabel");

		var editor = CreateEditor(yPositionLabel);

		rootLayout.Children.Add(descriptionLabel);
		rootLayout.Children.Add(editor);
		rootLayout.Children.Add(yPositionLabel);

		Content = rootLayout;
	}

	Label CreateLabel(string text, string automationId) =>
		new Label
		{
			Text = text,
			AutomationId = automationId
		};

	Editor CreateEditor(Label yPositionLabel)
	{
		var editor = new Editor
		{
			HeightRequest = 100,
			IsReadOnly = true,
			AutomationId = "editor",
			Text = GetEditorText()
		};

#if MACCATALYST
		editor.HandlerChanged += (_, _) =>
		{
			if (editor.Handler?.PlatformView is MauiTextView mauiTextView)
			{
				mauiTextView.Scrolled += (s, e) =>
				{
					if (s is UIScrollView scrollView)
					{
						yPositionLabel.Text = scrollView.ContentOffset.Y.ToString();
					}
				};
			}
		};
#endif

		return editor;
	}

	private string GetEditorText()
	{
#if MACCATALYST
		return BaseEditorText + " " + BaseEditorText;
#else
		return BaseEditorText;
#endif
	}
}