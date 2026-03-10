namespace Maui.Controls.Sample;

public partial class SandboxShell : Shell
{
	public SandboxShell()
	{
		InitializeComponent();
	}

	async void OnTestMenuItemClicked(object? sender, EventArgs e)
	{
		await DisplayAlertAsync("MenuItem", "Test Action was clicked!", "OK");
	}
}
