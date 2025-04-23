using Microsoft.Maui.Platform;
using UIKit;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19500, "[iOS] Editor is not be able to scroll if IsReadOnly is true", PlatformAffected.iOS)]
public partial class Issue19500 : TestContentPage
{
	string editorText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla porttitor mauris non ornare ultrices. Ut semper ultrices justo eget semper.Ut imperdiet dolor ut vestibulum molestie. Duis a libero ex. Etiam mi urna, lobortis sed tincidunt in, tempus eget magna. Aenean quis malesuada eros. Phasellus felis eros, condimentum et tortor sed, condimentum convallis turpis. Sed in varius metus, at auctor orci. Maecenas luctus nibh nibh, nec aliquam est fermentum in. Etiam consectetur lectus erat, sed placerat sapien rutrum eu. Suspendisse tincidunt fermentum tempor.Maecenas egestas neque nec lacinia fringilla.";

	protected override void Init()
	{
		var rootLayout = new VerticalStackLayout() { Spacing = 10, Padding = 10 };
		var descriptionLabel = new Label
		{
			Text = "This test case is to verify that the Editor is scrollable when IsReadOnly is set to true.",
			AutomationId = "descriptionLabel"
		};
		var ypositionLabel = new Label
		{
			Text = "0",
			AutomationId = "yPositionLabel"
		};

		var editor = new Editor()
		{
			HeightRequest = 100,
			IsReadOnly = true,
			AutomationId = "editor"
		};

#if MACCATALYST
		// Adding extra text to make the editor scrollable on Catalyst
		editorText += " Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla porttitor mauris non ornare ultrices. Ut semper ultrices justo eget semper. Ut imperdiet dolor ut vestibulum molestie. Duis a libero ex. Etiam mi urna, lobortis sed tincidunt in, tempus eget magna. Aenean quis malesuada eros. Phasellus felis eros, condimentum et tortor sed, condimentum convallis turpis. Sed in varius metus, at auctor orci. Maecenas luctus nibh nibh, nec aliquam est fermentum in. Etiam consectetur lectus erat, sed placerat sapien rutrum eu. Suspendisse tincidunt fermentum tempor. Maecenas egestas neque nec lacinia fringilla.";
#endif

		editor.Text = editorText;
		editor.HandlerChanged += (s, e) =>
		{
#if MACCATALYST
			var mauiTextView = editor.Handler.PlatformView as MauiTextView;
			mauiTextView.Scrolled += (s, e) =>
			{
				ypositionLabel.Text = (s as UIScrollView).ContentOffset.Y.ToString();
			};
#endif

		};
		rootLayout.Children.Add(descriptionLabel);
		rootLayout.Children.Add(editor);
		rootLayout.Children.Add(ypositionLabel);

		Content = rootLayout;
	}
}