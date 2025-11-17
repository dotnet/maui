namespace Maui.Controls.Sample;

public partial class SandboxShell : Shell
{
	public SandboxShell()
	{
		InitializeComponent();
		
		// Register route for subpage navigation
		Routing.RegisterRoute("subpage", typeof(SubPage));
	}
}
