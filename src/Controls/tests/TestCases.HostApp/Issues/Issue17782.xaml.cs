using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 17782, "[ManualMauiTests] New text in the Editor character spacing test sometimes uses the previous spacing", PlatformAffected.iOS)]
	public partial class Issue17782 : ContentPage
	{

		public Issue17782()
		{
			InitializeComponent();
			if (OperatingSystem.IsIOSVersionAtLeast(26))
			{
				verticalStack.Spacing = 0;
			}
			else
			{
				verticalStack.Spacing = 10;
			}
		}

		private void OnAddEditorTextClicked(object sender, EventArgs e)
		{
			initialCharacterSpacingEditor.Text = "Initial CharacterSpacing with Text";
		}

		private void OnUpdateCharacterSpacingClicked(object sender, EventArgs e)
		{
			dynamicCharacterSpacingEditor.CharacterSpacing = 10;
		}

		private void OnResetCharacterSpacingClicked(object sender, EventArgs e)
		{
			resetCharacterSpacingEditor.CharacterSpacing = 0;
		}

		private void OnEditorsUnfocusButtonClicked(object sender, EventArgs e)
		{
			foreach (var child in verticalStack.Children)
			{
				if (child is Editor editor)
					editor.Unfocus();
			}
		}
	}
}