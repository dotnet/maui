namespace Maui.Controls.Sample.Issues;

public partial class Issue24741TabbedPage : TabbedPage
{
	public Issue24741TabbedPage()
	{
		InitializeComponent();
	}
}

public class Issue24741Page1 : ContentPage
{
	public Issue24741Page1()
	{
		Title = "Page 1";

		Content = new Label
		{
			Text = "Page 1"
		};
	}
}

public class Issue24741Page2 : ContentPage
{
	public Issue24741Page2()
	{
		Title = "Page 2";

		Content = new Label
		{
			Text = "Page 2"
		};
	}
}