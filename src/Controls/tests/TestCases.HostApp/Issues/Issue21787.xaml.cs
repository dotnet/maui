namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 21787, "[Windows] Remove workaround for label text decorations", PlatformAffected.UWP)]
	public partial class Issue21787 : ContentPage
	{
		int _counter;

		public Issue21787()
		{
			InitializeComponent();

			BindingContext = new ViewModel5354();
		}

		void OnUpdateButtonClicked(object sender, EventArgs e)
		{
			_counter++;

			if (_counter % 2 == 1)
				TestLabel.TextDecorations = TextDecorations.Underline | TextDecorations.Strikethrough;
			else
				TestLabel.TextDecorations = TextDecorations.None;
		}
	}
}