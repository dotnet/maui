namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27281, "Application freezes when Background property is set to a DynamicResource of type OnPlatform Color", PlatformAffected.All)]
	public partial class Issue27281 : ContentPage
	{
		public Issue27281()
		{
			InitializeComponent();
		}

		void OnAddDictionaryButtonClicked(object sender, EventArgs e)
		{
			Resources.MergedDictionaries.Add(new Issue27281Colors());
		}

		void OnTestButtonClicked(object sender, EventArgs e)
		{
			ResultLabel.Text = "Passed";
		}
	}
}