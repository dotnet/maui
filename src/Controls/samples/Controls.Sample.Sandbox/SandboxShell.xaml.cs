namespace Maui.Controls.Sample;

public partial class SandboxShell : Shell
{
	public SandboxShell()
	{
		InitializeComponent();

		// Register route templates with path parameters
		Routing.RegisterRoute("product/{sku}", typeof(ProductPage));
		Routing.RegisterRoute("review", typeof(ReviewPage));
	}
}
