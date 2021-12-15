#nullable enable

namespace Microsoft.Maui.Controls
{
	public partial class FlyoutPage : IFlyoutView
	{
		IView IFlyoutView.Flyout => this.Flyout;
		IView IFlyoutView.Detail => this.Detail;

		Maui.FlyoutBehavior IFlyoutView.FlyoutBehavior
		{
			get
			{
				if (((IFlyoutPageController)this).ShouldShowSplitMode)
					return Maui.FlyoutBehavior.Locked;

				return Maui.FlyoutBehavior.Flyout;
			}
		}
	}
}
