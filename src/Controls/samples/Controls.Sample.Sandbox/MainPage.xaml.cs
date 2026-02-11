namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public List<string> Pages { get; set; }
	public string SelectedPage { get; set; }

	public MainPage()
	{
		BindingContext = this;

		Pages = new List<string>
		{
			"Page 1",
			"Page 2",
			"Page 3"
		};

		SelectedPage = Pages[0];

		InitializeComponent();
	}
}