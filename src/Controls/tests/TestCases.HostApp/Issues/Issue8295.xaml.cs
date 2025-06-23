namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 8295, "Can't Change ToolbarItem FontIconSource Glyph After Load", PlatformAffected.All)]
	public class Issue8295_NavigationPage : NavigationPage
	{
		public Issue8295_NavigationPage() : base(new Issue8295_ContentPage())
		{

		}

	}

	public partial class Issue8295_ContentPage : ContentPage
	{
		public Issue8295_ContentPage()
		{
			InitializeComponent();
			BindingContext = this;
		}

		void ChangeGlyphClicked(object sender, EventArgs e)
		{
			if (string.Compare(TbGlyph, "\uf164", StringComparison.Ordinal) != 0)
			{
				TbGlyph = "\uf164";
			}
			else
			{
				TbGlyph = "\uf165";
			}
		}

		string _tbGlypgh = "\uf164";
		public string TbGlyph
		{
			get => _tbGlypgh;
			set
			{
				_tbGlypgh = value;
				OnPropertyChanged();
			}
		}
	}
}