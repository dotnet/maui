using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19111, "Buttons with DragGestureRecogniser doesn't fire default Command on Android", PlatformAffected.Android)]
public partial class Issue19111 : ContentPage
{

	public ICommand ButtonCommand => new Command(() => 
	{
		commandLabel.Text = "Command";
	});
	public Issue19111()
	{
		InitializeComponent();
		BindingContext = this;
	}

	private void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e)
	{
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		clickedLabel.Text = "Clicked";
	}
}