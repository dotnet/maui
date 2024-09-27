namespace Maui.Controls.Sample;

public partial class MainWindow : Window
{
	public MainPage _page;

	public MainWindow()
	{
		InitializeComponent();
		_page = new MainPage();
		Page = _page;
	}
}