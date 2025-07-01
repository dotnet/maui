namespace Maui.Controls.Sample;

public partial class MainPage : Window
{
	Button button;
	public MainPage()
	{
		InitializeComponent();
		button = new Button
		{
			Text = "Toggle TitleBar FlowDirection",
		};
		Page = new NavigationPage(new ContentPage
		{

			Content = button,

		});

		button.Clicked += (s, e) =>
		{
			if (titleBar.FlowDirection == FlowDirection.LeftToRight)
			{
				titleBar.FlowDirection = FlowDirection.RightToLeft;
			}
			else
			{
				titleBar.FlowDirection = FlowDirection.LeftToRight;
			}
		};
	}

	private void OnToggleFlowDirectionClicked(object sender, EventArgs e)
	{
		if (titleBar.FlowDirection == FlowDirection.LeftToRight)
		{
			titleBar.FlowDirection = FlowDirection.RightToLeft;
		}
		else
		{
			titleBar.FlowDirection = FlowDirection.LeftToRight;
		}
	}
}