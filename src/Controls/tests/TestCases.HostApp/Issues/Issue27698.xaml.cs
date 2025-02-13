namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27698, "Editor with AutoSize=EditorAutoSizeOption.TextChanges bad performance on iOS", PlatformAffected.iOS)]
	public partial class Issue27698 : TestContentPage
	{
		int _fontSize;
		int _characterSpacing;
		
		public Issue27698()
		{
			InitializeComponent();

			_fontSize = 16;
			_characterSpacing = 2;
		}
		
		protected override void Init()
		{

		}

		void OnUpdateFontSizeButtonClicked(object sender, EventArgs e)
		{
			Test001.FontSize = Test002.FontSize =Test003.FontSize = _fontSize;
			_fontSize++;
		}

		void OnUpdateCharacterSpacingButtonClicked(object sender, EventArgs e)
		{
			Test001.CharacterSpacing = Test002.CharacterSpacing =Test003.CharacterSpacing = _characterSpacing;
			_characterSpacing++;
		}
	}
}