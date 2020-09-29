using System;
using Xamarin.Forms.Platform.Tizen.Renderers;

namespace Xamarin.Forms.Platform.Tizen
{
	public class FlyoutPageRenderer : VisualElementRenderer<FlyoutPage>
	{
		Native.FlyoutPage _flyoutPage;
		FlyoutContainer _flyoutContainer = null;
		FlyoutContainer _detailContainer = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public FlyoutPageRenderer()
		{
			RegisterPropertyHandler(nameof(Element.Flyout), UpdateFlyout);
			RegisterPropertyHandler(nameof(Element.Detail), UpdateDetail);
			RegisterPropertyHandler(FlyoutPage.IsPresentedProperty, UpdateIsPresented);
			RegisterPropertyHandler(FlyoutPage.FlyoutLayoutBehaviorProperty, UpdateFlyoutLayoutBehavior);
			RegisterPropertyHandler(FlyoutPage.IsGestureEnabledProperty, UpdateIsGestureEnabled);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<FlyoutPage> e)
		{
			if (_flyoutPage == null)
			{
				_flyoutPage = new Native.FlyoutPage(Forms.NativeParent)
				{
					IsPresented = e.NewElement.IsPresented,
					Flyout = _flyoutContainer = new FlyoutContainer(Element, true),
					Detail = _detailContainer = new FlyoutContainer(Element, false),
				};

				_flyoutPage.IsPresentedChanged += (sender, ev) =>
				{
					Element.IsPresented = ev.IsPresent;
				};
				_flyoutPage.UpdateIsPresentChangeable += (sender, ev) =>
				{
					(Element as IFlyoutPageController).CanChangeIsPresented = ev.CanChange;
				};
				SetNativeView(_flyoutPage);
			}

			if (e.OldElement != null)
			{
				(e.OldElement as IFlyoutPageController).BackButtonPressed -= OnBackButtonPressed;
				e.OldElement.Appearing -= OnFlyoutPageAppearing;
				e.OldElement.Disappearing -= OnFlyoutPageDisappearing;
			}

			if (e.NewElement != null)
			{
				(e.NewElement as IFlyoutPageController).BackButtonPressed += OnBackButtonPressed;
				e.NewElement.Appearing += OnFlyoutPageAppearing;
				e.NewElement.Disappearing += OnFlyoutPageDisappearing;
			}

			UpdateFlyoutLayoutBehavior();
			base.OnElementChanged(e);
		}

		void OnFlyoutPageDisappearing(object sender, EventArgs e)
		{
			_flyoutContainer?.SendDisappearing();
			_detailContainer?.SendDisappearing();
		}

		void OnFlyoutPageAppearing(object sender, EventArgs e)
		{
			_flyoutContainer?.SendAppearing();
			_detailContainer?.SendAppearing();
		}

		protected override void OnElementReady()
		{
			base.OnElementReady();
			UpdateFlyout(false);
			UpdateDetail(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_flyoutContainer != null)
				{
					_flyoutContainer.Dispose();
					_flyoutContainer = null;
				}

				if (_detailContainer != null)
				{
					_detailContainer.Dispose();
					_detailContainer = null;
				}

				if (Element != null)
				{
					Element.Appearing -= OnFlyoutPageAppearing;
					Element.Disappearing -= OnFlyoutPageDisappearing;
				}
			}

			base.Dispose(disposing);
		}

		protected void UpdateFlyoutRatio(double popoverRatio, double splitRatio)
		{
			_flyoutPage.PopoverRatio = popoverRatio;
			_flyoutPage.SplitRatio = splitRatio;
		}

		void OnBackButtonPressed(object sender, BackButtonPressedEventArgs e)
		{
			if ((Element != null) && Element.IsPresented && !_flyoutPage.IsSplit)
			{
				Element.IsPresented = false;
				e.Handled = true;
			}
		}

		void UpdateFlyoutLayoutBehavior()
		{
			_flyoutPage.FlyoutLayoutBehavior = Element.FlyoutLayoutBehavior;
		}

		void UpdateFlyout(bool isInit)
		{
			if (!isInit)
				_flyoutContainer.ChildView = Element.Flyout;
		}

		void UpdateDetail(bool isInit)
		{
			if (!isInit)
				_detailContainer.ChildView = Element.Detail;
		}

		void UpdateIsPresented()
		{
			// To update TabIndex order
			CustomFocusManager.StartReorderTabIndex();

			_flyoutPage.IsPresented = Element.IsPresented;
		}

		void UpdateIsGestureEnabled()
		{
			_flyoutPage.IsGestureEnabled = Element.IsGestureEnabled;
		}
	}
}
