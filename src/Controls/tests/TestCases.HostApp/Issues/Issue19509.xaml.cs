namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19509, "The entry text color property not working when the text value is bound after some time", PlatformAffected.iOS)]
	public partial class Issue19509 : ContentPage
	{
		string _text;

		public Issue19509()
		{
			InitializeComponent();

			BindingContext = this;
		}

		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged();
			}
		}

		void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			button.IsVisible = false;
			Text = "Updated text on button click";
		}
	}
}