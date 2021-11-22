#nullable enable

namespace Microsoft.Maui.Controls
{
	public partial class FlyoutPage : IFlyoutView, IToolbarElement
	{
		IView IFlyoutView.Flyout => this.Flyout;
		IView IFlyoutView.Detail => this.Detail;

		IToolbar IToolbarElement.Toolbar => _toolBar ??= new Toolbar(this);
		Toolbar? _toolBar;
	}
}
