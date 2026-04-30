namespace Maui.Controls.Sample;

public partial class SandboxShell : Shell
{
	public SandboxShell()
	{
		InitializeComponent();

		// Product routes: product/{sku} with review child that has
		// a default stars value via {stars=5}
		Routing.RegisterRoute("product/{sku}", typeof(ProductPage));
		Routing.RegisterRoute("review/{stars=5}", typeof(ReviewPage));

		// Order routes: order/{orderId:int}
		Routing.RegisterRoute("order/{orderId:int}", typeof(OrderDetailPage));
	}
}
