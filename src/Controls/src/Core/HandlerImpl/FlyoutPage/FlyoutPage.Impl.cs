#nullable enable

namespace Microsoft.Maui.Controls
{
	public partial class FlyoutPage : IFlyoutView
	{
		IView IFlyoutView.Flyout => this.Flyout;
		IView IFlyoutView.Detail => this.Detail;
	}
}
