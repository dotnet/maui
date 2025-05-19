namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 15649, "Updating a ControlTemplate at runtime for a Content Page is not working.", PlatformAffected.All)]
public partial class Issue15649 : ContentPage
{
	public int _positionSelected = 1;

	public int PositionSelected
	{
		set
		{
			if (_positionSelected != value)
			{
				_positionSelected = value;

				OnPropertyChanged();
			}
		}
		get => _positionSelected;
	}

	public Issue15649()
	{
		InitializeComponent();
		this.BindingContext = this;
	}
	private void Button_Clicked(object sender, EventArgs e)
	{
		PositionSelected = 2;
	}

}