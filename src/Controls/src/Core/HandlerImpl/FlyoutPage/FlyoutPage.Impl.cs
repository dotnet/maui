#nullable enable

using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/FlyoutPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.FlyoutPage']/Docs" />
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

#if ANDROID

		const double DefaultFlyoutSize = 320;
		const double DefaultSmallFlyoutSize = 240;

		double IFlyoutView.FlyoutWidth
		{
			get
			{
				if (DeviceInfo.Idiom == DeviceIdiom.Phone)
					return -1;

				var scaledScreenSize = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
				double w = scaledScreenSize.Width;
				return w < DefaultSmallFlyoutSize ? w : (w < DefaultFlyoutSize ? DefaultSmallFlyoutSize : DefaultFlyoutSize);
			}
		}
#else
		double IFlyoutView.FlyoutWidth => -1;
#endif


		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (DeviceInfo.Idiom == DeviceIdiom.Phone)
				return;

			if (args.NewHandler == null)
			{
				DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
			}
			else if (args.OldHandler == null)
			{
				DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
			}
		}

		void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
		{
			Handler?.UpdateValue(nameof(FlyoutBehavior));
		}
	}
}
